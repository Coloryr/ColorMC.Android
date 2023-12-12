using System.Runtime.InteropServices;

namespace ColorMC.Android.GLRender.Bridges;

public static class EGL
{
    //EGLBoolean (*eglMakeCurrent_p) (EGLDisplay dpy, EGLSurface draw, EGLSurface read, EGLContext ctx);
    public unsafe delegate EGLBoolean eglMakeCurrent(EGLDisplay display, EGLSurface draw, EGLSurface read, EGLContext ctx);
    //EGLBoolean (*eglDestroyContext_p) (EGLDisplay dpy, EGLContext ctx);
    public delegate EGLBoolean eglDestroyContext(EGLDisplay display, EGLContext draw);
    //EGLBoolean (*eglDestroySurface_p) (EGLDisplay dpy, EGLSurface surface);
    public delegate EGLBoolean eglDestroySurface(EGLDisplay display, EGLSurface draw);
    //EGLBoolean (*EGLBoolean) (EGLDisplay dpy);
    public delegate bool eglTerminate(EGLDisplay display);
    //EGLBoolean (*eglReleaseThread_p) (void);
    public delegate EGLBoolean eglReleaseThread();
    //EGLContext (*eglGetCurrentContext_p) (void);
    public delegate EGLContext eglGetCurrentContext();
    //EGLDisplay (*eglGetDisplay_p) (NativeDisplayType display);
    public delegate EGLDisplay eglGetDisplay(NativeDisplayType display);
    //EGLBoolean (*eglInitialize_p) (EGLDisplay dpy, EGLint *major, EGLint *minor);
    public unsafe delegate EGLBoolean eglInitialize(EGLDisplay display, EGLint* major, EGLint* minor);
    //EGLBoolean (*eglChooseConfig_p) (EGLDisplay dpy, const EGLint *attrib_list, EGLConfig *configs, EGLint config_size, EGLint *num_config);
    public unsafe delegate EGLBoolean eglChooseConfig(EGLDisplay display, EGLint* attrib_list, EGLConfig* configs, EGLint config_size, EGLint* num_config);
    //EGLBoolean (*eglGetConfigAttrib_p) (EGLDisplay dpy, EGLConfig config, EGLint attribute, EGLint *value);
    public unsafe delegate EGLBoolean eglGetConfigAttrib(EGLDisplay display, EGLConfig config, EGLint attribute, EGLint* value);
    //EGLBoolean (*eglBindAPI_p) (EGLenum api);
    public unsafe delegate EGLBoolean eglBindAPI(EGLenum api);
    //EGLSurface (*eglCreatePbufferSurface_p) (EGLDisplay dpy, EGLConfig config, const EGLint *attrib_list);
    public unsafe delegate EGLSurface eglCreatePbufferSurface(EGLDisplay display, EGLConfig config, EGLint* attrib_list);
    //EGLSurface (*eglCreateWindowSurface_p) (EGLDisplay dpy, EGLConfig config, NativeWindowType window, const EGLint *attrib_list);
    public unsafe delegate EGLSurface eglCreateWindowSurface(EGLDisplay display, EGLConfig config, IntPtr window, EGLint* attrib_list);
    //EGLBoolean (*eglSwapBuffers_p) (EGLDisplay dpy, EGLSurface draw);
    public delegate EGLSurface eglSwapBuffers(EGLDisplay display, EGLSurface draw);
    //EGLint (*eglGetError_p) (void);
    public delegate EGLint eglGetError();
    //EGLContext (*eglCreateContext_p) (EGLDisplay dpy, EGLConfig config, EGLContext share_list, const EGLint *attrib_list);
    public unsafe delegate EGLContext eglCreateContext(EGLDisplay display, EGLConfig config, EGLContext share_list, EGLint* attrib_list);
    //EGLBoolean (*eglSwapInterval_p) (EGLDisplay dpy, EGLint interval);
    public unsafe delegate EGLBoolean eglSwapInterval(EGLDisplay display, EGLint interval);
    //EGLSurface (*eglGetCurrentSurface_p) (EGLint readdraw);
    public delegate EGLSurface eglGetCurrentSurface(EGLint readdraw);

    public static eglMakeCurrent MakeCurrent;
    public static eglDestroyContext DestroyContext;
    public static eglDestroySurface DestroySurface;
    public static eglTerminate Terminate;
    public static eglReleaseThread ReleaseThread;
    public static eglGetCurrentContext GetCurrentContext;
    public static eglGetDisplay GetDisplay;
    public static eglInitialize Initialize;
    public static eglChooseConfig ChooseConfig;
    public static eglGetConfigAttrib GetConfigAttrib;
    public static eglBindAPI BindAPI;
    public static eglCreatePbufferSurface CreatePbufferSurface;
    public static eglCreateWindowSurface CreateWindowSurface;
    public static eglSwapBuffers SwapBuffers;
    public static eglGetError GetError;
    public static eglCreateContext CreateContext;
    public static eglSwapInterval SwapInterval;
    public static eglGetCurrentSurface GetCurrentSurface;

