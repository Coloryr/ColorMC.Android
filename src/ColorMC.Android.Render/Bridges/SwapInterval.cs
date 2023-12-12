using System.Runtime.InteropServices;

namespace ColorMC.Android.GLRender.Bridges;

[StructLayout(LayoutKind.Sequential)]
public struct AndroidNativeBase
{
    public delegate void Ref(AndroidNativeBase ba);

    /* a magic value defined by the actual EGL native type */
    public int magic;
    /* the sizeof() of the actual EGL native type */
    public int version;
    [MarshalAs(UnmanagedType.FunctionPtr, SizeConst = 4)]
    public IntPtr[] reserved;
    /* reference-counting interface */
    public Ref incRef;
    public Ref decRef;
}

[StructLayout(LayoutKind.Sequential)]
public struct ANativeWindowReal
{
    public unsafe delegate int SwapInterval(IntPtr window, int interval);

    public AndroidNativeBase common;
    /* flags describing some attributes of this surface or its updater */
    public uint flags;
    /* min swap interval supported by this updated */
    public int minSwapInterval;
    /* max swap interval supported by this updated */
    public int maxSwapInterval;
    /* horizontal and vertical resolution in DPI */
    public float xdpi;
    public float ydpi;
    /* Some storage reserved for the OEM's driver. */
    [MarshalAs(UnmanagedType.SysInt, SizeConst = 4)]
    public int[] oem;
    public SwapInterval setSwapInterval;
    public IntPtr dequeueBuffer_DEPRECATED;
    public IntPtr lockBuffer_DEPRECATED;
    public IntPtr queueBuffer_DEPRECATED;
    public IntPtr query;
    public IntPtr perform;
    public IntPtr cancelBuffer_DEPRECATED;
    public IntPtr dequeueBuffer;
    public IntPtr queueBuffer;
    public IntPtr cancelBuffer;
};

public static class SwapInterval
{
    public static void SetNativeWindowSwapInterval(IntPtr nativeWindow, int swapInterval) 
    {
        if (Environment.GetEnvironmentVariable("POJAV_VSYNC_IN_ZINK") != null) 
        {
            return;
        }
        int ANDROID_NATIVE_WINDOW_MAGIC = '_' << 24 | 'w' << 16 | 'n' << 8 | 'd';
        ANativeWindowReal nativeWindowReal = Marshal.PtrToStructure<ANativeWindowReal>(nativeWindow);
        if (nativeWindowReal.common.magic != ANDROID_NATIVE_WINDOW_MAGIC) 
        {
            RenderLog.Warn("SwapIntervalNoEGL", $"ANativeWindow magic does not match. " +
                $"Expected {ANDROID_NATIVE_WINDOW_MAGIC}, got {nativeWindowReal.common.magic}");
            return;
        }
        if (nativeWindowReal.common.version != Marshal.SizeOf<ANativeWindowReal>()) 
        {
            RenderLog.Warn("SwapIntervalNoEGL", $"ANativeWindow version does not match. " +
                $"Expected {Marshal.SizeOf<ANativeWindowReal>()}, got {nativeWindowReal.common.version}");
            return;
        }
        int error;
        if ((error = nativeWindowReal.setSwapInterval(nativeWindow, swapInterval)) != 0)
        {
            RenderLog.Warn("SwapIntervalNoEGL", $"Failed to set swap interval: {-error}");
        }
    }
}
