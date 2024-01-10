using Android.Content;
using Android.Graphics;
using Android.Opengl;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Javax.Microedition.Khronos.Opengles;
using System;

namespace ColorMC.Android.GLRender;

public class GLSurface : GLSurfaceView, ISurfaceHolderCallback, GLSurfaceView.IRenderer
{
    private static QuadRenderer render;

    private int width, height;
    private GameLauncher NowGame;

    public GLSurface(Context? context) : this(context, null)
    {

    }

    public GLSurface(Context? context, IAttributeSet? attributeSet) : base(context, attributeSet)
    {
        SetEGLContextClientVersion(3);
        SetRenderer(this);
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
                render.DrawTexture(NowGame.TexId, width, height, NowGame.RenderWidth, NowGame.RenderHeight, false);
            }
        }
    }

    public void OnSurfaceChanged(IGL10? gl, int width, int height)
    {
        this.width = width;
        this.height = height;
       
        RenderTest.Available();

        render = new();
        NowGame = new GameLauncher()
        {
            Width = 640,
            Height = 480
        };
        NowGame.Start(Context!);
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
