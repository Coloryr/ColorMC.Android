//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace ColorMC.Android.Render.Bridges;

//public record OsmRenderWindow  : BasicRenderWindow
//{
//    public ANativeWindowBuffer Buffer;
//    public int LastStride;
//    public bool DisableRendering;
//    public OSMesaContext Context;
//}

//public class OsmBridge : IBridge<OsmRenderWindow>
//{
//    private const string _logTag = "GLBridge";
//    private OsmRenderWindow? _currentBundle;
//    // a tiny buffer for rendering when there's nowhere t render
//    private byte[] _noRenderBuffer = new byte[4];

//    public bool Init()
//    {
//        Osm.Load();
//        return true; // no more specific initialization required
//    }

//    public OsmRenderWindow? GetCurrent()
//    {
//        return _currentBundle;
//    }

//    public OsmRenderWindow? InitContext(OsmRenderWindow share)
//    {
//        OsmRenderWindow window = new();
//        OSMesaContext osmesaShare = 0;
//        if (share != null) osmesaShare = share.Context;
//        OSMesaContext context = Osm.OSMesaCreateContext_p(0x1908, osmesaShare);
//        if (context == 0)
//        {
//            return null;
//        }
//        window.Context = context;
//        return window;
//    }

//    public unsafe void OsmSetNoRenderBuffer(ANativeWindowBuffer buffer)
//    {
//        fixed(void* ptr = _noRenderBuffer)
//        buffer.bits = ptr;
//        buffer.width = 1;
//        buffer.height = 1;
//        buffer.stride = 0;
//    }

//    public unsafe void OsmSwapSurfaces(OsmRenderWindow bundle)
//    {
//        if (bundle.NativeSurface != IntPtr.Zero && bundle.NewNativeSurface != bundle.NativeSurface)
//        {
//            if (!bundle.DisableRendering)
//            {
//                RenderLog.Info(_logTag, "Unlocking for cleanup...");
//                NativeWindow.UnlockAndPost(bundle.NativeSurface);
//            }
//            NativeWindow.Release(bundle.NativeSurface);
//        }
//        if (bundle.NewNativeSurface != IntPtr.Zero)
//        {
//            RenderLog.Error(_logTag, "Switching to new native surface");
//            bundle.NativeSurface = bundle.NewNativeSurface;
//            bundle.NewNativeSurface = IntPtr.Zero;
//            NativeWindow.Acquire(bundle.NativeSurface);
//            NativeWindow.SetBuffersGeometry(bundle.NativeSurface, 0, 0, 2);
//            bundle.DisableRendering = false;
//            return;
//        }
//        else
//        {
//            RenderLog.Error(_logTag, "No new native surface, switching to dummy framebuffer");
//            bundle.NativeSurface = IntPtr.Zero;
//            OsmSetNoRenderBuffer(bundle.Buffer);
//            bundle.DisableRendering = true;
//        }

//    }

//    public unsafe void OsmReleaseWindow()
//    {
//        _currentBundle!.NewNativeSurface = IntPtr.Zero;
//        OsmSwapSurfaces(_currentBundle);
//    }

//    public unsafe void OsmApplyCurrentLL()
//    {
//        ANativeWindowBuffer buffer = _currentBundle!.Buffer;
//        Osm.OSMesaMakeCurrent_p(_currentBundle.Context, buffer.bits, 0x1401, buffer.width, buffer.height);
//        if (buffer.stride != _currentBundle.LastStride)
//            Osm.OSMesaPixelStore_p(0x10, buffer.stride);
//        _currentBundle.LastStride = buffer.stride;
//    }

//    public unsafe void MakeCurrent(OsmRenderWindow bundle)
//    {
//        if (bundle == null)
//        {
//            //technically this does nothing as its not possible to unbind a context in OSMesa
//            Osm.OSMesaMakeCurrent_p(0, null, 0, 0, 0);
//            _currentBundle = null;
//            return;
//        }
//        bool hasSetMainWindow = false;
//        _currentBundle = bundle;
//        if (GameEnviron.Game.MainWindowBundle == null)
//        {
//            GameEnviron.Game.MainWindowBundle = bundle;
//            fixed (void* ptr = &GameEnviron.Game.MainWindowBundle)
//                RenderLog.Info(_logTag, $"Main window bundle is now {(nint)ptr:x}");
//            GameEnviron.Game.MainWindowBundle.NewNativeSurface = GameEnviron.Game.Window;
//            hasSetMainWindow = true;
//        }
//        if (bundle.NativeSurface == IntPtr.Zero)
//        {
//            //prepare the buffer for our first render!
//            OsmSwapSurfaces(bundle);
//            if (hasSetMainWindow) GameEnviron.Game.MainWindowBundle.State = STATE_RENDERER.ALIVE;
//        }
//        OsmSetNoRenderBuffer(bundle.Buffer);
//        OsmApplyCurrentLL();
//        Osm.OSMesaPixelStore_p(0x11, 0);
//    }

//    public unsafe void SwapBuffers()
//    {
//        if (_currentBundle!.State == STATE_RENDERER.NEW_WINDOW)
//        {
//            OsmSwapSurfaces(_currentBundle);
//            _currentBundle.State = STATE_RENDERER.ALIVE;
//        }

//        if (_currentBundle!.NativeSurface != IntPtr.Zero && !_currentBundle.DisableRendering)
//        {
//            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<ANativeWindowBuffer>());
//            Marshal.StructureToPtr(_currentBundle.Buffer, ptr, false);
//            if (NativeWindow.Lock(_currentBundle.NativeSurface, ptr, IntPtr.Zero) != 0)
//            {
//                OsmReleaseWindow();
//            }
//            _currentBundle.Buffer = Marshal.PtrToStructure<ANativeWindowBuffer>(ptr);
//            Marshal.FreeHGlobal(ptr);
//        }

//        OsmApplyCurrentLL();
//        Osm.glFinish_p(); // this will force osmesa to write the last rendered image into the buffer

//        if (_currentBundle.NativeSurface != IntPtr.Zero && !_currentBundle.DisableRendering)
//            if (NativeWindow.UnlockAndPost(_currentBundle.NativeSurface) != 0)
//                OsmReleaseWindow();
//    }

//    public unsafe void SetupWindow()
//    {
//        if (GameEnviron.Game.MainWindowBundle != null)
//        {
//            RenderLog.Info(_logTag, "Main window bundle is not NULL, changing state");
//            GameEnviron.Game.MainWindowBundle.State = STATE_RENDERER.NEW_WINDOW;
//            GameEnviron.Game.MainWindowBundle.NewNativeSurface = GameEnviron.Game.Window;
//        }
//    }

//    public unsafe void WwapInterval(int swapInterval)
//    {
//        if (GameEnviron.Game.MainWindowBundle != null && GameEnviron.Game.MainWindowBundle.NativeSurface != IntPtr.Zero)
//        {
//            SwapInterval.SetNativeWindowSwapInterval(GameEnviron.Game.MainWindowBundle.NativeSurface, swapInterval);
//        }
//    }
//}
