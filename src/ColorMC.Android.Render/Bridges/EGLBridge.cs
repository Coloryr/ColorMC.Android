using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.Render.Bridges;

public static class EGLBridge
{
    public struct PotatoBridge
    {

        /* EGLContext */
        public IntPtr eglContext;
        /* EGLDisplay */
        public IntPtr eglDisplay;
        /* EGLSurface */
        public IntPtr eglSurface;
        /*
            void* eglSurfaceRead;
            void* eglSurfaceDraw;
        */
    };
    public static EGLConfig config;
    public static PotatoBridge potatoBridge;

    private static IBridge<BasicRenderWindow> Bridge;

    [UnmanagedCallersOnly(EntryPoint = "pojavTerminate")]
    public static void Terminating()
    {
        Console.WriteLine("EGLBridge: Terminating");

        switch (GameEnviron.Game.config_renderer)
        {
            case RENDERER.GL4ES:
                {
                    EGL.eglMakeCurrent_p(potatoBridge.eglDisplay, 0, 0, 0);
                    EGL.eglDestroySurface_p(potatoBridge.eglDisplay, potatoBridge.eglSurface);
                    EGL.eglDestroyContext_p(potatoBridge.eglDisplay, potatoBridge.eglContext);
                    EGL.eglTerminate_p(potatoBridge.eglDisplay);
                    EGL.eglReleaseThread_p();

                    potatoBridge.eglContext = 0;
                    potatoBridge.eglDisplay = 0;
                    potatoBridge.eglSurface = 0;
                }
                break;

            //case RENDERER_VIRGL:
            case RENDERER.VK_ZINK:
                {
                    // Nothing to do here
                }
                break;
        }
    }

    public static void SetupBridgeWindow(IntPtr env, IntPtr surface)
    {
        GameEnviron.Game.Window = NativeWindow.FromSurface(env, surface);
        Bridge.SetupWindow();
    }

    public static void ReleaseBridgeWindow()
    {
        NativeWindow.Release(GameEnviron.Game.Window);
    }

    [UnmanagedCallersOnly(EntryPoint = "pojavGetCurrentContext")]
    public unsafe static nint GetCurrentContext()
    {
        GCHandle h = GCHandle.Alloc(Bridge.GetCurrent(), GCHandleType.WeakTrackResurrection);
        return GCHandle.ToIntPtr(h);
    }

