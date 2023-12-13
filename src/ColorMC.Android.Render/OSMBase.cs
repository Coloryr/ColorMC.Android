using Android.Content;
using Android.OS;
using Android.Systems;
using ColorMC.Android.GLRender.Bridges;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Tomlyn;

namespace ColorMC.Android.GLRender;

public static class OSMBase
{
    private static ANativeWindowBuffer buffer;
    private static OSMesaContext context;
    private static bool disable_rendering;
    private static RenderState state;
    private static int last_stride;
    private static IntPtr nativeSurface;
    private static IntPtr newNativeSurface;
    private static IntPtr no_render_buffer;
    private static IntPtr window;

    public static void osm_apply_current_ll()
    {
        OSM.MakeCurrent(context, buffer.bits, GL.GL_UNSIGNED_BYTE, buffer.width, buffer.height);
        if (buffer.stride != last_stride)
            OSM.PixelStore(OSM.OSMESA_ROW_LENGTH, buffer.stride);
        last_stride = buffer.stride;
    }

    public static void osm_set_no_render_buffer(ANativeWindowBuffer buffer)
    {
        buffer.bits = no_render_buffer;
        buffer.width = 1;
        buffer.height = 1;
        buffer.stride = 0;
    }

    public static void osm_swap_surfaces()
    {
        if (nativeSurface != IntPtr.Zero && newNativeSurface != nativeSurface)
        {
            if (!disable_rendering)
            {
                RenderLog.Info("OSM", "Unlocking for cleanup...");
                NativeWindow.UnlockAndPost(nativeSurface);
            }
            NativeWindow.Release(nativeSurface);
        }
        if (newNativeSurface != IntPtr.Zero)
        {
            RenderLog.Info("OSM", "Switching to new native surface");
            nativeSurface = newNativeSurface;
            newNativeSurface = IntPtr.Zero;
            NativeWindow.Acquire(nativeSurface);
            NativeWindow.SetBuffersGeometry(nativeSurface, 0, 0, NativeWindow.WINDOW_FORMAT_RGBX_8888);
            disable_rendering = false;
            return;
        }
        else
        {
            RenderLog.Info("OSM", "No new native surface, switching to dummy framebuffer");
            nativeSurface = IntPtr.Zero;
            osm_set_no_render_buffer(buffer);
            disable_rendering = true;
        }
    }

    public static OSMesaContext osm_init_context(OSMesaContext share)
    {
        context = OSM.CreateContext(GL.GL_RGBA, share);
        if (context == IntPtr.Zero)
        {
            return IntPtr.Zero;
        }
        return context;
    }

    public static void osm_release_window()
    {
        newNativeSurface = IntPtr.Zero;
        osm_swap_surfaces();
    }

    public static void osm_make_current()
    {
        if (nativeSurface == IntPtr.Zero)
        {
            //prepare the buffer for our first render!
            osm_swap_surfaces();
            state = RenderState.Alive;
        }
        osm_set_no_render_buffer(buffer);
        osm_apply_current_ll();
        OSM.PixelStore(OSM.OSMESA_Y_UP, 0);
    }

    public static void osm_swap_buffers()
    {
        if (state == RenderState.NewWindow)
        {
            osm_swap_surfaces();
            state = RenderState.Alive;
        }

        if (nativeSurface != 0 && !disable_rendering)
        {
            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<ANativeWindowBuffer>());
            Marshal.StructureToPtr(buffer, ptr, false);
            if (NativeWindow.Lock(nativeSurface, ptr, IntPtr.Zero) != 0)
            {
                osm_release_window();
            }
            buffer = Marshal.PtrToStructure<ANativeWindowBuffer>(ptr);
            Marshal.FreeHGlobal(ptr);
        }


        osm_apply_current_ll();
        GL.Finish(); // this will force osmesa to write the last rendered image into the buffer

        if (nativeSurface != IntPtr.Zero && !disable_rendering)
            if (NativeWindow.UnlockAndPost(nativeSurface) != 0)
                osm_release_window();
    }

    public static void osm_setup_window()
    {
        state = RenderState.NewWindow;
        newNativeSurface = window;
    }

    public static bool Init(Context context, IntPtr ptr, RenderType type)
    {
        window = ptr;

        no_render_buffer = Marshal.AllocHGlobal(4);
        Marshal.WriteInt64(no_render_buffer, 0, 0);

        StringBuilder ldLibraryPath = new StringBuilder();

        string temp = "/system/lib64:/vendor/lib64:/vendor/lib64/hw:" + context.ApplicationInfo!.NativeLibraryDir;

        Os.Setenv("NATIVE_DIR", temp, true);
        Os.Setenv("MESA_GLSL_CACHE_DIR", context.CacheDir!.AbsolutePath, true);
        Os.Setenv("force_glsl_extensions_warn", "true", true);
        Os.Setenv("allow_higher_compat_version", "true", true);
        Os.Setenv("allow_glsl_extension_directive_midshader", "true", true);
        Os.Setenv("MESA_LOADER_DRIVER_OVERRIDE", "zink", true);
        Os.Setenv("TMPDIR", context.CacheDir!.AbsolutePath, true);

        IntPtr dl_handle = NativeLoader.LoadLibrary("libcolormcnative.so");
        ColorMCNative.Load(dl_handle);
        ColorMCNative.LoadVulkan();

        dl_handle = NativeLoader.LoadLibrary("libOSMesa.so");
        OSM.Load(dl_handle);
        GL.Load(dl_handle);

        osm_init_context(IntPtr.Zero);
        osm_setup_window();
        osm_make_current();

        return true;
    }
}
