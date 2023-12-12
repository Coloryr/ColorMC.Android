using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.GLRender;

public struct ANativeWindowBuffer
{
    /// The number of pixels that are shown horizontally.
    public int width;

    /// The number of pixels that are shown vertically.
    public int height;

    /// The number of *pixels* that a line in the buffer takes in
    /// memory. This may be >= width.
    public int stride;

    /// The format of the buffer. One of AHardwareBuffer_Format.
    public int format;

    /// The actual bits.
    public IntPtr bits;

    /// Do not touch.
    [MarshalAs(UnmanagedType.SysUInt, SizeConst = 6)]
    public uint[] reserved;
}

public struct ARect
{
    /// Minimum X coordinate of the rectangle.
    public int left;
    /// Minimum Y coordinate of the rectangle.
    public int top;
    /// Maximum X coordinate of the rectangle.
    public int right;
    /// Maximum Y coordinate of the rectangle.
    public int bottom;
}

public partial class NativeWindow
{
    public const int WINDOW_FORMAT_RGBA_8888 = 1;
    public const int WINDOW_FORMAT_RGBX_8888 = 2;
    public const int WINDOW_FORMAT_RGB_565 = 4;

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_acquire")]
    internal static unsafe partial void Acquire(IntPtr window);

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_release")]
    internal static unsafe partial void Release(IntPtr window);

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_getWidth")]
    internal static unsafe partial int GetWidth(IntPtr window);

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_getHeight")]
    internal static unsafe partial int GetHeight(IntPtr window);

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_getFormat")]
    internal static unsafe partial int GetFormat(IntPtr window);

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_setBuffersGeometry")]
    internal static unsafe partial int SetBuffersGeometry(IntPtr window, int width, int height, int format);

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_lock")]
    //ANativeWindowBuffer* ARect*
    internal static unsafe partial int Lock(IntPtr window, IntPtr outBuffer, IntPtr inOutDirtyBounds);

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_unlockAndPost")]
    internal static unsafe partial int UnlockAndPost(IntPtr window);

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_setBuffersTransform")]
    internal static unsafe partial int setBuffersTransform(IntPtr window, int transform);

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_setBuffersDataSpace")]
    internal static unsafe partial int setBuffersDataSpace(IntPtr window, int dataSpace);

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_getBuffersDataSpace")]
    internal static unsafe partial int getBuffersDataSpace(IntPtr window);

    [LibraryImport("libandroid.so", EntryPoint = "ANativeWindow_fromSurface")]
    internal static unsafe partial IntPtr FromSurface(IntPtr env, IntPtr suface);
}
