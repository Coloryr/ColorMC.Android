using Android.Content;
using Android.Opengl;
using Android.OS;
using Android.Util;
using Android.Views;
using Javax.Microedition.Khronos.Opengles;

namespace ColorMC.Android.GLRender;

public class GLSurface : GLSurfaceView, ISurfaceHolderCallback, GLSurfaceView.IRenderer, View.IOnTouchListener
{
    private TapDetector _singleTapDetector;
    private TapDetector _doubleTapDetector;

    private static QuadRenderer qrender;

    private int renderWidth, renderHeight;

    public GameRender NowGame;

    private float XRenderRatio, YRenderRatio;

    private float mPrevX, mPrevY;
    private float mScrollLastInitialX, mScrollLastInitialY;
    private int mCurrentPointerID = -1000;
    private int mGuiScale;
    private int mLastPointerCount = 0;
    private bool mShouldBeDown = false;
    private float mSensitivityFactor;
    private float mInitialX, mInitialY;

    public readonly int FINGER_STILL_THRESHOLD;
    public readonly int FINGER_SCROLL_THRESHOLD;

    public static int MSG_LEFT_MOUSE_BUTTON_CHECK = 1028;
    public static int MSG_DROP_ITEM_BUTTON_CHECK = 1029;

    private bool triggeredLeftMouseButton = false;

    private class HandlerClass(GLSurface surface, Looper looper) : Handler(looper)
    {
        public override void HandleMessage(Message? msg)
        {
            if (msg?.What == MSG_LEFT_MOUSE_BUTTON_CHECK)
            {
                //if (LauncherPreferences.PREF_DISABLE_GESTURES) return;
                float x = surface.NowGame.MouseX;
                float y = surface.NowGame.MouseY;
                if (surface.NowGame.IsGrabbing &&
                        MathUtils.dist(x, y, surface.mInitialX, surface.mInitialY) < surface.FINGER_STILL_THRESHOLD)
                {
                    surface.triggeredLeftMouseButton = true;
                    surface.NowGame.MouseEvent(LwjglKeycode.GLFW_MOUSE_BUTTON_LEFT, true);
                }
                return;
            }
        }
    }

    private Handler mHandler;

    public GLSurface(Context? context, DisplayMetrics display) : this(context, attributeSet: null)
    {
        SetOnTouchListener(this);

        mHandler = new HandlerClass(this, Looper.MainLooper);

        FINGER_STILL_THRESHOLD = (int)(9 * display.Density);
        FINGER_SCROLL_THRESHOLD = (int)(6 * display.Density);

        mSensitivityFactor = (float)(1.4 * (1080f / display.HeightPixels));

        _singleTapDetector = new TapDetector(1, TapDetector.DelectionMethodBoth, display);
        _doubleTapDetector = new TapDetector(2, TapDetector.DelectionMethodDown, display);
    }

    public GLSurface(Context? context, IAttributeSet? attributeSet) : base(context, attributeSet)
    {
        SetEGLContextClientVersion(3);
        SetRenderer(this);
    }

    public void SetGame(GameRender game)
    {
        if (NowGame != null && NowGame != game)
        {
            NowGame.SizeChange -= NowGame_SizeChange;
            NowGame.IsGrabbingChange -= NowGame_IsGrabbingChange;
        }

        NowGame = game;
        NowGame.SizeChange += NowGame_SizeChange;
        NowGame.IsGrabbingChange += NowGame_IsGrabbingChange;
    }

    private void NowGame_IsGrabbingChange()
    {
        Post(() =>
        {
            UpdateGrabState(NowGame.IsGrabbing);
        });
    }

    private void UpdateGrabState(bool isGrabbing)
    {
        bool hasPointerCapture = HasPointerCapture;
        if (isGrabbing)
        {
            if (!hasPointerCapture)
            {
                RequestFocus();
                if (HasWindowFocus) RequestPointerCapture();
                // Otherwise, onWindowFocusChanged() would get called.
            }
            return;
        }

        if (hasPointerCapture)
        {
            ReleasePointerCapture();
            ClearFocus();
        }
    }

    private void NowGame_SizeChange()
    {
        XRenderRatio = (float)NowGame.GameWidth / renderWidth;
        YRenderRatio = (float)NowGame.GameHeight / renderHeight;
    }

    public void OnDrawFrame(IGL10? gl)
    {
        GLES20.GlViewport(0, 0, renderWidth, renderHeight);
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
                qrender.DrawTexture(NowGame.TexId, renderWidth, renderHeight,
                    NowGame.GameWidth, NowGame.GameHeight, NowGame.ShowType, NowGame.FlipY,
                    out renderWidth, out renderHeight);
            }
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

        //Getting scaled position from the event
        /* Tells if a double tap happened [MOUSE GRAB ONLY]. Doesn't tell where though. */
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

        // Check double tap state, used for the hotbar
        //bool hasDoubleTapped = _doubleTapDetector.OnTouchEvent(e);

