using Android.Content;
using Android.Opengl;
using Android.OS;
using Android.Util;
using Android.Views;
using Javax.Microedition.Khronos.Opengles;

namespace ColorMC.Android.GLRender;

public class GLSurface : GLSurfaceView, ISurfaceHolderCallback, GLSurfaceView.IRenderer, View.IOnTouchListener
{
    private readonly TapDetector _singleTapDetector;
    private readonly List<GameRender> displayList = [];

    private QuadRenderer qrender;
    private int renderWidth, renderHeight;
    private float XRenderRatio, YRenderRatio;

    public GameRender NowGame;

    public GLSurface(Context? context, DisplayMetrics display) : base(context)
    {
        _singleTapDetector = new(1, TapDetector.DelectionMethodBoth, display);
        SetEGLContextClientVersion(3);
        SetRenderer(this);
        SetOnTouchListener(this);
    }

    public void SetGame(GameRender game)
    {
        if (NowGame != null)
        {
            NowGame.SwitchOff();
            NowGame.SizeChange -= NowGame_SizeChange;
            NowGame.IsGrabbingChange -= NowGame_IsGrabbingChange;
        }

        NowGame = game;
        NowGame.SwitchOn();
        NowGame.SizeChange += NowGame_SizeChange;
        NowGame.IsGrabbingChange += NowGame_IsGrabbingChange;
    }

    private void NowGame_IsGrabbingChange()
    {
        Post(() =>
        {

        });
    }

    private void NowGame_SizeChange()
    {
        XRenderRatio = (float)NowGame.GameWidth / Width;
        YRenderRatio = (float)NowGame.GameHeight / Height;
    }

    public void OnDrawFrame(IGL10? gl)
    {
        GLES20.GlViewport(0, 0, Width, Height);
        GLES20.GlClearColor(0, 0, 0, 0);
        GLES20.GlClear(GLES20.GlColorBufferBit);

        if (NowGame.HaveBuffer)
        {
            if (NowGame.TexId == 0)
            {
                NowGame.BindTexture();
                displayList.Add(NowGame);
            }
            else
            {
                qrender.DrawTexture(NowGame.TexId, Width, Height,
                    NowGame.GameWidth, NowGame.GameHeight, NowGame.ShowType, NowGame.FlipY,
                    out renderWidth, out renderHeight);
            }
        }
        else if(NowGame.IsClose)
        {
            displayList.Remove(NowGame);
        }
    }

    protected override void OnDetachedFromWindow()
    {
        base.OnDetachedFromWindow();

        foreach (var item in displayList)
        {
            item.DeleteTexture();
        }
    }

    public void OnSurfaceChanged(IGL10? gl, int width, int height)
    {
        RenderNative.Available();

        qrender = new();
    }

    public void OnSurfaceCreated(IGL10? gl, Javax.Microedition.Khronos.Egl.EGLConfig? config)
    {

    }

    private bool DoTouch(float x, float y)
    {
        if (NowGame.ShowType == GameRender.DisplayType.Full)
        {
            NowGame.SendCursorPos(x * XRenderRatio, y * YRenderRatio);
            return true;
        }
        else
        {
            //检查是否在游戏窗口内
            float temp = Width / 2;
            float temp1 = renderWidth / 2;
            float widthStart = temp - temp1;
            float widthEnd = temp + temp1;
            float temp2 = Height / 2;
            float temp3 = renderHeight / 2;
            float heightStart = temp2 - temp3;
            float heightEnd = temp2 + temp3;

            if (x < widthStart || x > widthEnd)
            {
                return false;
            }
            if (y < heightStart || y > heightEnd)
            {
                return false;
            }

            float x1, y1;
            x1 = x - widthStart;
            y1 = y - heightStart;

            NowGame.SendCursorPos(x1 * XRenderRatio, y1 * YRenderRatio);
            return true;
        }
    }

    public bool OnTouch(View? v, MotionEvent? e)
    {
        // Looking for a mouse to handle, won't have an effect if no mouse exists.
        for (int i = 0; i < e.PointerCount; i++)
        {
            if (e.GetToolType(i) != MotionEventToolType.Mouse
                && e.GetToolType(i) != MotionEventToolType.Stylus)
            {
                continue;
            }

            // Mouse found
            if (NowGame.IsGrabbing)
            {
                return false;
            }
            //mouse event handled
            return DoTouch(e.GetX(i), e.GetY(i));
        }

        if (!NowGame.IsGrabbing)
        {
            if (!DoTouch(e.GetX(), e.GetY()))
            {
                return false;
            }
            //One android click = one MC click
            if (_singleTapDetector.OnTouchEvent(e))
            {
                NowGame.MouseEvent(LwjglKeycode.GLFW_MOUSE_BUTTON_LEFT, true);

                Task.Run(() =>
                {
                    Thread.Sleep(100);
                    NowGame.MouseEvent(LwjglKeycode.GLFW_MOUSE_BUTTON_LEFT, false);
                });
                return true;
            }
        }

        return true;
    }

    public override bool DispatchCapturedPointerEvent(MotionEvent? e)
    {
        NowGame.MouseX += e.GetX() * XRenderRatio;
        NowGame.MouseY += e.GetX() * YRenderRatio;

        // Position is updated by many events, hence it is send regardless of the event value
        NowGame.SendCursorPos();

        switch (e.ActionMasked)
        {
            case MotionEventActions.Move:
                return true;
            case MotionEventActions.ButtonPress:
                return SendMouseButtonUnconverted(e.ActionButton, true);
            case MotionEventActions.ButtonRelease:
                return SendMouseButtonUnconverted(e.ActionButton, false);
            case MotionEventActions.Scroll:
                NowGame.SendScroll(e.GetAxisValue(Axis.Hscroll), e.GetAxisValue(Axis.Vscroll));
                return true;
            default:
                return base.DispatchCapturedPointerEvent(e);
        }
    }

    public bool SendMouseButtonUnconverted(MotionEventButtonState button, bool status)
    {
        int glfwButton = -256;
        switch (button)
        {
            case MotionEventButtonState.Primary:
                glfwButton = LwjglKeycode.GLFW_MOUSE_BUTTON_LEFT;
                break;
            case MotionEventButtonState.Tertiary:
                glfwButton = LwjglKeycode.GLFW_MOUSE_BUTTON_MIDDLE;
                break;
            case MotionEventButtonState.Secondary:
                glfwButton = LwjglKeycode.GLFW_MOUSE_BUTTON_RIGHT;
                break;
        }
        if (glfwButton == -256) return false;
        NowGame.MouseEvent(glfwButton, status);
        return true;
    }
}
