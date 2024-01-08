using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.GLRender;

public class GLSurface : View, ISurfaceHolderCallback
{
    private SurfaceView view;
    public GLSurface(Context? context) : this(context, null)
    {
    }

    public GLSurface(Context? context, IAttributeSet? attributeSet) : base(context, attributeSet)
    {
        view = new SurfaceView(Context);
        view.Holder?.AddCallback(this);
    }

    protected override void OnAttachedToWindow()
    {
        base.OnAttachedToWindow();

        (Parent as ViewGroup)!.AddView(view);
    }

    public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
    {
        RenderTest.ChangeSize(width, height);
    }

    public void SurfaceCreated(ISurfaceHolder holder)
    {
        RenderTest.Init(Context!);
    }

    public void SurfaceDestroyed(ISurfaceHolder holder)
    {
        
    }

    public void Init()
    { 
        
    }
}
