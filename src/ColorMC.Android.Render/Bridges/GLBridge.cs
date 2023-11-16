using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.Render.Bridges;

public record GLRenderWindow : BasicRenderWindow
{
    public EGLConfig config;
    public EGLint format;
    public EGLContext context;
    public EGLSurface surface;
};

public class GLBridge : IBridge<GLRenderWindow>
{
    private const string _logTag = "GLBridge";
    private GLRenderWindow? _currentBundle = null!;
    private EGLDisplay _eglDisplay;

    public unsafe bool Init()
    {
        EGL.Load();
        _eglDisplay = EGL.eglGetDisplay_p(0);
        if (_eglDisplay == IntPtr.Zero)
        {
            RenderLog.Error(_logTag, "eglGetDisplay_p(EGL_DEFAULT_DISPLAY) returned EGL_NO_DISPLAY");
            return false;
        }

        if (EGL.eglInitialize_p(_eglDisplay, (int*)0, (int*)0) != 1)
        {
            RenderLog.Error(_logTag, $"eglInitialize_p() failed: {EGL.eglGetError_p():04x}");
            return false;
        }

        return true;
    }

    public GLRenderWindow? GetCurrent()
    {
        return _currentBundle;
    }

    public unsafe GLRenderWindow? InitContext(GLRenderWindow share)
    {
        GLRenderWindow bundle = new();
        EGLint[] egl_attributes = [0x3022, 8, 0x3023, 8, 0x3024, 8, 0x3021, 8, 0x3025, 24, 0x3033, 0x0004 | 0x0001, 0x3040, 0x0004, 0x3038];
        EGLint num_configs = 0;
        fixed (int* ptr = egl_attributes)
        {
            if (EGL.eglChooseConfig_p(_eglDisplay, ptr, (EGLConfig*)0, 0, &num_configs) != 1)
            {
                RenderLog.Error(_logTag, $"eglChooseConfig_p() failed:{EGL.eglGetError_p():04x}");
                return null;
            }
            if (num_configs == 0)
            {
                RenderLog.Error(_logTag, "eglChooseConfig_p() found no matching config");
                return null;
            }

            // Get the first matching config
            EGLConfig config;
            EGL.eglChooseConfig_p(_eglDisplay, ptr, &config, 1, &num_configs);
            bundle.config = config;
            int format;
            EGL.eglGetConfigAttrib_p(_eglDisplay, bundle.config, 0x302E, &format);
            bundle.format = format;
        }
        {
            EGLBoolean bindResult;
            var render = Environment.GetEnvironmentVariable("RENDERER");
            if (render == "opengles3_desktopgl")
            {
                RenderLog.Info(_logTag, "EGLBridge: Binding to desktop OpenGL");
                bindResult = EGL.eglBindAPI_p(0x30A2);
            }
            else 
            {
                RenderLog.Info(_logTag, "EGLBridge: Binding to OpenGL ES");
                bindResult = EGL.eglBindAPI_p(0x30A0);
            }
            if (bindResult == 0)
            {
                RenderLog.Error(_logTag, $"EGLBridge: bind failed: {EGL.eglGetError_p():04x}");
            }
        }

        var version = Environment.GetEnvironmentVariable("LIBGL_ES")!;
        int libgl_es = int.Parse(version);
        if (libgl_es < 0 || libgl_es > short.MaxValue) libgl_es = 2;
        EGLint[] egl_context_attributes = { 0x3098, libgl_es, 0x3038 };
        fixed(int* ptr= egl_context_attributes)
        bundle.context = EGL.eglCreateContext_p(_eglDisplay, bundle.config, share == null ? 0 : share.context, ptr);

        if (bundle.context == 0)
        {
            RenderLog.Error(_logTag, $"eglCreateContext_p() finished with error: {EGL.eglGetError_p():04x}");
            return null;
        }
        return bundle;
    }

