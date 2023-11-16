#include "SharedObject1.h"
#include <android/native_window.h>
#include <android/native_window_jni.h>
#include <stdatomic.h>

#define LOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, "SharedObject1", __VA_ARGS__))
#define LOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, "SharedObject1", __VA_ARGS__))

#include <EGL/egl.h>

extern "C" {
	/*此简单函数返回平台 ABI，此动态本地库为此平台 ABI 进行编译。*/
	const char * SharedObject1::getPlatformABI()
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

	void release()
	{
		glGetString
	}
}
