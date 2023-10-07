#include "jni.h"
#include "log.h"

#include <stdbool.h>
#include <dlfcn.h>

JavaVM* jvm;
JNIEnv* env;

/*此简单函数返回平台 ABI，此动态本地库为此平台 ABI 进行编译。*/
const char* get_platform_ABI()
{
#if defined(__arm__)
#if defined(__ARM_ARCH_7A__)
#if defined(__ARM_NEON__)
#define ABI "armeabi-v7a/NEON"
#else
#define ABI "armeabi-v7a"
#endif
#else
#define ABI "armeabi"
#endif
#elif defined(__i386__)
#define ABI "x86"
#else
#define ABI "unknown"
#endif
	LOGI("This dynamic shared library is compiled with ABI: %s", ABI);
	return "This native library is compiled with ABI: %s" ABI ".";
}

JNIEXPORT jint JNI_OnLoad(JavaVM* vm, __attribute((unused)) void* reserved) {
    jvm = vm;
    (*vm)->GetEnv(vm, (void**)&env, JNI_VERSION_1_4);
    return JNI_VERSION_1_4;
}

bool load_native(char* path)
{
	void* handle = dlopen(path, RTLD_GLOBAL | RTLD_LAZY);
	if (!handle) 
	{
		LOGE("dlopen %s failed: %s", path, dlerror());
	}
	else 
	{
		LOGD("dlopen %s success", path);
	}
	return handle != NULL;
}

typedef void (*android_update_LD_LIBRARY_PATH_t)(char*);

void set_native_ld(char* path)
{
	android_update_LD_LIBRARY_PATH_t android_update_LD_LIBRARY_PATH;

	void* libdl_handle = dlopen("libdl.so", RTLD_LAZY);
	void* updateLdLibPath = dlsym(libdl_handle, "android_update_LD_LIBRARY_PATH");
	if (updateLdLibPath == NULL) 
	{
		updateLdLibPath = dlsym(libdl_handle, "__loader_android_update_LD_LIBRARY_PATH");
		if (updateLdLibPath == NULL) 
		{
			char* dl_error_c = dlerror();
			LOGE("Error getting symbol android_update_LD_LIBRARY_PATH: %s", dl_error_c);
		}
	}

	android_update_LD_LIBRARY_PATH = (android_update_LD_LIBRARY_PATH_t)updateLdLibPath;
	android_update_LD_LIBRARY_PATH(path);
}