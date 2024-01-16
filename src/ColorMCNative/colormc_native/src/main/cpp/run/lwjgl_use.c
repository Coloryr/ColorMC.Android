//
// Created by 40206 on 2024/1/11.
//

#include <stdio.h>
#include <unistd.h>
#include <dlfcn.h>
#include <stdlib.h>
#include <libgen.h>
#include <string.h>
#include <jni.h>

#include "GL/gl.h"

#include "game_sock.h"
#include "dl_loader/egl_loader.h"
#include "dl_loader/gl_loader.h"
#include "dl_loader/ah_loader.h"
#include "run.h"
#include "events.h"
#include "render_sock.h"

void* showingWindow;

JavaVM* vm_env;
JNIEnv* jre_env;

jclass vmGlfwClass;

jmethodID method_glftSetWindowAttrib;
jmethodID method_internalWindowSizeChanged;

jbyte* keyDownBuffer;
jbyte* mouseDownBuffer;

#define EXTERNAL_API __attribute__((used))

EXTERNAL_API int colormcInit() {
    event_init();
    return egl_init();
}

EXTERNAL_API void* colormcCreateContext(void* context) {
    showingWindow = egl_create_context(context);
    return showingWindow;
}

EXTERNAL_API void* colormcGetCurrentContext() {
    return egl_get_context();
}

EXTERNAL_API void colormcMakeCurrent(void* window) {
    egl_make_current(window);
}

EXTERNAL_API void colormcDestroyContext(void* window) {
    egl_destroy_context(window);
}

#define GLFW_CLIENT_API 0x22001
/* Consider GLFW_NO_API as Vulkan API */
#define GLFW_NO_API 0
#define GLFW_OPENGL_API 0x30001

EXTERNAL_API void colormcSetWindowHint(int hint, int value) {
//    if (hint != GLFW_CLIENT_API) return;
//    switch (value) {
//        case GLFW_NO_API:
//
//            break;
//        case GLFW_OPENGL_API:
//
//            break;
//        default:
//            printf("[ColorMC Error] GLFW unimplemented window hint 0x%x\n", value);
//            fflush(stdout);
//    }
}

EXTERNAL_API void colormcSwapBuffers() {
    egl_swap_buffers();
}

EXTERNAL_API void colormcSwapInterval(int interval) {
    egl_swap_interval(interval);
}

EXTERNAL_API void colormcPumpEvents(void* window) {
    start_event(window);
}

EXTERNAL_API void colormcRewindEvents() {
    get_event();
}

EXTERNAL_API void colormcComputeEventTarget() {
    compute_event();
}

EXTERNAL_API void colormcTerminate() {
    egl_close();
}

//JNI list

jint (*orig_ProcessImpl_forkAndExec)(JNIEnv *env, jobject process, jint mode, jbyteArray helperpath, jbyteArray prog, jbyteArray argBlock, jint argc, jbyteArray envBlock, jint envc, jbyteArray dir, jintArray std_fds, jboolean redirectErrorStream);

jint
hooked_ProcessImpl_forkAndExec(JNIEnv *env, jobject process, jint mode, jbyteArray helperpath, jbyteArray prog, jbyteArray argBlock, jint argc, jbyteArray envBlock, jint envc, jbyteArray dir, jintArray std_fds, jboolean redirectErrorStream) {
    char *pProg = (char *)((*env)->GetByteArrayElements(env, prog, NULL));

    // Here we only handle the "xdg-open" command
    if (strcmp(basename(pProg), "xdg-open") != 0) {
        (*env)->ReleaseByteArrayElements(env, prog, (jbyte *)pProg, 0);
        return orig_ProcessImpl_forkAndExec(env, process, mode, helperpath, prog, argBlock, argc, envBlock, envc, dir, std_fds, redirectErrorStream);
    }
    (*env)->ReleaseByteArrayElements(env, prog, (jbyte *)pProg, 0);

    //TODO CLIPBOARD_OPEN
    //Java_org_lwjgl_glfw_CallbackBridge_nativeClipboard(env, NULL, /* CLIPBOARD_OPEN */ 2002, argBlock);
    return 0;
}

