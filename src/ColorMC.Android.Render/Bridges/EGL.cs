using ColorMC.Core.Utils;
using System.Runtime.InteropServices;

namespace ColorMC.Android.Render.Bridges;

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
    public unsafe delegate EGLSurface eglCreateWindowSurface(EGLDisplay display, EGLConfig config, NativeWindowType window, EGLint* attrib_list);
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

    public static eglMakeCurrent eglMakeCurrent_p;
    public static eglDestroyContext eglDestroyContext_p;
    public static eglDestroySurface eglDestroySurface_p;
    public static eglTerminate eglTerminate_p;
    public static eglReleaseThread eglReleaseThread_p;
    public static eglGetCurrentContext eglGetCurrentContext_p ;
    public static eglGetDisplay eglGetDisplay_p;
    public static eglInitialize eglInitialize_p;
    public static eglChooseConfig eglChooseConfig_p;
    public static eglGetConfigAttrib eglGetConfigAttrib_p;
    public static eglBindAPI eglBindAPI_p;
    public static eglCreatePbufferSurface eglCreatePbufferSurface_p;
    public static eglCreateWindowSurface eglCreateWindowSurface_p;
    public static eglSwapBuffers eglSwapBuffers_p;
    public static eglGetError eglGetError_p;
    public static eglCreateContext eglCreateContext_p;
    public static eglSwapInterval eglSwapInterval_p;
    public static eglGetCurrentSurface eglGetCurrentSurface_p;

    public static void Load()
    {
        var file = Environment.GetEnvironmentVariable("EGL_FILE");
        if (!File.Exists(file))
        {
            throw new FileNotFoundException("EGL Lib not found");
        }
        IntPtr dl_handle =NativeLoader.LoadLibrary(file);

        eglBindAPI_p = Marshal.GetDelegateForFunctionPointer<eglBindAPI>(NativeLoader.GetProcAddress(dl_handle, "eglBindAPI"));
        eglChooseConfig_p = Marshal.GetDelegateForFunctionPointer<eglChooseConfig>(NativeLoader.GetProcAddress(dl_handle, "eglChooseConfig"));
        eglCreateContext_p = Marshal.GetDelegateForFunctionPointer<eglCreateContext>(NativeLoader.GetProcAddress(dl_handle, "eglCreateContext"));
        eglCreatePbufferSurface_p = Marshal.GetDelegateForFunctionPointer<eglCreatePbufferSurface>(NativeLoader.GetProcAddress(dl_handle, "eglCreatePbufferSurface"));
        eglCreateWindowSurface_p = Marshal.GetDelegateForFunctionPointer<eglCreateWindowSurface>(NativeLoader.GetProcAddress(dl_handle, "eglCreateWindowSurface"));
        eglDestroyContext_p = Marshal.GetDelegateForFunctionPointer<eglDestroyContext>(NativeLoader.GetProcAddress(dl_handle, "eglDestroyContext"));
        eglDestroySurface_p = Marshal.GetDelegateForFunctionPointer<eglDestroySurface>(NativeLoader.GetProcAddress(dl_handle, "eglDestroySurface"));
        eglGetConfigAttrib_p = Marshal.GetDelegateForFunctionPointer<eglGetConfigAttrib>(NativeLoader.GetProcAddress(dl_handle, "eglGetConfigAttrib"));
        eglGetCurrentContext_p = Marshal.GetDelegateForFunctionPointer<eglGetCurrentContext>(NativeLoader.GetProcAddress(dl_handle, "eglGetCurrentContext"));
        eglGetDisplay_p = Marshal.GetDelegateForFunctionPointer<eglGetDisplay>(NativeLoader.GetProcAddress(dl_handle, "eglGetDisplay"));
        eglGetError_p = Marshal.GetDelegateForFunctionPointer<eglGetError>(NativeLoader.GetProcAddress(dl_handle, "eglGetError"));
        eglInitialize_p = Marshal.GetDelegateForFunctionPointer<eglInitialize>(NativeLoader.GetProcAddress(dl_handle, "eglInitialize"));
        eglMakeCurrent_p = Marshal.GetDelegateForFunctionPointer<eglMakeCurrent>(NativeLoader.GetProcAddress(dl_handle, "eglMakeCurrent"));
        eglSwapBuffers_p = Marshal.GetDelegateForFunctionPointer<eglSwapBuffers>(NativeLoader.GetProcAddress(dl_handle, "eglSwapBuffers"));
        eglReleaseThread_p = Marshal.GetDelegateForFunctionPointer<eglReleaseThread>(NativeLoader.GetProcAddress(dl_handle, "eglReleaseThread"));
        eglSwapInterval_p = Marshal.GetDelegateForFunctionPointer<eglSwapInterval>(NativeLoader.GetProcAddress(dl_handle, "eglSwapInterval"));
        eglTerminate_p = Marshal.GetDelegateForFunctionPointer<eglTerminate>(NativeLoader.GetProcAddress(dl_handle, "eglTerminate"));
        eglGetCurrentSurface_p = Marshal.GetDelegateForFunctionPointer<eglGetCurrentSurface>(NativeLoader.GetProcAddress(dl_handle, "eglGetCurrentSurface"));
    }
}
