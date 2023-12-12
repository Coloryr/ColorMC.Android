using ColorMC.Android.GLRender.Bridges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.GLRender;

public static class GLBase
{
    public static void Init(RenderType type)
    {
        var handel = IntPtr.Zero;
        if (type == RenderType.GL4ES)
        {
            handel = NativeLoader.LoadLibrary("libgl4es_114.so");
        }
        else if (type == RenderType.ANGEL)
        {
            handel = NativeLoader.LoadLibrary("libGLESv2_angle.so");
        }
        else if (type == RenderType.ANDROID)
        {
            handel = NativeLoader.LoadLibrary("libGLESv2.so");
        }

        if (handel != IntPtr.Zero)
        {
            GL.Load(handel);
        }
    }

    public static void GetVersion()
    {
        var byteGlVersion = GL.GetString(GL.GL_VERSION);
        var byteGlVendor = GL.GetString(GL.GL_VENDOR);
        var byteGlRenderer = GL.GetString(GL.GL_RENDERER);
        var byteSLVersion = GL.GetString(GL.GL_SHADING_LANGUAGE_VERSION);
        string strTemp = "OpenGL version: " + Utf8Buffer.StringFromPtr(byteGlVersion);
        RenderLog.Info("GL", strTemp);
        strTemp = "GL_VENDOR: " + Utf8Buffer.StringFromPtr(byteGlVendor);
        RenderLog.Info("GL", strTemp);
        strTemp = "GL_RENDERER: " + Utf8Buffer.StringFromPtr(byteGlRenderer);
        RenderLog.Info("GL", strTemp);
        strTemp = "GLSL version: " + Utf8Buffer.StringFromPtr(byteSLVersion);
        RenderLog.Info("GL", strTemp);
    }

    public static bool IsAdrenoGraphics()
    {
        var byteGlVendor = GL.GetString(GL.GL_VENDOR);
        var byteGlRenderer = GL.GetString(GL.GL_RENDERER);

        var strTemp = Utf8Buffer.StringFromPtr(byteGlVendor);
        var strTemp1 = Utf8Buffer.StringFromPtr(byteGlRenderer);

        if (strTemp?.Contains("Qualcomm") == true 
            && strTemp1?.Contains("Adreno") == true)
        {
            return true;
        }

        return false;
    }
}
