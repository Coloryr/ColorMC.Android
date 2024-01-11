using Android.Content;
using Android.Opengl;
using Android.Systems;
using Android.Util;
using Android.Views;
using Javax.Microedition.Khronos.Opengles;
using System.Diagnostics;

namespace ColorMC.Android.GLRender;

public class TestSurface : GLSurfaceView, ISurfaceHolderCallback, GLSurfaceView.IRenderer
{
    private static QuadRenderer qrender;

    private int width, height;

    public TestSurface(Context? context) : this(context, null)
    {

    }

    public TestSurface(Context? context, IAttributeSet? attributeSet) : base(context, attributeSet)
    {
        SetEGLContextClientVersion(3);
        SetRenderer(this);
    }

    public void OnDrawFrame(IGL10? gl)
    {
        GLES20.GlViewport(0, 0, width, height);
        GLES20.GlClearColor(0, 0, 0, 0);
        GLES20.GlClear(GLES20.GlColorBufferBit);

        if (HaveBuffer)
        {
            if (TexId == 0)
            {
                BindTexture();
            }
            else
            {
                qrender.DrawTexture(TexId, width, height, RenderWidth, RenderHeight, false);
            }
        }
    }

    public void OnSurfaceChanged(IGL10? gl, int width, int height)
    {
        this.width = width;
        this.height = height;

        RenderTest.Available();

        qrender = new();

        Start(Context);
    }

    public void OnSurfaceCreated(IGL10? gl, Javax.Microedition.Khronos.Egl.EGLConfig? config)
    {

    }

    public void SetSize(string? text1, string? text2)
    {
        if (ushort.TryParse(text1, out var width)
            && ushort.TryParse(text2, out var height))
        {
            HaveBuffer = false;
            HaveTexture = false;
            RenderTest.DeleteBuffer(buffer, texture);
            GLHelper.DeleteTexture(TexId);
            TexId = 0;
            Sock.SetWindowSize(width, height);
            Sock.ChangeSize();
        }
        else
        {
            Toast.MakeText(Context!, "错误的输入", ToastLength.Short).Show();
        }
    }

    private GameSock Sock;
    private string render;

    public void Start(Context context)
    {
        render = context.FilesDir.AbsolutePath + "/render.sock";
        var game = context.FilesDir.AbsolutePath + "/game.sock";
        Sock = new GameSock(game)
        {
            CommandRead = Command
        };
        var temp1 = Os.Getenv("PATH");

        //var LD_LIBRARY_PATH = context.ApplicationInfo.NativeLibraryDir + ":" + temp1
        //    + ":" + "/system/lib64";
        var LD_LIBRARY_PATH = "/system/lib64:" +  context.ApplicationInfo.NativeLibraryDir
            + ":/data/user/0/coloryr.colormc.android/files/java/jre8-x86_64-20231113-release.tar.xz/lib/amd64/jli:/data/user/0/coloryr.colormc.android/files/java/jre8-x86_64-20231113-release.tar.xz/lib/amd64:/vendor/lib64:/vendor/lib64/hw:/product/bin:/apex/com.android.runtime/bin:/apex/com.android.art/bin:/system_ext/bin:/system/bin:/system/xbin:/odm/bin:/vendor/bin:/vendor/xbin:/data/user/0/coloryr.colormc.android/files/java/jre8-x86_64-20231113-release.tar.xz/lib/amd64/server";

        // 获取文件描述符的整数值
        var info = new ProcessStartInfo(context.ApplicationInfo.NativeLibraryDir + "/" + "libcolormcnative.so")
        {
            WorkingDirectory = context.FilesDir.AbsolutePath
        };
        info.Environment.Add("GAME_SOCK", game);
        info.Environment.Add("RENDER_SOCK", render);
        info.Environment.Add("RUN_SO", "libcolormcnative_run.so");
        info.Environment.Add("GL_SO", "libgl4es_114.so");
        info.Environment.Add("EGL_SO", "libEGL.so");
        info.Environment.Add("LD_LIBRARY_PATH", LD_LIBRARY_PATH);
        info.Environment.Add("LIBGL_MIPMAP", "3");
        info.Environment.Add("LIBGL_NOERROR", "1");
        info.Environment.Add("LIBGL_NOINTOVLHACK", "1");
        info.Environment.Add("LIBGL_NORMALIZE", "1");
        info.Environment.Add("GL_ES_VERSION", "3");

        info.RedirectStandardError = true;
        info.RedirectStandardInput = true;
        info.RedirectStandardOutput = true;

        RenderLog.Info("Pipe", "Start Pipe");
        var p = new Process
        {
            StartInfo = info,
            EnableRaisingEvents = true
        };
        p.OutputDataReceived += (sender, message) =>
        {
            RenderLog.Info("Pipe", message.Data ?? "");
        };
        p.ErrorDataReceived += (sender, message) =>
        {
            RenderLog.Info("Pipe", message.Data ?? "");
        };
        p.Exited += (a, b) =>
        {
             
        };
        p.Start(); 
        p.BeginErrorReadLine();
        p.BeginOutputReadLine(); 
         
        if (!Sock.Connect())
        {
            return;

        }
        Sock.SetWindowSize(640, 480); 
        Sock.Start();
        ReadBuffer(); 
    }

    public bool HaveBuffer;
    public bool HaveTexture;

    private IntPtr buffer;
    private IntPtr texture;

    public int RenderWidth;
    public int RenderHeight;

    public int TexId;

    public void BindTexture()
    {
        if (HaveTexture || !HaveBuffer)
        {
            return;
        }
        if (TexId == 0)
        {
            TexId = GLHelper.CreateTexture();
        }
        HaveTexture = RenderTest.BindTexture(TexId, buffer, out var width, out var height, out texture);
        RenderWidth = width;
        RenderHeight = height;
    }

    private void ReadBuffer()
    {
        Task.Run(() =>
        {
            while (true)
            {
                buffer = RenderTest.GetBuffer(render);
                if (buffer != IntPtr.Zero)
                {
                    HaveBuffer = true;
                    return;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        });
    }

    private void Command(byte data)
    {
        switch (data)
        {
            case 3:
                ReadBuffer();
                break;
        }
    }
}
