#include "jni.h"
#include "log.h"
#include "abi_info.h"

static JavaVM* jvm;
static JNIEnv* env;

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