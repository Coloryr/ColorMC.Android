using Android.Opengl;
using Android.OS;
using Android.Util;

namespace ColorMC.Android.GLRender;

public static class MathUtils
{

    //Ported from https://www.arduino.cc/reference/en/language/functions/math/map/
    public static float map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    /** Returns the distance between two points. */
    public static float Dist(float x1, float y1, float x2, float y2)
    {
        float x = (x2 - x1);
        float y = (y2 - y1);
        return float.Hypot(x, y);
    }

}

public static class GLHelper
{
    public static int CreateTexture()
    {
        int[] textures = new int[1];
        GLES20.GlGenTextures(1, textures, 0);
        GLES20.GlBindTexture(GLES20.GlTexture2d, textures[0]);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapS, GLES20.GlClampToEdge);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapT, GLES20.GlClampToEdge);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlLinear);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlLinear);
        return textures[0];
    }

    public static void DeleteTexture(int texId)
    {
        int[] textures = [texId];
        GLES20.GlDeleteTextures(1, textures, 0);
    }

    public static string GetFileName(this GameRender.RenderType type)
    {
        return type switch
        {
            _ => "libgl4es_114.so"
        };
    }

    public static string GetName(this GameRender.RenderType type)
    {
        return type switch
        {
            GameRender.RenderType.angle => "angle",
            GameRender.RenderType.zink => "zink",
            _ => "gl4es"
        };
    }
}

public static class AndroidHelper
{
    public static readonly Handler Main = new(Looper.MainLooper);

    public static DisplayMetrics GetDisplayMetrics(Activity activity)
    {
        var displayMetrics = new DisplayMetrics();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.N
            && (activity.IsInMultiWindowMode || activity.IsInPictureInPictureMode))
        {
            //For devices with free form/split screen, we need window size, not screen size.
            displayMetrics = activity.Resources.DisplayMetrics;
        }
        else
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                activity.Display.GetRealMetrics(displayMetrics);
            }
            else
            { // Removed the clause for devices with unofficial notch support, since it also ruins all devices with virtual nav bars before P
                activity.WindowManager.DefaultDisplay.GetRealMetrics(displayMetrics);
            }
        }
        return displayMetrics;
    }
}
