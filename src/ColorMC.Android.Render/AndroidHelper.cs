using Android.Opengl;
using Android.OS;
using Android.Util;
using System.Text;

namespace ColorMC.Android.GLRender;

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