void hookExec() {
    jclass cls;
    orig_ProcessImpl_forkAndExec = dlsym(RTLD_DEFAULT, "Java_java_lang_UNIXProcess_forkAndExec");
    if (!orig_ProcessImpl_forkAndExec) {
        orig_ProcessImpl_forkAndExec = dlsym(RTLD_DEFAULT, "Java_java_lang_ProcessImpl_forkAndExec");
        cls = (*jre_env)->FindClass(jre_env, "java/lang/ProcessImpl");
    } else {
        cls = (*jre_env)->FindClass(jre_env, "java/lang/UNIXProcess");
    }
    JNINativeMethod methods[] = {
            {"forkAndExec", "(I[B[B[BI[BI[B[IZ)I", (void *)&hooked_ProcessImpl_forkAndExec}
    };
    (*jre_env)->RegisterNatives(jre_env, cls, methods, 1);
    printf("[ColorMC Info] Registered forkAndExec\n");
    fflush(stdout);
}

/**
 * Basically a verbatim implementation of ndlopen(), found at
 * https://github.com/PojavLauncherTeam/lwjgl3/blob/3.3.1/modules/lwjgl/core/src/generated/c/linux/org_lwjgl_system_linux_DynamicLinkLoader.c#L11
 * The idea is that since, on Android 10 and earlier, the linker doesn't really do namespace nesting.
 * It is not a problem as most of the libraries are in the launcher path, but when you try to run
 * VulkanMod which loads shaderc outside of the default jni libs directory through this method,
 * it can't load it because the path is not in the allowed paths for the anonymous namesapce.
 * This method fixes the issue by being in libpojavexec, and thus being in the classloader namespace
 */
jlong ndlopen_bugfix(JNIEnv *env,jclass class,jlong filename_ptr,jint jmode) {
    const char *filename = (const char *) filename_ptr;
    int mode = (int) jmode;
    return (jlong) dlopen(filename, mode);
}

/**
 * Install the linker bug mitigation for Android 10 and lower. Fixes VulkanMod crashing on these
 * Android versions due to missing namespace nesting.
 */
void installLinkerBugMitigation() {
    char *version = getenv("ANDROID_VERSION");
    if (version == NULL) {
        return;
    }
    int num = atoi(version);
    if (num >= 30) return;
    printf("[ColorMC Info] API < 30 detected, installing linker bug mitigation\n");
    JNIEnv *env = jre_env;
    jclass dynamicLinkLoader = (*env)->FindClass(env, "org/lwjgl/system/linux/DynamicLinkLoader");
    if (dynamicLinkLoader == NULL) {
        printf("[ColorMC Error] failed to find the DynamicLinkLoader class\n");
        (*env)->ExceptionClear(env);
    } else {
        JNINativeMethod ndlopenMethod[] = {
                {"ndlopen", "(JI)J", &ndlopen_bugfix}
        };
        if ((*env)->RegisterNatives(env, dynamicLinkLoader, ndlopenMethod, 1) != 0) {
            printf("[ColorMC Error] failed to register the bugfix method\n");
            (*env)->ExceptionClear(env);
        } else {
            printf("[ColorMC Info] register the bugfix method done.\n");
        }
    }

    fflush(stdout);
}

