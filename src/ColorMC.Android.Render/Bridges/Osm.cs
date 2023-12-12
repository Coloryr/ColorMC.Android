using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ColorMC.Android.GLRender.Bridges;

public class Osm
{
    //GLboolean (*OSMesaMakeCurrent_p) (OSMesaContext ctx, void *buffer, GLenum type, GLsizei width, GLsizei height);
    public unsafe delegate GLBoolean OSMesaMakeCurrent(OSMesaContext ctx, void* buffer, GLenum type, GLsizei width, GLsizei height);
    //OSMesaContext (*OSMesaGetCurrentContext_p) (void);
    public delegate OSMesaContext OSMesaGetCurrentContext();
    //OSMesaContext  (*OSMesaCreateContext_p) (GLenum format, OSMesaContext sharelist);
    public delegate OSMesaContext OSMesaCreateContext(GLenum format, OSMesaContext sharelist);
    //void (*OSMesaDestroyContext_p) (OSMesaContext ctx);
    public delegate void OSMesaDestroyContext(OSMesaContext ctx);
    //void (*OSMesaPixelStore_p) ( GLint pname, GLint value );
    public delegate void OSMesaPixelStore(GLint pname, GLint value);
    //GLubyte* (*glGetString_p) (GLenum name);
    public unsafe delegate GLubyte* glGetString(GLenum name);
    //void (*glFinish_p) (void);
    public delegate void glFinish();
    //void (*glClearColor_p) (GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);
    public delegate void glClearColor(GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);
    //void (*glClear_p) (GLbitfield mask);
    public delegate void glClear(GLbitfield mask);
    //void (*glReadPixels_p) (GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, void * data);
    public unsafe delegate void glReadPixels(GLint x, GLint y, GLsizei width, GLsizei height, GLenum format, GLenum type, void* data);

    public static OSMesaMakeCurrent OSMesaMakeCurrent_p;
    public static OSMesaGetCurrentContext OSMesaGetCurrentContext_p;
    public static OSMesaCreateContext OSMesaCreateContext_p;
    public static OSMesaDestroyContext OSMesaDestroyContext_p;
    public static OSMesaPixelStore OSMesaPixelStore_p;
    public static glGetString glGetString_p;
    public static glFinish glFinish_p;
    public static glClearColor glClearColor_p;
    public static glClear glClear_p;
    public static glReadPixels glReadPixels_p;

    public static void Load()
    {
        var local =  Environment.GetEnvironmentVariable("NATIVEDIR") 
            ?? throw new FileNotFoundException("Mesa Lib not found");
        IntPtr dl_handle;
        string main_path = local + "/libOSMesa.so";
        string alt_path = local + "/libOSMesa.so.8";
        if (File.Exists(main_path))
        {
            dl_handle = NativeLoader.LoadLibrary(main_path);
        }
        else if (File.Exists(alt_path))
        {
            dl_handle = NativeLoader.LoadLibrary(alt_path);
        }
        else
        {
            throw new FileNotFoundException("Mesa Lib not found");
        }

        OSMesaMakeCurrent_p = Marshal.GetDelegateForFunctionPointer<OSMesaMakeCurrent>(NativeLoader.GetProcAddress(dl_handle, "OSMesaMakeCurrent"));
        OSMesaGetCurrentContext_p = Marshal.GetDelegateForFunctionPointer<OSMesaGetCurrentContext>(NativeLoader.GetProcAddress(dl_handle, "OSMesaGetCurrentContext"));
        OSMesaCreateContext_p = Marshal.GetDelegateForFunctionPointer<OSMesaCreateContext>(NativeLoader.GetProcAddress(dl_handle, "OSMesaCreateContext"));
        OSMesaDestroyContext_p = Marshal.GetDelegateForFunctionPointer<OSMesaDestroyContext>(NativeLoader.GetProcAddress(dl_handle, "OSMesaDestroyContext"));
        OSMesaPixelStore_p = Marshal.GetDelegateForFunctionPointer<OSMesaPixelStore>(NativeLoader.GetProcAddress(dl_handle, "OSMesaPixelStore"));
        glGetString_p = Marshal.GetDelegateForFunctionPointer<glGetString>(NativeLoader.GetProcAddress(dl_handle, "glGetString"));
        glClearColor_p = Marshal.GetDelegateForFunctionPointer<glClearColor>(NativeLoader.GetProcAddress(dl_handle, "glClearColor"));
        glClear_p = Marshal.GetDelegateForFunctionPointer<glClear>(NativeLoader.GetProcAddress(dl_handle, "glClear"));
        glFinish_p = Marshal.GetDelegateForFunctionPointer<glFinish>(NativeLoader.GetProcAddress(dl_handle, "glFinish"));
        glReadPixels_p = Marshal.GetDelegateForFunctionPointer<glReadPixels>(NativeLoader.GetProcAddress(dl_handle, "glReadPixels"));
    }
}
