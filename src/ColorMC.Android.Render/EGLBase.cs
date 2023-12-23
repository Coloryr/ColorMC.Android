using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using ColorMC.Android.GLRender.Bridges;
using ColorMC.Core.Utils;
using Java.IO;
using Java.Lang;
using System.Net.Sockets;
using System.Text;
using File = System.IO.File;

namespace ColorMC.Android.GLRender;

public static class EGLBase
{
    public static EGLDisplay EglDisplay;
    public static EGLContext EglContext;
    public static EGLConfig EglConfig;
    public static EGLSurface EglSurface;

    public static Socket server;

    public const string Name = "\0colormc.android.game.sock";

    public static unsafe bool EglInit(Context context, IntPtr window, RenderType type)
    {
        //string name = context.GetExternalFilesDir(null).AbsolutePath + "/" + Name;
        string name = Name;
        //if (File.Exists(name))
        //{
        //    File.Delete(name);
        //}

        //var server = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        //var endPoint = new UnixDomainSocketEndPoint(name);
        //server.Bind(endPoint);
        //server.Listen(1); // Set the backlog value as needed
        //Client();

        LocalServerSocket socket = new(name);
        Task.Run(() =>
        {
            var local = socket.Accept();
            var sr1 = new InputStreamReader(local.InputStream);
            var sr2 = new BufferedReader(sr1);
            string str;
            while ((str = sr2.ReadLine()) != null)
            {
                RenderLog.Info("Pipe", str);
            }
        });

        // 获取文件描述符的整数值
        ProcessBuilder process = new(context.ApplicationInfo.NativeLibraryDir + "/" + "libcolormcnative.so");
        process.Environment().Add("GAME_FD", name[1..]);
        //process.Environment().Add("GAME_FD_SIZE", size.ToString());
        process.Directory(context.GetExternalFilesDir(null));
        RenderLog.Info("Pipe", "Start Pipe");
        var p = process.Start();
        Task.Run(() =>
        {
            string? str;
            using var inputReader = new BufferedReader(new InputStreamReader(p.InputStream));
            while ((str = inputReader.ReadLine()) != null)
            {
                RenderLog.Info("Pipe", str);
            }
            //脚本执行异常时的输出信息
            using var errorReader = new BufferedReader(new InputStreamReader(p.ErrorStream));
            while ((str = errorReader.ReadLine()) != null)
            {
                RenderLog.Error("Pipe", str);
            }
        });

        if (type == RenderType.ANGEL)
        {
            EGL.Load("libEGL_angle.so");
        }
        else
        {
            EGL.Load("libEGL.so");
        }

        //1、获取显示设备
        EglDisplay = EGL.GetDisplay(EGL.EGL_DEFAULT_DISPLAY);
        if (EglDisplay == EGL.EGL_NO_DISPLAY)
        {
            RenderLog.Error("EGL", "eglGetDisplay error");
            return false;
        }
        // 2、 EGL初始化
        EGLint major, minor;
        if (!EGL.Initialize(EglDisplay, &major, &minor))
        {
            RenderLog.Error("EGL", "eglInitialize error");
            return false;
        }

        //3、 资源配置，例如颜色位数等
        EGLint[] attribs =
        [
            EGL.EGL_RED_SIZE,
            8,
            EGL.EGL_GREEN_SIZE,
            8,
            EGL.EGL_BLUE_SIZE,
            8,
            EGL.EGL_ALPHA_SIZE,
            8,
            EGL.EGL_DEPTH_SIZE,
            8,
            EGL.EGL_STENCIL_SIZE,
            8,
            EGL.EGL_RENDERABLE_TYPE,
            EGL.EGL_OPENGL_ES2_BIT,
            EGL.EGL_NONE
        ];

        EGLint num_config;
        EGLConfig config;
        fixed (EGLint* ptr = attribs)
        {
            if (!EGL.ChooseConfig(EglDisplay, ptr, (EGLConfig*)0, 1, &num_config))
            {
                RenderLog.Error("EGL", "eglChooseConfig  error 1");
                return false;
            }

            //4、ChooseConfig
            if (!EGL.ChooseConfig(EglDisplay, ptr, &config, num_config, &num_config))
            {
                RenderLog.Error("EGL", "eglChooseConfig  error 2");
                return false;
            }

            EglConfig = config;
        }

        if (type == RenderType.ANGEL)
        {
            EGL.BindAPI(EGL.EGL_OPENGL_API);
            RenderLog.Info("EGL", "EGLBridge: Binding to desktop OpenGL");
        }
        else
        {
            EGL.BindAPI(EGL.EGL_OPENGL_ES_API);
            RenderLog.Info("EGL", "EGLBridge: Binding to OpenGL ES");
        }

        // 5、创建上下文
        EGLint[] attrib_list =
        [
            EGL.EGL_CONTEXT_CLIENT_VERSION, 3,
            EGL.EGL_NONE
        ];

        fixed (EGLint* ptr = attrib_list)
        {
            EglContext = EGL.CreateContext(EglDisplay, EglConfig, EGL.EGL_NO_CONTEXT, ptr);
        }

        if (EglContext == EGL.EGL_NO_CONTEXT)
        {
            RenderLog.Error("EGL", "eglCreateContext  error");
            return false;
        }

        if (window != IntPtr.Zero)
        {
            //6、创建渲染的Surface
            EglSurface = EGL.CreateWindowSurface(EglDisplay, EglConfig, window, (EGLint*)0);
            if (EglSurface == EGL.EGL_NO_SURFACE)
            {
                RenderLog.Error("EGL", "eglCreateWindowSurface  error");
                return false;
            }
        }

        // 7、使用
        if (!EGL.MakeCurrent(EglDisplay, EglSurface, EglSurface, EglContext))
        {
            RenderLog.Error("EGL", "eglMakeCurrent  error");
            return false;
        }
        RenderLog.Info("EGL", "egl init success! ");
        return true;
    }

