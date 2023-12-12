using System.Runtime.InteropServices;

namespace ColorMC.Android.GLRender.Bridges;

/// <summary>
/// 本地加载DLL
/// </summary>
public static class NativeLoader
{
    static class LinuxImports
    {
        [DllImport("libdl.so.2")]
        private static extern IntPtr dlopen(string path, int flags);

        [DllImport("libdl.so.2")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl.so.2")]
        private static extern IntPtr dlerror();

        public static void Init()
        {
            DlOpen = dlopen;
            DlSym = dlsym;
            DlError = dlerror;
        }
    }

    static class AndroidImports
    {
        [DllImport("libdl.so")]
        private static extern IntPtr dlopen(string path, int flags);

        [DllImport("libdl.so")]
        private static extern IntPtr dlsym(IntPtr handle, string symbol);

        [DllImport("libdl.so")]
        private static extern IntPtr dlerror();

        public static void Init()
        {
            DlOpen = dlopen;
            DlSym = dlsym;
            DlError = dlerror;
        }
    }

    static NativeLoader()
    {
        AndroidImports.Init();
    }

    private static Func<string, int, IntPtr> DlOpen;
    private static Func<IntPtr, string, IntPtr> DlSym;
    private static Func<IntPtr> DlError;
    // ReSharper restore InconsistentNaming

    static string? DlErrorString() => Marshal.PtrToStringAnsi(DlError());

    public static IntPtr LoadLibrary(string dll)
    {
        var handle = DlOpen(dll, 1);
        if (handle == IntPtr.Zero)
            throw new Exception(DlErrorString());
        return handle;
    }

    public static IntPtr GetProcAddress(IntPtr dll, string proc)
    {
        var ptr = DlSym(dll, proc);
        if (ptr == IntPtr.Zero)
            throw new Exception(DlErrorString());
        return ptr;
    }
}