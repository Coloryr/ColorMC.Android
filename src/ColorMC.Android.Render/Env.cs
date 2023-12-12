using ColorMC.Android.GLRender.Bridges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.GLRender;

public struct GLFWInputEvent
{
    public int type;
    public int i1;
    public int i2;
    public int i3;
    public int i4;
}

public record BasicRenderWindow
{
    public RenderState State;
    public IntPtr NativeSurface;
    public IntPtr NewNativeSurface;
}

public enum RenderState
{
    Alive = 0,
    NewWindow = 1
}

public enum RenderType
{
    GL4ES,
    ZINK,
    VIRGL,
    ANGEL,
    ANDROID
}

public record GameEnviron
{
    public readonly static GameEnviron Game = new();

    public IntPtr Window;
    public BasicRenderWindow? MainWindowBundle;
    public RenderType config_renderer;
    public bool force_vsync;
    public List<long> eventCounter; // Count the number of events to be pumped out
    public GLFWInputEvent[] events = new GLFWInputEvent[8000];
    public long outEventIndex; // Point to the current event that has yet to be pumped out to MC
    public long outTargetIndex; // Point to the newt index to stop by
    public long inEventIndex; // Point to the next event that has to be filled
    public long inEventCount; // Count registered right before pumping OUT events. Used as a cache.
    public double cursorX, cursorY, cLastX, cLastY;
    //public jmethodID method_accessAndroidClipboard;
    //public jmethodID method_onGrabStateChanged;
    //public jmethodID method_glftSetWindowAttrib;
    //public jmethodID method_internalWindowSizeChanged;
    //public jclass bridgeClazz;
    //public jclass vmGlfwClass;
    public bool isGrabbing;
    public byte[] keyDownBuffer;
    public byte[] mouseDownBuffer;
    //public JavaVM* runtimeJavaVMPtr;
    //public JNIEnv* runtimeJNIEnvPtr_JRE;
    //public JavaVM* dalvikJavaVMPtr;
    //public JNIEnv* dalvikJNIEnvPtr_ANDROID;
    public long showingWindow;
    public bool isInputReady, isCursorEntered, isUseStackQueueCall, isPumpingEvents;
    public int savedWidth, savedHeight;
//#define ADD_CALLBACK_WWIN(NAME) \
//    GLFW_invoke_##NAME##_func* GLFW_invoke_##NAME;
//    ADD_CALLBACK_WWIN(Char);
//    ADD_CALLBACK_WWIN(CharMods);
//    ADD_CALLBACK_WWIN(CursorEnter);
//    ADD_CALLBACK_WWIN(CursorPos);
//    ADD_CALLBACK_WWIN(FramebufferSize);
//    ADD_CALLBACK_WWIN(Key);
//    ADD_CALLBACK_WWIN(MouseButton);
//    ADD_CALLBACK_WWIN(Scroll);
//    ADD_CALLBACK_WWIN(WindowSize);

//#undef ADD_CALLBACK_WWIN
};

public static class Env
{
    public static bool IsAdrenoGraphics()
    {
        try
        {
            EGLBase.EglInit(IntPtr.Zero, RenderType.ANDROID);
            GLBase.Init(RenderType.ANDROID);

            return GLBase.IsAdrenoGraphics();
        }
        finally
        {
            EGLBase.DestroyEgl();
        }
    }
}