    //Checks if your graphics are Adreno. Returns true if your graphics are Adreno, false otherwise or if there was an error
    public unsafe static bool checkAdrenoGraphics()
    {
        EGLDisplay eglDisplay = EGL.eglGetDisplay_p(0);
        if (eglDisplay == 0 || EGL.eglInitialize_p(eglDisplay, (int*)0, (int*)0) != 1) return false;
        EGLint[] egl_attributes = [0x3022, 8, 0x3023, 8, 0x3024, 8, 0x3021, 8, 0x3025, 24, 0x3033, 0x0001, 0x3040, 0x0004, 0x3038];
        EGLint num_configs = 0;
        bool is_adreno = false;
        fixed (int* ptr = egl_attributes)
        {
            if (EGL.eglChooseConfig_p(eglDisplay, ptr, (nint*)0, 0, &num_configs) != 1 || num_configs == 0)
            {
                EGL.eglTerminate_p(eglDisplay);
                return false;
            }
            EGLConfig eglConfig;
            EGL.eglChooseConfig_p(eglDisplay, ptr, &eglConfig, 1, &num_configs);
            EGLint[] egl_context_attributes = { 0x3098, 3, 0x3038 };
            fixed (int* ptr1 = egl_context_attributes)
            {
                EGLContext context = EGL.eglCreateContext_p(eglDisplay, eglConfig, 0, ptr1);
                if (context == 0)
                {
                    EGL.eglTerminate_p(eglDisplay);
                    return false;
                }
                if (EGL.eglMakeCurrent_p(eglDisplay, 0, 0, context) != 1)
                {
                    EGL.eglDestroyContext_p(eglDisplay, context);
                    EGL.eglTerminate_p(eglDisplay);
                }

                var vendor = new string(Osm.glGetString_p(0x1F00));
                var renderer = new string(Osm.glGetString_p(0x1F01));
                
                if (vendor.Contains("Qualcomm")  && renderer.Contains("Adreno"))
                {
                    is_adreno = true; // TODO: check for Turnip support
                }
                EGL.eglMakeCurrent_p(eglDisplay, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
                EGL.eglDestroyContext_p(eglDisplay, context);
                EGL.eglTerminate_p(eglDisplay);
            }
        }
        return is_adreno;
    }
    void* load_turnip_vulkan()
    {
        if (!checkAdrenoGraphics()) return NULL;
        const char* native_dir = getenv("POJAV_NATIVEDIR");
        const char* cache_dir = getenv("TMPDIR");
        if (!linker_ns_load(native_dir)) return NULL;
        void* linkerhook = linker_ns_dlopen("liblinkerhook.so", RTLD_LOCAL | RTLD_NOW);
        if (linkerhook == NULL) return NULL;
        void* turnip_driver_handle = linker_ns_dlopen("libvulkan_freedreno.so", RTLD_LOCAL | RTLD_NOW);
        if (turnip_driver_handle == NULL)
        {
            printf("AdrenoSupp: Failed to load Turnip!\n%s\n", dlerror());
            dlclose(linkerhook);
            return NULL;
        }
        void* dl_android = linker_ns_dlopen("libdl_android.so", RTLD_LOCAL | RTLD_LAZY);
        if (dl_android == NULL)
        {
            dlclose(linkerhook);
            dlclose(turnip_driver_handle);
            return NULL;
        }
        void* android_get_exported_namespace = dlsym(dl_android, "android_get_exported_namespace");
        void(*linkerhook_pass_handles)(void *, void *, void *) = dlsym(linkerhook, "app__pojav_linkerhook_pass_handles");
        if (linkerhook_pass_handles == NULL || android_get_exported_namespace == NULL)
        {
            dlclose(dl_android);
            dlclose(linkerhook);
            dlclose(turnip_driver_handle);
            return NULL;
        }
        linkerhook_pass_handles(turnip_driver_handle, android_dlopen_ext, android_get_exported_namespace);
        void* libvulkan = linker_ns_dlopen_unique(cache_dir, "libvulkan.so", RTLD_LOCAL | RTLD_NOW);
        return libvulkan;
    }

    static void set_vulkan_ptr(void* ptr)
    {
        char envval[64];
        sprintf(envval, "%"PRIxPTR, (uintptr_t)ptr);
        setenv("VULKAN_PTR", envval, 1);
    }

    void load_vulkan()
    {
        if (getenv("POJAV_ZINK_PREFER_SYSTEM_DRIVER") == NULL &&
            android_get_device_api_level() >= 28)
        { // the loader does not support below that

            void* result = load_turnip_vulkan();
            if (result != NULL)
            {
                printf("AdrenoSupp: Loaded Turnip, loader address: %p\n", result);
                set_vulkan_ptr(result);
                return;
            }
        }
        printf("OSMDroid: loading vulkan regularly...\n");
        void* vulkan_ptr = dlopen("libvulkan.so", RTLD_LAZY | RTLD_LOCAL);
        printf("OSMDroid: loaded vulkan, ptr=%p\n", vulkan_ptr);
        set_vulkan_ptr(vulkan_ptr);
    }

    int pojavInitOpenGL()
    {
        // Only affects GL4ES as of now
        const char* forceVsync = getenv("FORCE_VSYNC");
        if (strcmp(forceVsync, "true") == 0)
            pojav_environ->force_vsync = true;

        // NOTE: Override for now.
        const char* renderer = getenv("POJAV_RENDERER");
        if (strncmp("opengles", renderer, 8) == 0)
        {
            pojav_environ->config_renderer = RENDERER_GL4ES;
            set_gl_bridge_tbl();
        }
        else if (strcmp(renderer, "vulkan_zink") == 0)
        {
            pojav_environ->config_renderer = RENDERER_VK_ZINK;
            load_vulkan();
            setenv("GALLIUM_DRIVER", "zink", 1);
            set_osm_bridge_tbl();
        }
        if (br_init())
        {
            br_setup_window();
        }
        return 0;
    }

    EXTERNAL_API int pojavInit()
    {
        ANativeWindow_acquire(GameEnviron.Game.Window);
        pojav_environ->savedWidth = ANativeWindow_getWidth(GameEnviron.Game.Window);
        pojav_environ->savedHeight = ANativeWindow_getHeight(GameEnviron.Game.Window);
        ANativeWindow_setBuffersGeometry(GameEnviron.Game.Window, pojav_environ->savedWidth, pojav_environ->savedHeight, AHARDWAREBUFFER_FORMAT_R8G8B8X8_UNORM);
        pojavInitOpenGL();
        return 1;
    }

    EXTERNAL_API void pojavSetWindowHint(int hint, int value)
    {
        if (hint != GLFW_CLIENT_API) return;
        switch (value)
        {
            case GLFW_NO_API:
                pojav_environ->config_renderer = RENDERER_VULKAN;
                /* Nothing to do: initialization is handled in Java-side */
                // pojavInitVulkan();
                break;
            case GLFW_OPENGL_API:
                /* Nothing to do: initialization is called in pojavCreateContext */
                // pojavInitOpenGL();
                break;
            default:
                printf("GLFW: Unimplemented API 0x%x\n", value);
                abort();
        }
    }

    EXTERNAL_API void pojavSwapBuffers()
    {
        br_swap_buffers();
    }


    EXTERNAL_API void pojavMakeCurrent(void* window)
    {
        br_make_current((basic_render_window_t*)window);
    }

    EXTERNAL_API void* pojavCreateContext(void* contextSrc)
    {
        if (pojav_environ->config_renderer == RENDERER_VULKAN)
        {
            return (void*)GameEnviron.Game.Window;
        }
        return br_init_context((basic_render_window_t*)contextSrc);
    }

    EXTERNAL_API JNIEXPORT jlong JNICALL
Java_org_lwjgl_vulkan_VK_getVulkanDriverHandle(ABI_COMPAT JNIEnv *env, ABI_COMPAT jclass thiz)
    {
        printf("EGLBridge: LWJGL-side Vulkan loader requested the Vulkan handle\n");
        // The code below still uses the env var because
        // 1. it's easier to do that
        // 2. it won't break if something will try to load vulkan and osmesa simultaneously
        if (getenv("VULKAN_PTR") == NULL) load_vulkan();
        return strtoul(getenv("VULKAN_PTR"), NULL, 0x10);
    }

    EXTERNAL_API void pojavSwapInterval(int interval)
    {
        br_swap_interval(interval);
    }


}
