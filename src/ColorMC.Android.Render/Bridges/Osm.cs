using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ColorMC.Android.GLRender.Bridges;

public class OSM
{
    //GLboolean (*OSMesaMakeCurrent_p) (OSMesaContext ctx, void *buffer, GLenum type, GLsizei width, GLsizei height);
    public unsafe delegate GLBoolean OSMesaMakeCurrent(OSMesaContext ctx, IntPtr buffer, GLenum type, GLsizei width, GLsizei height);
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

    public static OSMesaMakeCurrent MakeCurrent;
    public static OSMesaGetCurrentContext GetCurrentContext;
    public static OSMesaCreateContext CreateContext;
    public static OSMesaDestroyContext DestroyContext;
    public static OSMesaPixelStore PixelStore;

    public const int OSMESA_ROW_LENGTH = 0x10;
    public const int OSMESA_Y_UP = 0x11;

    public static void Load(IntPtr dl_handle)
    {
        MakeCurrent = Marshal.GetDelegateForFunctionPointer<OSMesaMakeCurrent>(NativeLoader.GetProcAddress(dl_handle, "OSMesaMakeCurrent"));
        GetCurrentContext = Marshal.GetDelegateForFunctionPointer<OSMesaGetCurrentContext>(NativeLoader.GetProcAddress(dl_handle, "OSMesaGetCurrentContext"));
        CreateContext = Marshal.GetDelegateForFunctionPointer<OSMesaCreateContext>(NativeLoader.GetProcAddress(dl_handle, "OSMesaCreateContext"));
        DestroyContext = Marshal.GetDelegateForFunctionPointer<OSMesaDestroyContext>(NativeLoader.GetProcAddress(dl_handle, "OSMesaDestroyContext"));
        PixelStore = Marshal.GetDelegateForFunctionPointer<OSMesaPixelStore>(NativeLoader.GetProcAddress(dl_handle, "OSMesaPixelStore"));
    }
}