        //switch (e.ActionMasked)
        //{
        //    case MotionEventActions.Move:
        //        int pointerCount = e.PointerCount;

        //        // In-menu interactions
        //        if (!NowGame.IsGrabbing)
        //        {
        //            // Touch hover
        //            if (pointerCount == 1)
        //            {
        //                NowGame.SendCursorPos();
        //                mPrevX = e.GetX();
        //                mPrevY = e.GetY();
        //                break;
        //            }

        //            // Scrolling feature
        //            //if (LauncherPreferences.PREF_DISABLE_GESTURES) break;
        //            // The pointer count can never be 0, and it is not 1, therefore it is >= 2
        //            int hScroll = ((int)(e.GetX() - mScrollLastInitialX)) / FINGER_SCROLL_THRESHOLD;
        //            int vScroll = ((int)(e.GetY() - mScrollLastInitialY)) / FINGER_SCROLL_THRESHOLD;

        //            if (vScroll != 0 || hScroll != 0)
        //            {
        //                NowGame.SendScroll(hScroll, vScroll);
        //                mScrollLastInitialX = e.GetX();
        //                mScrollLastInitialY = e.GetY();
        //            }
        //            break;
        //        }

        //        // Camera movement
        //        int pointerIndex = e.FindPointerIndex(mCurrentPointerID);
        //        // Start movement, due to new pointer or loss of pointer
        //        if (pointerIndex == -1 || mLastPointerCount != pointerCount || !mShouldBeDown)
        //        {
        //            mShouldBeDown = true;
        //            mCurrentPointerID = e.GetPointerId(0);
        //            mPrevX = e.GetX();
        //            mPrevY = e.GetY();
        //            break;
        //        }
        //        // Continue movement as usual
        //        NowGame.MouseX += (e.GetX(pointerIndex) - mPrevX) * mSensitivityFactor;
        //        NowGame.MouseY += (e.GetY(pointerIndex) - mPrevY) * mSensitivityFactor;

        //        mPrevX = e.GetX(pointerIndex);
        //        mPrevY = e.GetY(pointerIndex);

        //        NowGame.SendCursorPos();
        //        break;

        //    case MotionEventActions.Down: // 0
        //        NowGame.SendCursorPos();
        //        mPrevX = e.GetX();
        //        mPrevY = e.GetY();

        //        if (NowGame.IsGrabbing)
        //        {
        //            mCurrentPointerID = e.GetPointerId(0);
        //            // It cause hold left mouse while moving camera
        //            mInitialX = NowGame.MouseX;
        //            mInitialY = NowGame.MouseY;
        //            mHandler.SendEmptyMessageDelayed(MSG_LEFT_MOUSE_BUTTON_CHECK, 2000);

        //            //LauncherPreferences.PREF_LONGPRESS_TRIGGER
        //        }
        //        break;

        //    case MotionEventActions.Up: // 1
        //    case MotionEventActions.Cancel: // 3
        //        mShouldBeDown = false;
        //        mCurrentPointerID = -1;

        //        // We only treat in world events
        //        if (!NowGame.IsGrabbing) break;

        //        // Stop the dropping of items

        //        // Remove the mouse left button
        //        if (triggeredLeftMouseButton)
        //        {
        //            NowGame.MouseEvent(LwjglKeycode.GLFW_MOUSE_BUTTON_LEFT, false);
        //            triggeredLeftMouseButton = false;
        //            break;
        //        }
        //        mHandler.RemoveMessages(MSG_LEFT_MOUSE_BUTTON_CHECK);

        //        // In case of a short click, just send a quick right click
        //        if (MathUtils.dist(mInitialX, mInitialY, NowGame.MouseX, NowGame.MouseY) < FINGER_STILL_THRESHOLD)
        //        {
        //            NowGame.MouseEvent(LwjglKeycode.GLFW_MOUSE_BUTTON_RIGHT, true);
        //            NowGame.MouseEvent(LwjglKeycode.GLFW_MOUSE_BUTTON_RIGHT, false);
        //        }
        //        break;

        //    case MotionEventActions.PointerDown: // 5
        //        //TODO Hey we could have some sort of middle click detection ?

        //        mScrollLastInitialX = e.GetX();
        //        mScrollLastInitialY = e.GetY();
        //        break;

        //}

        //// Actualise the pointer count
        //mLastPointerCount = e.PointerCount;