jint JNI_OnLoad(JavaVM* vm, void* reserved) {
    printf("[ColorMC Info] Saving JVM environ...\n");
    fflush(stdout);
    vm_env = vm;
    (*vm)->GetEnv(vm, (void **) &jre_env, JNI_VERSION_1_6);
    vmGlfwClass = (*jre_env)->NewGlobalRef(
            jre_env, (*jre_env)->FindClass(jre_env, "org/lwjgl/glfw/GLFW"));
    method_glftSetWindowAttrib = (*jre_env)->GetStaticMethodID(
            jre_env, vmGlfwClass, "glfwSetWindowAttrib", "(JII)V");
    method_internalWindowSizeChanged = (*jre_env)->GetStaticMethodID(
            jre_env, vmGlfwClass, "internalWindowSizeChanged", "(JII)V");
    jfieldID field_keyDownBuffer = (*jre_env)->GetStaticFieldID(
            jre_env, vmGlfwClass, "keyDownBuffer", "Ljava/nio/ByteBuffer;");
    jobject keyDownBufferJ = (*jre_env)->GetStaticObjectField(
            jre_env, vmGlfwClass, field_keyDownBuffer);
    keyDownBuffer = (*jre_env)->GetDirectBufferAddress(
            jre_env, keyDownBufferJ);
    jfieldID field_mouseDownBuffer = (*jre_env)->GetStaticFieldID(
            jre_env, vmGlfwClass, "mouseDownBuffer", "Ljava/nio/ByteBuffer;");
    jobject mouseDownBufferJ = (*jre_env)->GetStaticObjectField(
            jre_env, vmGlfwClass, field_mouseDownBuffer);
    mouseDownBuffer = (*jre_env)->GetDirectBufferAddress(
            jre_env, mouseDownBufferJ);
    //hookExec();
    installLinkerBugMitigation();
    isGrabbing = JNI_FALSE;

    return JNI_VERSION_1_6;
}

JNIEXPORT jlong JNICALL Java_org_lwjgl_glfw_GLFW_nglfwSetFramebufferSizeCallback(JNIEnv * env, jclass cls, jlong window, jlong callbackptr) {
    void **oldCallback = (void **) &GLFW_invoke_FramebufferSize;
    GLFW_invoke_FramebufferSize = (GLFW_invoke_FramebufferSize_func *) (uintptr_t) callbackptr;
    return (jlong) (uintptr_t) *oldCallback;
}

JNIEXPORT jlong JNICALL Java_org_lwjgl_glfw_GLFW_nglfwSetWindowSizeCallback(JNIEnv * env, jclass cls, jlong window, jlong callbackptr) {
    void **oldCallback = (void **) &GLFW_invoke_WindowSize;
    GLFW_invoke_WindowSize = (GLFW_invoke_WindowSize_func *) (uintptr_t) callbackptr;
    return (jlong) (uintptr_t) *oldCallback;
}

#define ADD_CALLBACK_WWIN(NAME) \
JNIEXPORT jlong JNICALL Java_org_lwjgl_glfw_GLFW_nglfwSet##NAME##Callback(JNIEnv * env, jclass cls, jlong window, jlong callbackptr) { \
    void** oldCallback = (void**) &GLFW_invoke_##NAME; \
    GLFW_invoke_##NAME = (GLFW_invoke_##NAME##_func*) (uintptr_t) callbackptr; \
    return (jlong) (uintptr_t) *oldCallback; \
}

ADD_CALLBACK_WWIN(Char)
ADD_CALLBACK_WWIN(CharMods)
ADD_CALLBACK_WWIN(CursorEnter)
ADD_CALLBACK_WWIN(CursorPos)
ADD_CALLBACK_WWIN(Key)
ADD_CALLBACK_WWIN(MouseButton)
ADD_CALLBACK_WWIN(Scroll)

#undef ADD_CALLBACK_WWIN

JNIEXPORT void JNICALL
Java_org_lwjgl_glfw_GLFW_nglfwGetCursorPos(JNIEnv *env, jclass clazz, __attribute__((unused)) jlong window, jobject xpos, jobject ypos) {
    *(double*)(*env)->GetDirectBufferAddress(env, xpos) = cursorX;
    *(double*)(*env)->GetDirectBufferAddress(env, ypos) = cursorY;
}

JNIEXPORT void JNICALL JavaCritical_org_lwjgl_glfw_GLFW_nglfwGetCursorPosA(__attribute__((unused)) jlong window, jint lengthx, jdouble* xpos, jint lengthy, jdouble* ypos) {
    *xpos = cursorX;
    *ypos = cursorY;
}

JNIEXPORT void JNICALL
Java_org_lwjgl_glfw_GLFW_nglfwGetCursorPosA(JNIEnv *env, jclass clazz, __attribute__((unused)) jlong window, jdoubleArray xpos, jdoubleArray ypos) {
    (*env)->SetDoubleArrayRegion(env, xpos, 0,1, &cursorX);
    (*env)->SetDoubleArrayRegion(env, ypos, 0,1, &cursorY);
}

