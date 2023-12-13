using System.Runtime.InteropServices;

namespace ColorMC.Android.GLRender.Bridges;

public static class ColorMCNative
{
    //EXTERNAL_API void load_vulkan()
    public delegate void load_vulkan();
    //EXTERNAL_API void set_vulkan_ptr(void* ptr)
    public delegate void set_vulkan_ptr(IntPtr ptr);

    public static load_vulkan LoadVulkan;
    public static set_vulkan_ptr SetVulkanPtr;

    public static void Load(IntPtr dl_handle)
    {
        LoadVulkan = Marshal.GetDelegateForFunctionPointer<load_vulkan>(NativeLoader.GetProcAddress(dl_handle, "load_vulkan"));
        SetVulkanPtr = Marshal.GetDelegateForFunctionPointer<set_vulkan_ptr>(NativeLoader.GetProcAddress(dl_handle, "set_vulkan_ptr"));
    }
}
