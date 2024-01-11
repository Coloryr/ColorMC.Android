using Android.Content;
using Android.Opengl;
using Android.Util;
using Android.Views;
using Javax.Microedition.Khronos.Opengles;

namespace ColorMC.Android.GLRender;

public class GLSurface : GLSurfaceView, ISurfaceHolderCallback, GLSurfaceView.IRenderer
{
    private static QuadRenderer qrender;

    private int width, height;

    private GameRender NowGame;

    public GLSurface(Context? context, GameRender game) : this(context, attributeSet: null)
    {
        NowGame = game;
    }

    public GLSurface(Context? context, IAttributeSet? attributeSet) : base(context, attributeSet)
    {
        SetEGLContextClientVersion(3);
        SetRenderer(this);
    }

    public void SetGame(GameRender game)
    {
        NowGame = game;
    }

    public void OnDrawFrame(IGL10? gl)
    {
        GLES20.GlViewport(0, 0, width, height);
        GLES20.GlClearColor(0, 0, 0, 0);
        GLES20.GlClear(GLES20.GlColorBufferBit);

        if (NowGame.HaveBuffer)
        {
            if (NowGame.TexId == 0)
            {
                NowGame.BindTexture();
            }
            else
            {
                qrender.DrawTexture(NowGame.TexId, width, height, 
                    NowGame.RenderWidth, NowGame.RenderHeight, false);
            }
        }
    }

    public void OnSurfaceChanged(IGL10? gl, int width, int height)
    {
        this.width = width;
        this.height = height;

        RenderTest.Available();

        qrender = new();
    }

    public void OnSurfaceCreated(IGL10? gl, Javax.Microedition.Khronos.Egl.EGLConfig? config)
    {

    }

    public void SetSize(string? text1, string? text2)
    {
        if (ushort.TryParse(text1, out var width)
            && ushort.TryParse(text2, out var height))
        {
            NowGame.Width = width;
            NowGame.Height = height;
            NowGame.SetSize();
        }
        else
        {
            Toast.MakeText(Context!, "错误的输入", ToastLength.Short).Show();
        }
    }
}
