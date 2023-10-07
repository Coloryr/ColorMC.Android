using Android.Util;
using ColorMC.Android.Lib;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using Javax.Microedition.Khronos.Egl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android;

public static class GameRender
{
    public static string GameRenderName = "vulkan_zink";
    public static string RenderLibrary;

    private const int EGL_OPENGL_ES_BIT = 0x0001;
    private const int EGL_OPENGL_ES2_BIT = 0x0004;
    private const int EGL_OPENGL_ES3_BIT_KHR = 0x0040;

    public static string LoadGraphicsLibrary()
    {
        switch (GameRenderName)
        {
            case "opengles3":
                RenderLibrary = "libgl4es_114.so";
                break;
            case "opengles2_vgpu":
                RenderLibrary = "libvgpu.so";
                break;
            case "opengles3_virgl":
                RenderLibrary = "libOSMesa_virgl.so";
                break;
            case "vulkan_zink":
                RenderLibrary = "libOSMesa.so";
                break;
            case "opengles3_desktopgl_angle_vulkan":
                RenderLibrary = "libtinywrapper.so";
                break;
            default:
                Log.Warn("RENDER_LIBRARY", "No renderer selected, defaulting to opengles2");
                RenderLibrary = "libgl4es_114.so";
                break;
        }

        if (!NativeHook.LoadNative(RenderLibrary) 
            && !NativeHook.LoadNative(JavaLoad.FindFromLdPath(RenderLibrary)))
        {
            Log.Error("RENDER_LIBRARY", "Failed to load renderer " + RenderLibrary + ". Falling back to GL4ES 1.1.4");
            GameRenderName = "opengles2";
            RenderLibrary = "libgl4es_114.so";
            NativeHook.LoadNative(JavaLoad.NativeLibPath + "/libgl4es_114.so");
        }
        return RenderLibrary;
    }

    public static Dictionary<string, string> GetRenderEnv()
    {
        var envs = new Dictionary<string, string>();
        if (GameRenderName != null)
        {
            envs.Add("MESA_GL_VERSION_OVERRIDE", GameRenderName == "opengles3_virgl" ? "4.3" : "4.6");
            envs.Add("MESA_GLSL_VERSION_OVERRIDE", GameRenderName == "opengles3_virgl" ? "430" : "460");
        }
        //if (PREF_DUMP_SHADERS)
        //    envs.Add("LIBGL_VGPU_DUMP", "1");
        //if (PREF_ZINK_PREFER_SYSTEM_DRIVER)
        //    envs.Add("POJAV_ZINK_PREFER_SYSTEM_DRIVER", "1");
        if (GameRenderName != null)
        {
            envs.Add("GAME_RENDERER", GameRenderName);
            if (GameRenderName == "opengles3_desktopgl_angle_vulkan")
            {
                envs.Add("LIBGL_ES", "3");
                envs.Add("POJAVEXEC_EGL", "libEGL_angle.so"); // Use ANGLE EGL
            }
        }
        //if (LauncherPreferences.PREF_BIG_CORE_AFFINITY) 
        //    envs.Add("POJAV_BIG_CORE_AFFINITY", "1");
        //envs.Add("AWTSTUB_WIDTH", Integer.toString(CallbackBridge.windowWidth > 0 ? CallbackBridge.windowWidth : CallbackBridge.physicalWidth));
        //envs.Add("AWTSTUB_HEIGHT", Integer.toString(CallbackBridge.windowHeight > 0 ? CallbackBridge.windowHeight : CallbackBridge.physicalHeight));

        // The OPEN GL version is changed according

        if (!envs.ContainsKey("LIBGL_ES") && GameRenderName != null)
        {
            int glesMajor = GetDetectedVersion();
            Log.Info("glesDetect", "GLES version detected: " + glesMajor);

            if (glesMajor < 3)
            {
                //fallback to 2 since it's the minimum for the entire app
                envs.Add("LIBGL_ES", "2");
            }
            else if (GameRenderName.StartsWith("opengles"))
            {
                envs.Add("LIBGL_ES", GameRenderName.Replace("opengles", "").Replace("_5", ""));
            }
            else
            {
                // TODO if can: other backends such as Vulkan.
                // Sure, they should provide GLES 3 support.
                envs.Add("LIBGL_ES", "3");
            }
        }

        return envs;
    }

    public static int GetDetectedVersion()
    {
        /*
         * Get all the device configurations and check the EGL_RENDERABLE_TYPE attribute
         * to determine the highest ES version supported by any config. The
         * EGL_KHR_create_context extension is required to check for ES3 support; if the
         * extension is not present this test will fail to detect ES3 support. This
         * effectively makes the extension mandatory for ES3-capable devices.
         */
        IEGL10 egl = (IEGL10)EGLContext.EGL!;
        EGLDisplay display = egl.EglGetDisplay(IEGL10.EglDefaultDisplay)!;
        int[] numConfigs = new int[1];
        if (egl.EglInitialize(display, null))
        {
            try
            {
                var checkES3 = egl.EglQueryString(display, IEGL10.EglExtensions).Contains("EGL_KHR_create_context");
                if (egl.EglGetConfigs(display, null, 0, numConfigs))
                {
                    EGLConfig[] configs = new EGLConfig[numConfigs[0]];
                    if (egl.EglGetConfigs(display, configs, numConfigs[0], numConfigs))
                    {
                        int highestEsVersion = 0;
                        int[] value = new int[1];
                        for (int i = 0; i < numConfigs[0]; i++)
                        {
                            if (egl.EglGetConfigAttrib(display, configs[i],
                                    IEGL10.EglRenderableType, value))
                            {
                                if (checkES3 && ((value[0] & EGL_OPENGL_ES3_BIT_KHR) ==
                                        EGL_OPENGL_ES3_BIT_KHR))
                                {
                                    if (highestEsVersion < 3) highestEsVersion = 3;
                                }
                                else if ((value[0] & EGL_OPENGL_ES2_BIT) == EGL_OPENGL_ES2_BIT)
                                {
                                    if (highestEsVersion < 2) highestEsVersion = 2;
                                }
                                else if ((value[0] & EGL_OPENGL_ES_BIT) == EGL_OPENGL_ES_BIT)
                                {
                                    if (highestEsVersion < 1) highestEsVersion = 1;
                                }
                            }
                            else
                            {
                                Log.Warn("glesDetect", "Getting config attribute with "
                                        + "EGL10#eglGetConfigAttrib failed "
                                        + "(" + i + "/" + numConfigs[0] + "): "
                                        + egl.EglGetError());
                            }
                        }
                        return highestEsVersion;
                    }
                    else
                    {
                        Log.Error("glesDetect", "Getting configs with EGL10#eglGetConfigs failed: "
                                + egl.EglGetError());
                        return -1;
                    }
                }
                else
                {
                    Log.Error("glesDetect", "Getting number of configs with EGL10#eglGetConfigs failed: "
                            + egl.EglGetError());
                    return -2;
                }
            }
            finally
            {
                egl.EglTerminate(display);
            }
        }
        else
        {
            Log.Error("glesDetect", "Couldn't initialize EGL.");
            return -3;
        }
    }
}
