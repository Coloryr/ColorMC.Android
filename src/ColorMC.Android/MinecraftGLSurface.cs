using Android.Content;
using Android.Graphics;
using Android.Renderscripts;
using Android.Runtime;
using Android.Views;
using Java.Interop;
using Net.Kdt.Pojavlaunch.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android;

public class MinecraftGLSurface : View
{
    private bool isCalled = false;
    public Action Run;
    private TextureView _surface;
    public MinecraftGLSurface(Context? context) : base(context)
    {
        _surface = new TextureView(Context);
        _surface.SetOpaque(true);
        _surface.Alpha = 1.0f;

        _surface.SurfaceTextureAvailable += TextureView_SurfaceTextureAvailable;
        _surface.SurfaceTextureDestroyed += TextureView_SurfaceTextureDestroyed;
        _surface.SurfaceTextureSizeChanged += TextureView_SurfaceTextureSizeChanged;
    }

    public void Start()
    {
        ((ViewGroup)Parent).AddView(_surface);
    }

    private void TextureView_SurfaceTextureSizeChanged(object? sender, TextureView.SurfaceTextureSizeChangedEventArgs e)
    {

    }

    private void TextureView_SurfaceTextureDestroyed(object? sender, TextureView.SurfaceTextureDestroyedEventArgs e)
    {

    }

    private void TextureView_SurfaceTextureAvailable(object? sender, TextureView.SurfaceTextureAvailableEventArgs e)
    {
        Surface tSurface = new Surface(e.Surface);
        if (isCalled)
        {
            //JREUtils.setupBridgeWindow(tSurface);
            return;
        }
        isCalled = true;

        Run();

        //realStart(tSurface);
    }
}