    public static void Client()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                Socket receiverSocket = await server.AcceptAsync();

                // 获取输入输出流
                var temp = new byte[1024];
                receiverSocket.ReceiveTimeout = 1;
                while (receiverSocket.Connected)
                {
                    int size = await receiverSocket.ReceiveAsync(temp);
                    if (size > 0)
                    {
                        RenderLog.Info("Pipe", Encoding.UTF8.GetString(temp, 0, size));
                    }
                }
            }
        });
    }

    public static bool SwapBuffers()
    {
        if (EglDisplay != EGL.EGL_NO_DISPLAY && EglSurface != EGL.EGL_NO_SURFACE)
        {
            if (EGL.SwapBuffers(EglDisplay, EglSurface) != 0)
            {
                return true;
            }
        }
        return false;
    }

    public static void DestroyEgl()
    {
        if (EglDisplay != EGL.EGL_NO_DISPLAY)
        {
            EGL.MakeCurrent(EglDisplay, EGL.EGL_NO_SURFACE, EGL.EGL_NO_SURFACE, EGL.EGL_NO_CONTEXT);
        }
        if (EglDisplay != EGL.EGL_NO_DISPLAY && EglSurface != EGL.EGL_NO_SURFACE)
        {
            EGL.DestroySurface(EglDisplay, EglSurface);
            EglSurface = EGL.EGL_NO_SURFACE;
        }
        if (EglDisplay != EGL.EGL_NO_DISPLAY && EglContext != EGL.EGL_NO_CONTEXT)
        {
            EGL.DestroyContext(EglDisplay, EglContext);
            EglContext = EGL.EGL_NO_CONTEXT;
        }
        if (EglDisplay != EGL.EGL_NO_DISPLAY)
        {
            EGL.Terminate(EglDisplay);
            EglDisplay = EGL.EGL_NO_DISPLAY;
        }
    }
}