    public unsafe void GLSwapSurface(GLRenderWindow bundle)
    {
        if (bundle.NativeSurface != IntPtr.Zero)
        {
            NativeWindow.Release(bundle.NativeSurface);
        }
        if (bundle.surface != 0) EGL.eglDestroySurface_p(_eglDisplay, bundle.surface);
        if (bundle.NewNativeSurface != IntPtr.Zero)
        {
            RenderLog.Error(_logTag, "Switching to new native surface");
            bundle.NativeSurface = bundle.NewNativeSurface;
            bundle.NewNativeSurface = IntPtr.Zero;
            NativeWindow.Acquire(bundle.NativeSurface);
            NativeWindow.SetBuffersGeometry(bundle.NativeSurface, 0, 0, bundle.format);
            bundle.surface = EGL.eglCreateWindowSurface_p(_eglDisplay, bundle.config, bundle.NativeSurface, (int*)0);
        }
        else
        {
            RenderLog.Error(_logTag, "No new native surface, switching to 1x1 pbuffer");
            bundle.NativeSurface = IntPtr.Zero;
            EGLint[] pbuffer_attrs = [0x3057, 1, 0x3056, 1, 0x3038];
            fixed(int* ptr = pbuffer_attrs)
            bundle.surface = EGL.eglCreatePbufferSurface_p(_eglDisplay, bundle.config, ptr);
        }
    }

    public unsafe void MakeCurrent(GLRenderWindow bundle)
    {
        if (bundle == null)
        {
            if (EGL.eglMakeCurrent_p(_eglDisplay, 0, 0, 0) != 0)
            {
                _currentBundle = null;
            }
            return;
        }
        bool hasSetMainWindow = false;
        if (GameEnviron.Game.MainWindowBundle == null)
        {
            GameEnviron.Game.MainWindowBundle = bundle;
            fixed(void* temp = &GameEnviron.Game.MainWindowBundle)
            RenderLog.Info(_logTag, $"Main window bundle is now {(int)temp:x}");
            GameEnviron.Game.MainWindowBundle.NewNativeSurface = GameEnviron.Game.Window;
            hasSetMainWindow = true;
        }
        
        RenderLog.Info(_logTag, $"Making current, surface={bundle.surface:x}, nativeSurface={(nint)bundle.NativeSurface:x}, newNativeSurface={(nint)bundle.NewNativeSurface:x}");
        if (bundle.surface == 0)
        { //it likely will be on the first run
            GLSwapSurface(bundle);
        }
        if (EGL.eglMakeCurrent_p(_eglDisplay, bundle.surface, bundle.surface, bundle.context) != 0)
        {
            _currentBundle = bundle;
        }
        else
        {
            if (hasSetMainWindow)
            {
                GameEnviron.Game.MainWindowBundle.NewNativeSurface = IntPtr.Zero;
                var temp = (GameEnviron.Game.MainWindowBundle as GLRenderWindow)!;
                GLSwapSurface(temp);
                GameEnviron.Game.MainWindowBundle = null;
            }
            RenderLog.Error(_logTag, $"eglMakeCurrent returned with error: {EGL.eglGetError_p():04x}");
        }
    }

    public unsafe void SwapBuffers()
    {
        if (_currentBundle == null)
        {
            return;
        }
        if (_currentBundle.State == STATE_RENDERER.NEW_WINDOW)
        {
            EGL.eglMakeCurrent_p(_eglDisplay, 0, 0, 0); //detach everything to destroy the old EGLSurface
            GLSwapSurface(_currentBundle);
            EGL.eglMakeCurrent_p(_eglDisplay, _currentBundle.surface, _currentBundle.surface, _currentBundle.context);
            _currentBundle.State = STATE_RENDERER.ALIVE;
        }
        if (_currentBundle.surface != 0)
            if (EGL.eglSwapBuffers_p(_eglDisplay, _currentBundle.surface) != 0 && EGL.eglGetError_p() == 0x300D)
            {
                EGL.eglMakeCurrent_p(_eglDisplay, 0, 0, 0);
                _currentBundle.NewNativeSurface = IntPtr.Zero;
                GLSwapSurface(_currentBundle);
                EGL.eglMakeCurrent_p(_eglDisplay, _currentBundle.surface, _currentBundle.surface, _currentBundle.context);
                RenderLog.Info(_logTag, "The window has died, awaiting window change");
            }

    }

    public unsafe void SetupWindow()
    {
        if (GameEnviron.Game.MainWindowBundle != null)
        {
            RenderLog.Info(_logTag, "Main window bundle is not NULL, changing state");
            GameEnviron.Game.MainWindowBundle.State = STATE_RENDERER.NEW_WINDOW;
            GameEnviron.Game.MainWindowBundle.NewNativeSurface = GameEnviron.Game.Window;
        }
    }

    public void WwapInterval(int swapInterval)
    {
        if (GameEnviron.Game.force_vsync) swapInterval = 1;

        EGL.eglSwapInterval_p(_eglDisplay, swapInterval);
    }
}
