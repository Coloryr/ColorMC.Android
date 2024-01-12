using Android.Util;
using Android.Views;

namespace ColorMC.Android.GLRender;

public class TapDetector
{
    public const int DelectionMethodDown = 0x1;
    public const int DelectionMethodUp = 0x2;
    public const int DelectionMethodBoth = 0x3; //Unused for now

    private const int TapMinDelay = 10;
    private const int TapMaxDelay = 300;

    private int _tapStopSquarePx;

    private int _tapNumberToDetect;
    private int _currentTapNumber = 0;

    private int _detectionMethod;

    private long _lastEventTime = 0;
    private float _lastX = 9999;
    private float _lastY = 9999;

    /**
     * @param tapNumberToDetect How many taps are needed before onTouchEvent returns True.
     * @param detectionMethod Method used to detect touches. See DETECTION_METHOD constants above.
     */
    public TapDetector(int tapNumberToDetect, int detectionMethod, DisplayMetrics display)
    {
        _tapStopSquarePx = 
            (int)Math.Pow(100 * display.Density, 2);
        _detectionMethod = detectionMethod;
        //We expect both ACTION_DOWN and ACTION_UP for the DETECTION_METHOD_BOTH
        _tapNumberToDetect = DetectBothTouch() ? 2 * tapNumberToDetect : tapNumberToDetect;
    }

    /**
     * A function to call when you have a touch event.
     * @param e The MotionEvent to inspect
     * @return whether or not a X-tap happened for a pointer
     */
    public bool OnTouchEvent(MotionEvent e)
    {
        var eventAction = e.ActionMasked;
        int pointerIndex = -1;

        //Get the event to look forward
        if (DetectDownTouch())
        {
            if (eventAction == MotionEventActions.Down)
            {
                pointerIndex = 0;
            }
            else if (eventAction == MotionEventActions.PointerDown)
            {
                pointerIndex = e.ActionIndex;
            }
        }
        if (DetectUpTouch())
        {
            if (eventAction == MotionEventActions.Up)
            {
                pointerIndex = 0;
            }
            else if (eventAction == MotionEventActions.PointerUp)
            {
                pointerIndex = e.ActionIndex;
            }
        }

        if (pointerIndex == -1) return false; // Useless event

        //Store current event info
        float eventX = e.GetX(pointerIndex);
        float eventY = e.GetY(pointerIndex);
        long eventTime = e.EventTime;

        //Compute deltas
        long deltaTime = eventTime - _lastEventTime;
        int deltaX = (int)_lastX - (int)eventX;
        int deltaY = (int)_lastY - (int)eventY;

        //Store current event info to persist on next event
        _lastEventTime = eventTime;
        _lastX = eventX;
        _lastY = eventY;

        //Check for high enough speed and precision
        if (_currentTapNumber > 0)
        {
            if ((deltaTime < TapMinDelay || deltaTime > TapMaxDelay) ||
                ((deltaX * deltaX + deltaY * deltaY) > _tapStopSquarePx))
            {
                // We invalidate previous taps, not this one though
                _currentTapNumber = 0;
            }
        }

        //A worthy tap happened
        _currentTapNumber += 1;
        if (_currentTapNumber >= _tapNumberToDetect)
        {
            ResetTapDetectionState();
            return true;
        }

        //If not enough taps are reached
        return false;
    }

    /**
     * Reset the double tap values.
     */
    private void ResetTapDetectionState()
    {
        _currentTapNumber = 0;
        _lastEventTime = 0;
        _lastX = 9999;
        _lastY = 9999;
    }

    private bool DetectDownTouch()
    {
        return (_detectionMethod & DelectionMethodDown) == DelectionMethodDown;
    }

    private bool DetectUpTouch()
    {
        return (_detectionMethod & DelectionMethodUp) == DelectionMethodUp;
    }

    private bool DetectBothTouch()
    {
        return _detectionMethod == DelectionMethodBoth;
    }
}
