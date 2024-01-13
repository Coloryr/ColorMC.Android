//#include <dlfcn.h>
//#include <unistd.h>
//#include <android/rect.h>
//#include <android/log.h>
//#include <android/hardware_buffer.h>
//#include <jni.h>
//#include <GLES/egl.h>
//#include <GLES/glext.h>
//#include <EGL/eglext.h>
//#include <stdio.h>
//
//#include "run_shared_texture.h"
//#include "dl_loader/egl_loader.h"
//#include "dl_loader/ah_loader.h"
//#include "dl_loader/egl_loader.h"
//
//extern EGLDisplay display;
//
//extern void (*glFlush_p)();
//
//const char *getEGLError();
//
//int createEGLFence() {
//    EGLSyncKHR eglSync = eglCreateSyncKHR_p(display, EGL_SYNC_NATIVE_FENCE_ANDROID, NULL);
//    if (eglSync == EGL_NO_SYNC_KHR) {
//        printf("[ColorMC Error] createEGLFence null: eglCreateSyncKHR null: %s\n", getEGLError());
//        fflush(stdout);
//        return EGL_NO_NATIVE_FENCE_FD_ANDROID;
//    }
//
//    // need flush before wait
//    glFlush_p();
//
//    int fenceFd = eglDupNativeFenceFDANDROID_p(display, eglSync);
//    eglDestroySyncKHR_p(display, eglSync);
//
//    if (fenceFd == EGL_NO_NATIVE_FENCE_FD_ANDROID) {
//        printf("[ColorMC Error] createEGLFence null: eglDupNativeFenceFDANDROID error: %s\n", getEGLError());
//        fflush(stdout);
//        fflush(stdout);
//    }
//
//    return fenceFd;
//}
//
//bool waitEGLFence(int fenceFd) {
//    EGLint attribs[] = {EGL_SYNC_NATIVE_FENCE_FD_ANDROID, fenceFd, EGL_NONE};
//    EGLSyncKHR eglSync = eglCreateSyncKHR_p(display, EGL_SYNC_NATIVE_FENCE_ANDROID, attribs);
//    if (eglSync == EGL_NO_SYNC_KHR) {
//        printf("[ColorMC Error] waitEGLFence failed: eglCreateSyncKHR null: %s\n", getEGLError());
//        fflush(stdout);
//        close(fenceFd);
//        return false;
//    }
//
//    EGLint success = eglWaitSyncKHR_p(display, eglSync, 0);
//    eglDestroySyncKHR_p(display, eglSync);
//
//    if (success == EGL_FALSE) {
//        printf("[ColorMC Error] waitEGLFence failed: eglWaitSyncKHR fail: %s\n", getEGLError());
//        fflush(stdout);
//        return false;
//    }
//
//    return true;
//}