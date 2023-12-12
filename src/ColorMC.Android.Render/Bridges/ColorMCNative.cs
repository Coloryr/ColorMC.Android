using System.Runtime.InteropServices;

namespace ColorMC.Android.GLRender.Bridges;

public static class ColorMCNative
{
    //void load_vulkan()
    public delegate void load_vulkan();

    public static load_vulkan LoadVulkan;

    public static void Load(IntPtr dl_handle)
    {
        LoadVulkan = Marshal.GetDelegateForFunctionPointer<load_vulkan>(NativeLoader.GetProcAddress(dl_handle, "load_vulkan"));
    }
}