    public const int EGL_DEFAULT_DISPLAY = 0;
    public const int EGL_NO_CONTEXT = 0;
    public const int EGL_NO_DISPLAY    = 0;
    public const int EGL_NO_SURFACE = 0;
    public const int EGL_OPENGL_ES2_BIT = 0x0004;
    public const int EGL_ALPHA_SIZE = 0x3021;
    public const int EGL_BLUE_SIZE = 0x3022;
    public const int EGL_GREEN_SIZE = 0x3023;
    public const int EGL_RED_SIZE = 0x3024;
    public const int EGL_DEPTH_SIZE = 0x3025;
    public const int EGL_STENCIL_SIZE = 0x3026;
    public const int EGL_NONE = 0x3038;
    public const int EGL_RENDERABLE_TYPE = 0x3040;
    public const int EGL_CONTEXT_CLIENT_VERSION = 0x3098;
    public const int EGL_OPENGL_ES_API = 0x30A0;
    public const int EGL_OPENGL_API = 0x30A2;

    public static void Load(string egl)
    {
        IntPtr dl_handle = NativeLoader.LoadLibrary(egl);

        BindAPI = Marshal.GetDelegateForFunctionPointer<eglBindAPI>(NativeLoader.GetProcAddress(dl_handle, "eglBindAPI"));
        ChooseConfig = Marshal.GetDelegateForFunctionPointer<eglChooseConfig>(NativeLoader.GetProcAddress(dl_handle, "eglChooseConfig"));
        CreateContext = Marshal.GetDelegateForFunctionPointer<eglCreateContext>(NativeLoader.GetProcAddress(dl_handle, "eglCreateContext"));
        CreatePbufferSurface = Marshal.GetDelegateForFunctionPointer<eglCreatePbufferSurface>(NativeLoader.GetProcAddress(dl_handle, "eglCreatePbufferSurface"));
        CreateWindowSurface = Marshal.GetDelegateForFunctionPointer<eglCreateWindowSurface>(NativeLoader.GetProcAddress(dl_handle, "eglCreateWindowSurface"));
        DestroyContext = Marshal.GetDelegateForFunctionPointer<eglDestroyContext>(NativeLoader.GetProcAddress(dl_handle, "eglDestroyContext"));
        DestroySurface = Marshal.GetDelegateForFunctionPointer<eglDestroySurface>(NativeLoader.GetProcAddress(dl_handle, "eglDestroySurface"));
        GetConfigAttrib = Marshal.GetDelegateForFunctionPointer<eglGetConfigAttrib>(NativeLoader.GetProcAddress(dl_handle, "eglGetConfigAttrib"));
        GetCurrentContext = Marshal.GetDelegateForFunctionPointer<eglGetCurrentContext>(NativeLoader.GetProcAddress(dl_handle, "eglGetCurrentContext"));
        GetDisplay = Marshal.GetDelegateForFunctionPointer<eglGetDisplay>(NativeLoader.GetProcAddress(dl_handle, "eglGetDisplay"));
        GetError = Marshal.GetDelegateForFunctionPointer<eglGetError>(NativeLoader.GetProcAddress(dl_handle, "eglGetError"));
        Initialize = Marshal.GetDelegateForFunctionPointer<eglInitialize>(NativeLoader.GetProcAddress(dl_handle, "eglInitialize"));
        MakeCurrent = Marshal.GetDelegateForFunctionPointer<eglMakeCurrent>(NativeLoader.GetProcAddress(dl_handle, "eglMakeCurrent"));
        SwapBuffers = Marshal.GetDelegateForFunctionPointer<eglSwapBuffers>(NativeLoader.GetProcAddress(dl_handle, "eglSwapBuffers"));
        ReleaseThread = Marshal.GetDelegateForFunctionPointer<eglReleaseThread>(NativeLoader.GetProcAddress(dl_handle, "eglReleaseThread"));
        SwapInterval = Marshal.GetDelegateForFunctionPointer<eglSwapInterval>(NativeLoader.GetProcAddress(dl_handle, "eglSwapInterval"));
        Terminate = Marshal.GetDelegateForFunctionPointer<eglTerminate>(NativeLoader.GetProcAddress(dl_handle, "eglTerminate"));
        GetCurrentSurface = Marshal.GetDelegateForFunctionPointer<eglGetCurrentSurface>(NativeLoader.GetProcAddress(dl_handle, "eglGetCurrentSurface"));
    }
}
