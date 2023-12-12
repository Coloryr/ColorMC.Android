#include <jni.h>
#include <assert.h>
#include <dlfcn.h>

#include <stdbool.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <sys/types.h>
#include <unistd.h>

#include <EGL/egl.h>
#include <GLES2/gl2.h>

#include <android/log.h>

#include <android/native_window.h>
#include <android/native_window_jni.h>
#include <android/rect.h>
#include <string.h>
#include <android/dlext.h>

#include <EGL/egl.h>

#include "nsbypass.h"
#include "hook.h"

// This means that the function is an external API and that it will be used
#define EXTERNAL_API __attribute__((used))
// This means that you are forced to have this function/variable for ABI compatibility
#define ABI_COMPAT __attribute__((unused))

#define LOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, "ColorMC", __VA_ARGS__))
#define LOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, "ColorMC", __VA_ARGS__))

void* load_turnip_vulkan() {
    const char* native_dir = getenv("POJAV_NATIVEDIR");
    const char* cache_dir = getenv("TMPDIR");
    if (!linker_ns_load(native_dir)) return NULL;
    void* turnip_driver_handle = linker_ns_dlopen("libvulkan_freedreno.so", RTLD_LOCAL | RTLD_NOW);
    if (turnip_driver_handle == NULL) {
        LOGW("AdrenoSupp: Failed to load Turnip!\n%s\n", dlerror());
        return NULL;
    }
    void* dl_android = linker_ns_dlopen("libdl_android.so", RTLD_LOCAL | RTLD_LAZY);
    if (dl_android == NULL) {
        dlclose(turnip_driver_handle);
        return NULL;
    }
    void* android_get_exported_namespace = dlsym(dl_android, "android_get_exported_namespace");
    if (android_get_exported_namespace == NULL) {
        dlclose(dl_android);
        dlclose(turnip_driver_handle);
        return NULL;
    }
    app__pojav_linkerhook_pass_handles(turnip_driver_handle, android_dlopen_ext, android_get_exported_namespace);
    void* libvulkan = linker_ns_dlopen_unique(cache_dir, "libvulkan.so", RTLD_LOCAL | RTLD_NOW);
    return libvulkan;
}

static void set_vulkan_ptr(void* ptr) {
    char envval[64];
    sprintf(envval, "%"PRIxPTR, (uintptr_t)ptr);
    setenv("VULKAN_PTR", envval, 1);
}

EXTERNAL_API void load_vulkan() {
    if (android_get_device_api_level() >= 28) { // the loader does not support below that
        void* result = load_turnip_vulkan();
        if (result != NULL) {
            LOGI("AdrenoSupp: Loaded Turnip, loader address: %p\n", result);
            set_vulkan_ptr(result);
            return;
        }
    }
    LOGI("OSMDroid: loading vulkan regularly...\n");
    void* vulkan_ptr = dlopen("libvulkan.so", RTLD_LAZY | RTLD_LOCAL);
    LOGI("OSMDroid: loaded vulkan, ptr=%p\n", vulkan_ptr);
    set_vulkan_ptr(vulkan_ptr);
}