JNIEXPORT void JNICALL JavaCritical_org_lwjgl_glfw_GLFW_glfwSetCursorPos(jlong window, jdouble xpos, jdouble ypos) {
    cLastX = cursorX = xpos;
    cLastY = cursorY = ypos;

}

JNIEXPORT void JNICALL
Java_org_lwjgl_glfw_GLFW_glfwSetCursorPos(__attribute__((unused)) JNIEnv *env, __attribute__((unused)) jclass clazz, __attribute__((unused)) jlong window, jdouble xpos,
                                          jdouble ypos) {
    JavaCritical_org_lwjgl_glfw_GLFW_glfwSetCursorPos(window, xpos, ypos);
}

JNIEXPORT void JNICALL Java_org_lwjgl_glfw_GLFW_nglfwSetShowingWindow(__attribute__((unused)) JNIEnv* env, __attribute__((unused)) jclass clazz, jlong window) {
    showingWindow = (void*) window;
    printf("[ColorMC Info] window show: %p\n", showingWindow);
    fflush(stdout);
}

JNIEXPORT jstring JNICALL Java_org_lwjgl_glfw_CallbackBridge_nativeClipboard(JNIEnv* env, __attribute__((unused)) jclass clazz, jint action, jbyteArray copySrc) {
    printf("[ColorMC Info] Clipboard: Converting string\n");
    char *copySrcC;
    if (copySrc) {
        copySrcC = (char *)((*env)->GetByteArrayElements(env, copySrc, NULL));
    }

    printf("[ColorMC Info] Clipboard: Calling 2nd\n");

    jstring dstStr = (*env)->NewStringUTF(env, copySrcC);

    //TODO to Clipboard
//    jstring pasteDst = convertStringJVM(dalvikEnv, env, (jstring) (*dalvikEnv)->CallStaticObjectMethod(dalvikEnv, bridgeClazz, method_accessAndroidClipboard, action, copyDst));
//
//    if (copySrc) {
//        (*dalvikEnv)->DeleteLocalRef(dalvikEnv, copyDst);
//        (*env)->ReleaseByteArrayElements(env, copySrc, (jbyte *)copySrcC, 0);
//    }
//    (*dalvikJavaVMPtr)->DetachCurrentThread(dalvikJavaVMPtr);

    return dstStr;
}

JNIEXPORT jboolean JNICALL JavaCritical_org_lwjgl_glfw_CallbackBridge_nativeSetInputReady(jboolean inputReady) {
    printf("[ColorMC Info] Input ready: %i\n", inputReady);
    fflush(stdout);
    isInputReady = inputReady;
    return isUseStackQueueCall;
}

JNIEXPORT jboolean JNICALL Java_org_lwjgl_glfw_CallbackBridge_nativeSetInputReady(JNIEnv* env, jclass clazz, jboolean inputReady) {
    return JavaCritical_org_lwjgl_glfw_CallbackBridge_nativeSetInputReady(inputReady);
}

JNIEXPORT void JNICALL Java_org_lwjgl_glfw_CallbackBridge_nativeSetGrabbing(JNIEnv* env, jclass clazz, jboolean grabbing) {
    send_grabbing(grabbing);
    isGrabbing = grabbing;
}

JNIEXPORT void JNICALL Java_org_lwjgl_glfw_CallbackBridge_nativeSetWindowAttrib(JNIEnv* env, jclass clazz, jint attrib, jint value) {
    if (!showingWindow || !isUseStackQueueCall) {
        // If the window is not shown, there is nothing to do yet.
        // For Minecraft < 1.13, calling to JNI functions here crashes the JVM for some reason, therefore it is skipped for now.
        return;
    }

    (*jre_env)->CallStaticVoidMethod(
            jre_env,
            vmGlfwClass, method_glftSetWindowAttrib,
            (jlong) showingWindow, attrib, value
    );
}