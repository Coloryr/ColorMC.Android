using Android.Content;
using Android.Opengl;
using Android.OS;
using Android.Text.Method;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Interop;
using Java.Util;
using Javax.Microedition.Khronos.Opengles;

namespace ColorMC.Android.GLRender;

public class GLSurface : GLSurfaceView, ISurfaceHolderCallback, GLSurfaceView.IRenderer, View.IOnTouchListener
{
    private TapDetector _singleTapDetector;
    private TapDetector _doubleTapDetector;

    private static QuadRenderer qrender;

    private int width, height;

    private GameRender NowGame;

    private float XRenderRatio, YRenderRatio;
    private bool Full;

    private float mPrevX, mPrevY;
    private int mCurrentPointerID = -1000;
    private int mGuiScale;

    public GLSurface(Context? context, DisplayMetrics display) : this(context, attributeSet: null)
    {
        SetOnTouchListener(this);

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
        }

        NowGame = game;
        NowGame.SizeChange += NowGame_SizeChange;
    }

    private void NowGame_SizeChange()
    {
        XRenderRatio = (float)NowGame.RenderWidth / width;
        YRenderRatio = (float)NowGame.RenderHeight / height;
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
                    NowGame.RenderWidth, NowGame.RenderHeight, Full);
            }
        }
    }

    public void OnSurfaceChanged(IGL10? gl, int width, int height)
    {
        this.width = width;
        this.height = height;

        RenderNative.Available();

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
            NowGame.SetSize(width, height);
        }
        else
        {
            Toast.MakeText(Context!, "错误的输入", ToastLength.Short).Show();
        }
    }

    private bool DoTouch(float x, float y)
    {
        if (Full)
        {
            NowGame.SendCursorPos(x * XRenderRatio, y * YRenderRatio);
            return true;
        }
        else
        {
            //检查是否在游戏窗口内
            float temp = Width / 2;
            float temp1 = NowGame.RenderWidth / 2;
            float widthStart = temp - temp1;
            float widthEnd = temp + temp1;
            float temp2 = Height / 2;
            float temp3 = NowGame.RenderHeight / 2;
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
            x1 = x - temp + temp1;
            y1 = y - temp2 + temp3;

            NowGame.SendCursorPos(x1, y1);
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
                NowGame.MouseEvent(Keycode.GLFW_MOUSE_BUTTON_LEFT, true);

                Task.Run(() =>
                {
                    Thread.Sleep(100);
                    NowGame.MouseEvent(Keycode.GLFW_MOUSE_BUTTON_LEFT, false);
                });
                return true;
            }
        }

        // Check double tap state, used for the hotbar
        bool hasDoubleTapped = _doubleTapDetector.OnTouchEvent(e);

        switch (e.ActionMasked)
        {
            case MotionEventActions.Move:
                int pointerCount = e.PointerCount;

                // In-menu interactions
                if (!NowGame.IsGrabbing)
                {
                    // Touch hover
                    if (pointerCount == 1)
                    {
                        NowGame.SendCursorPos();
                        mPrevX = e.GetX();
                        mPrevY = e.GetY();
                        break;
                    }

                    //// Scrolling feature
                    //if (LauncherPreferences.PREF_DISABLE_GESTURES) break;
                    //// The pointer count can never be 0, and it is not 1, therefore it is >= 2
                    //int hScroll = ((int)(e.getX() - mScrollLastInitialX)) / FINGER_SCROLL_THRESHOLD;
                    //int vScroll = ((int)(e.getY() - mScrollLastInitialY)) / FINGER_SCROLL_THRESHOLD;

                    //if (vScroll != 0 || hScroll != 0)
                    //{
                    //    CallbackBridge.sendScroll(hScroll, vScroll);
                    //    mScrollLastInitialX = e.getX();
                    //    mScrollLastInitialY = e.getY();
                    //}
                    break;
                }

                // Camera movement
                //int pointerIndex = e.FindPointerIndex(mCurrentPointerID);
                //int hudKeyHandled = HandleGuiBar((int)e.getX(), (int)e.getY());
                //// Start movement, due to new pointer or loss of pointer
                //if (pointerIndex == -1 || mLastPointerCount != pointerCount || !mShouldBeDown)
                //{
                //    if (hudKeyHandled != -1) break; //No pointer attribution on hotbar

                //    mShouldBeDown = true;
                //    mCurrentPointerID = e.getPointerId(0);
                //    mPrevX = e.getX();
                //    mPrevY = e.getY();
                //    break;
                //}
                //// Continue movement as usual
                //if (hudKeyHandled == -1)
                //{ //No camera on hotbar
                //    CallbackBridge.mouseX += (e.getX(pointerIndex) - mPrevX) * mSensitivityFactor;
                //    CallbackBridge.mouseY += (e.getY(pointerIndex) - mPrevY) * mSensitivityFactor;
                //}

                //mPrevX = e.getX(pointerIndex);
                //mPrevY = e.getY(pointerIndex);

                //CallbackBridge.sendCursorPos(CallbackBridge.mouseX, CallbackBridge.mouseY);
                break;

            case MotionEventActions.Down: // 0
                //hudKeyHandled = handleGuiBar((int)e.getX(), (int)e.getY());
                //boolean isTouchInHotbar = hudKeyHandled != -1;
                //if (isTouchInHotbar)
                //{
                //    sendKeyPress(hudKeyHandled);
                //    if (hasDoubleTapped && hudKeyHandled == mLastHotbarKey && !PREF_DISABLE_SWAP_HAND)
                //    {
                //        //Prevent double tapping Event on two different slots
                //        sendKeyPress(Keycode.GLFW_KEY_F);
                //    }
                //    else
                //    {
                //        mHandler.sendEmptyMessageDelayed(MSG_DROP_ITEM_BUTTON_CHECK, 350);
                //    }

                //    CallbackBridge.sendCursorPos(CallbackBridge.mouseX, CallbackBridge.mouseY);
                //    mLastHotbarKey = hudKeyHandled;
                //    break;
                //}

                //CallbackBridge.sendCursorPos(CallbackBridge.mouseX, CallbackBridge.mouseY);
                //mPrevX = e.getX();
                //mPrevY = e.getY();

                //if (CallbackBridge.isGrabbing())
                //{
                //    mCurrentPointerID = e.getPointerId(0);
                //    // It cause hold left mouse while moving camera
                //    mInitialX = CallbackBridge.mouseX;
                //    mInitialY = CallbackBridge.mouseY;
                //    mHandler.sendEmptyMessageDelayed(MSG_LEFT_MOUSE_BUTTON_CHECK, LauncherPreferences.PREF_LONGPRESS_TRIGGER);
                //}
                //mLastHotbarKey = hudKeyHandled;
                break;

            case MotionEventActions.Up: // 1
            case MotionEventActions.Cancel: // 3
                //mShouldBeDown = false;
                //mCurrentPointerID = -1;

                //hudKeyHandled = handleGuiBar((int)e.getX(), (int)e.getY());
                //isTouchInHotbar = hudKeyHandled != -1;
                //// We only treat in world events
                //if (!CallbackBridge.isGrabbing()) break;

                //// Stop the dropping of items
                //sendKeyPress(Keycode.GLFW_KEY_Q, 0, false);
                //mHandler.removeMessages(MSG_DROP_ITEM_BUTTON_CHECK);

                //// Remove the mouse left button
                //if (triggeredLeftMouseButton)
                //{
                //    sendMouseButton(Keycode.GLFW_MOUSE_BUTTON_LEFT, false);
                //    triggeredLeftMouseButton = false;
                //    break;
                //}
                //mHandler.removeMessages(MSG_LEFT_MOUSE_BUTTON_CHECK);

                //// In case of a short click, just send a quick right click
                //if (!LauncherPreferences.PREF_DISABLE_GESTURES &&
                //        MathUtils.dist(mInitialX, mInitialY, CallbackBridge.mouseX, CallbackBridge.mouseY) < FINGER_STILL_THRESHOLD)
                //{
                //    sendMouseButton(Keycode.GLFW_MOUSE_BUTTON_RIGHT, true);
                //    sendMouseButton(Keycode.GLFW_MOUSE_BUTTON_RIGHT, false);
                //}
                break;

            case MotionEventActions.PointerDown: // 5
                //TODO Hey we could have some sort of middle click detection ?

                //mScrollLastInitialX = e.getX();
                //mScrollLastInitialY = e.getY();
                ////Checking if we are pressing the hotbar to select the item
                //hudKeyHandled = handleGuiBar((int)e.getX(e.getPointerCount() - 1), (int)e.getY(e.getPointerCount() - 1));
                //if (hudKeyHandled != -1)
                //{
                //    sendKeyPress(hudKeyHandled);
                //    if (hasDoubleTapped && hudKeyHandled == mLastHotbarKey)
                //    {
                //        //Prevent double tapping Event on two different slots
                //        sendKeyPress(Keycode.GLFW_KEY_F);
                //    }
                //}

                //mLastHotbarKey = hudKeyHandled;
                break;

        }

        // Actualise the pointer count
        //mLastPointerCount = e.getPointerCount();

        return true;
    }

    //public int handleGuiBar(int x, int y)
    //{
    //    if (!NowGame.IsGrabbing) return -1;

    //    int barHeight = mcscale(20);
    //    int barY = NowGame.RenderHeight - barHeight;
    //    if (y < barY) return -1;

    //    int barWidth = mcscale(180);
    //    int barX = (NowGame.RenderWidth / 2) - (barWidth / 2);
    //    if (x < barX || x >= barX + barWidth) return -1;

    //    return HOTBAR_KEYS[(int)net.kdt.pojavlaunch.utils.MathUtils.map(x, barX, barX + barWidth, 0, 9)];
    //}

    //private int mcscale(int input)
    //{
    //    return (int)((mGuiScale * input) / mScaleFactor);
    //}
}
