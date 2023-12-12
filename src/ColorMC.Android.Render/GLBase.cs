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
        if (type == RenderType.GL4ES)
        {
            GL.Load("libgl4es_114.so");
        }
        else if(type == RenderType.ANGEL)
        {
            GL.Load("libGLESv2_angle.so");
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
}
