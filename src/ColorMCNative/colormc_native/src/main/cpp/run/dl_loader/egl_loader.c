//
// Created by maks on 21.09.2022.
//
#include <stddef.h>
#include <stdlib.h>
#include <dlfcn.h>
#include <stdbool.h>
#include <android/hardware_buffer.h>
#include <GLES/egl.h>
#include <GLES/glext.h>
#include <EGL/eglext.h>

#include "egl_loader.h"

void* (*eglGetProcAddress_p)(const char* procname);
EGLBoolean (*eglMakeCurrent_p) (EGLDisplay dpy, EGLSurface draw, EGLSurface read, EGLContext ctx);
EGLBoolean (*eglDestroyContext_p) (EGLDisplay dpy, EGLContext ctx);
EGLBoolean (*eglDestroySurface_p) (EGLDisplay dpy, EGLSurface surface);
EGLBoolean (*eglTerminate_p) (EGLDisplay dpy);
EGLBoolean (*eglReleaseThread_p) (void);
EGLContext (*eglGetCurrentContext_p) (void);
EGLDisplay (*eglGetDisplay_p) (NativeDisplayType display);
EGLBoolean (*eglInitialize_p) (EGLDisplay dpy, EGLint *major, EGLint *minor);
EGLBoolean (*eglChooseConfig_p) (EGLDisplay dpy, const EGLint *attrib_list, EGLConfig *configs, EGLint config_size, EGLint *num_config);
EGLBoolean (*eglGetConfigAttrib_p) (EGLDisplay dpy, EGLConfig config, EGLint attribute, EGLint *value);
EGLBoolean (*eglBindAPI_p) (EGLenum api);
EGLSurface (*eglCreatePbufferSurface_p) (EGLDisplay dpy, EGLConfig config, const EGLint *attrib_list);
EGLSurface (*eglCreateWindowSurface_p) (EGLDisplay dpy, EGLConfig config, NativeWindowType window, const EGLint *attrib_list);
EGLBoolean (*eglSwapBuffers_p) (EGLDisplay dpy, EGLSurface draw);
EGLint (*eglGetError_p) (void);
EGLContext (*eglCreateContext_p) (EGLDisplay dpy, EGLConfig config, EGLContext share_list, const EGLint *attrib_list);
EGLBoolean (*eglSwapInterval_p) (EGLDisplay dpy, EGLint interval);
EGLSurface (*eglGetCurrentSurface_p) (EGLint readdraw);

EGLClientBuffer (*eglGetNativeClientBufferANDROID_p) (AHardwareBuffer *buffer);
void (*glEGLImageTargetTexture2DOES_p) (GLenum target, GLeglImageOES image);
EGLImageKHR (*eglCreateImageKHR_p)(EGLDisplay dpy, EGLContext ctx, EGLenum target,
        EGLClientBuffer buffer, const EGLint *attrib_list);
EGLBoolean (*eglDestroyImageKHR_p)(EGLDisplay dpy, EGLImageKHR image);

EGLSyncKHR (*eglCreateSyncKHR_p)(EGLDisplay dpy, EGLenum type, const EGLint *attrib_list);
EGLBoolean (*eglDestroySyncKHR_p)(EGLDisplay dpy, EGLSyncKHR sync);
EGLint (*eglWaitSyncKHR_p)(EGLDisplay dpy, EGLSyncKHR sync, EGLint flags);
EGLint (*eglDupNativeFenceFDANDROID_p)(EGLDisplay dpy, EGLSyncKHR sync);

bool egl_load() {
    char *name = getenv("EGL_SO");
    if (name == NULL) {
        return false;
    }
    void* egl_dl_handle = dlopen(name, RTLD_LAZY);
    eglBindAPI_p = dlsym(egl_dl_handle, "eglBindAPI");
    eglChooseConfig_p = dlsym(egl_dl_handle, "eglChooseConfig");
    eglCreateContext_p = dlsym(egl_dl_handle, "eglCreateContext");
    eglCreatePbufferSurface_p = dlsym(egl_dl_handle, "eglCreatePbufferSurface");
    eglCreateWindowSurface_p = dlsym(egl_dl_handle, "eglCreateWindowSurface");
    eglDestroyContext_p = dlsym(egl_dl_handle, "eglDestroyContext");
    eglDestroySurface_p = dlsym(egl_dl_handle, "eglDestroySurface");
    eglGetConfigAttrib_p = dlsym(egl_dl_handle, "eglGetConfigAttrib");
    eglGetCurrentContext_p = dlsym(egl_dl_handle, "eglGetCurrentContext");
    eglGetDisplay_p = dlsym(egl_dl_handle, "eglGetDisplay");
    eglGetError_p = dlsym(egl_dl_handle, "eglGetError");
    eglInitialize_p = dlsym(egl_dl_handle, "eglInitialize");
    eglMakeCurrent_p = dlsym(egl_dl_handle, "eglMakeCurrent");
    eglSwapBuffers_p = dlsym(egl_dl_handle, "eglSwapBuffers");
    eglReleaseThread_p = dlsym(egl_dl_handle, "eglReleaseThread");
    eglSwapInterval_p = dlsym(egl_dl_handle, "eglSwapInterval");
    eglTerminate_p = dlsym(egl_dl_handle, "eglTerminate");
    eglGetCurrentSurface_p = dlsym(egl_dl_handle, "eglGetCurrentSurface");
    eglGetProcAddress_p = dlsym(egl_dl_handle, "eglGetProcAddress");

    eglGetNativeClientBufferANDROID_p =
            eglGetProcAddress_p("eglGetNativeClientBufferANDROID");
    glEGLImageTargetTexture2DOES_p =
            eglGetProcAddress_p("glEGLImageTargetTexture2DOES");
    eglCreateImageKHR_p = eglGetProcAddress_p("eglCreateImageKHR");
    eglDestroyImageKHR_p = eglGetProcAddress_p("eglDestroyImageKHR");

    eglCreateSyncKHR_p = eglGetProcAddress_p("eglCreateSyncKHR");
    eglDestroySyncKHR_p = eglGetProcAddress_p("eglDestroySyncKHR");
    eglWaitSyncKHR_p = eglGetProcAddress_p("eglWaitSyncKHR");
    eglDupNativeFenceFDANDROID_p =
            eglGetProcAddress_p("eglDupNativeFenceFDANDROID");

    return true;
}