        return true;
    }

    public override bool DispatchGenericMotionEvent(MotionEvent? e)
    {
        int mouseCursorIndex = -1;
        //TODO 游戏手柄
        //        if (Gamepad.isGamepadEvent(event)){
        //        if (mGamepad == null)
        //        {
        //            mGamepad = new Gamepad(this, e.getDevice());
        //            }

        //mInputManager.handleMotionEventInput(getContext(), event, mGamepad);
        //            return true;
        //}

        for (int i = 0; i < e.PointerCount; i++)
        {
            if (e.GetToolType(i) != MotionEventToolType.Mouse && e.GetToolType(i) != MotionEventToolType.Stylus) continue;
            // Mouse found
            mouseCursorIndex = i;
            break;
        }
        if (mouseCursorIndex == -1) return false; // we cant consoom that, theres no mice!

        // Make sure we grabbed the mouse if necessary
        UpdateGrabState(NowGame.IsGrabbing);

        switch (e.ActionMasked)
        {
            case MotionEventActions.HoverMove:
                NowGame.MouseX = (e.GetX(mouseCursorIndex) * XRenderRatio);
                NowGame.MouseY = (e.GetY(mouseCursorIndex) * YRenderRatio);
                NowGame.SendCursorPos(NowGame.MouseX, NowGame.MouseY);
                return true;
            case MotionEventActions.Scroll:
                NowGame.SendScroll(e.GetAxisValue(Axis.Hscroll), e.GetAxisValue(Axis.Vscroll));
                return true;
            case MotionEventActions.ButtonPress:
                return SendMouseButtonUnconverted(e.ActionButton, true);
            case MotionEventActions.ButtonRelease:
                return SendMouseButtonUnconverted(e.ActionButton, false);
            default:
                return base.DispatchGenericMotionEvent(e);
        }
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

    /** The event for keyboard/ gamepad button inputs */
    //public bool processKeyEvent(KeyEvent e)
    //{
    //    //Toast.makeText(this, event.toString(),Toast.LENGTH_SHORT).show();
    //    //Toast.makeText(this, event.getDevice().toString(), Toast.LENGTH_SHORT).show();

    //    //Filtering useless events by order of probability
    //    Keycode eventKeycode = e.KeyCode;
    //    if (eventKeycode == KeyEvent.KEYCODE_UNKNOWN) return true;
    //    if (eventKeycode == KeyEvent.KEYCODE_VOLUME_DOWN) return false;
    //    if (eventKeycode == KeyEvent.KEYCODE_VOLUME_UP) return false;
    //    if (e.getRepeatCount() != 0) return true;
    //    int action = e.getAction();
    //    if (action == KeyEvent.ACTION_MULTIPLE) return true;
    //    // Ignore the cancelled up events. They occur when the user switches layouts.
    //    // In accordance with https://developer.android.com/reference/android/view/KeyEvent#FLAG_CANCELED
    //    if (action == KeyEvent.ACTION_UP &&
    //            (e.getFlags() & KeyEvent.FLAG_CANCELED) != 0) return true;

    //    //Sometimes, key events comes from SOME keys of the software keyboard
    //    //Even weirder, is is unknown why a key or another is selected to trigger a keyEvent
    //    if ((e.getFlags() & KeyEvent.FLAG_SOFT_KEYBOARD) == KeyEvent.FLAG_SOFT_KEYBOARD)
    //    {
    //        if (eventKeycode == KeyEvent.KEYCODE_ENTER) return true; //We already listen to it.
    //        touchCharInput.dispatchKeyEvent(e);
    //        return true;
    //    }

    //    //Sometimes, key events may come from the mouse
    //    if (e.getDevice() != null
    //            && ((e.getSource() & InputDevice.SOURCE_MOUSE_RELATIVE) == InputDevice.SOURCE_MOUSE_RELATIVE
    //            || (e.getSource() & InputDevice.SOURCE_MOUSE) == InputDevice.SOURCE_MOUSE))
    //    {

    //        if (eventKeycode == KeyEvent.KEYCODE_BACK)
    //        {
    //            sendMouseButton(LwjglGlfwKeycode.GLFW_MOUSE_BUTTON_RIGHT, e.getAction() == KeyEvent.ACTION_DOWN);
    //            return true;
    //        }
    //    }

    //    if (Gamepad.isGamepadEvent(e))
    //    {
    //        if (mGamepad == null)
    //        {
    //            mGamepad = new Gamepad(this, e.getDevice());
    //        }

    //        mInputManager.handleKeyEventInput(getContext(), e, mGamepad);
    //        return true;
    //    }

    //    int index = EfficientAndroidLWJGLKeycode.getIndexByKey(eventKeycode);
    //    if (EfficientAndroidLWJGLKeycode.containsIndex(index))
    //    {
    //        EfficientAndroidLWJGLKeycode.execKey(e, index);
    //        return true;
    //    }

    //    // Some events will be generated an infinite number of times when no consumed
    //    return (e.getFlags() & KeyEvent.FLAG_FALLBACK) == KeyEvent.FLAG_FALLBACK;
    //}

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

    public override void OnWindowFocusChanged(bool hasWindowFocus)
    {
        base.OnWindowFocusChanged(hasWindowFocus);
        if (HasWindowFocus && NowGame.IsGrabbing)
        {
            RequestPointerCapture();
        }
    }
}
