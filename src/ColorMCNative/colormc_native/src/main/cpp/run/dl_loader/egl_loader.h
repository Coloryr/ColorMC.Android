//
// Created by maks on 21.09.2022.
//

#ifndef POJAVLAUNCHER_EGL_LOADER_H
#define POJAVLAUNCHER_EGL_LOADER_H

#include <stdbool.h>
#include <android/hardware_buffer.h>
#include <GLES/egl.h>
#include <EGL/egl.h>
#include <EGL/eglext.h>

extern EGLBoolean (*eglMakeCurrent_p) (EGLDisplay dpy, EGLSurface draw, EGLSurface read, EGLContext ctx);
extern EGLBoolean (*eglDestroyContext_p) (EGLDisplay dpy, EGLContext ctx);
extern EGLBoolean (*eglDestroySurface_p) (EGLDisplay dpy, EGLSurface surface);
extern EGLBoolean (*eglTerminate_p) (EGLDisplay dpy);
extern EGLBoolean (*eglReleaseThread_p) (void);
extern EGLContext (*eglGetCurrentContext_p) (void);
extern EGLDisplay (*eglGetDisplay_p) (NativeDisplayType display);
extern EGLBoolean (*eglInitialize_p) (EGLDisplay dpy, EGLint *major, EGLint *minor);
extern EGLBoolean (*eglChooseConfig_p) (EGLDisplay dpy, const EGLint *attrib_list, EGLConfig *configs, EGLint config_size, EGLint *num_config);
extern EGLBoolean (*eglGetConfigAttrib_p) (EGLDisplay dpy, EGLConfig config, EGLint attribute, EGLint *value);
extern EGLBoolean (*eglBindAPI_p) (EGLenum api);
extern EGLSurface (*eglCreatePbufferSurface_p) (EGLDisplay dpy, EGLConfig config, const EGLint *attrib_list);
extern EGLSurface (*eglCreateWindowSurface_p) (EGLDisplay dpy, EGLConfig config, NativeWindowType window, const EGLint *attrib_list);
extern EGLBoolean (*eglSwapBuffers_p) (EGLDisplay dpy, EGLSurface draw);
extern EGLint (*eglGetError_p) (void);
extern EGLContext (*eglCreateContext_p) (EGLDisplay dpy, EGLConfig config, EGLContext share_list, const EGLint *attrib_list);
extern EGLBoolean (*eglSwapInterval_p) (EGLDisplay dpy, EGLint interval);
extern EGLSurface (*eglGetCurrentSurface_p) (EGLint readdraw);
extern void* (*eglGetProcAddress_p)(const char* procname);

extern EGLClientBuffer (*eglGetNativeClientBufferANDROID_p) (AHardwareBuffer *buffer);
extern void (*glEGLImageTargetTexture2DOES_p) (GLenum target, GLeglImageOES image);
extern EGLImageKHR (*eglCreateImageKHR_p)(EGLDisplay dpy, EGLContext ctx, EGLenum target,
                                   EGLClientBuffer buffer, const EGLint *attrib_list);
extern EGLBoolean (*eglDestroyImageKHR_p)(EGLDisplay dpy, EGLImageKHR image);

extern EGLSyncKHR (*eglCreateSyncKHR_p)(EGLDisplay dpy, EGLenum type, const EGLint *attrib_list);
extern EGLBoolean (*eglDestroySyncKHR_p)(EGLDisplay dpy, EGLSyncKHR sync);
extern EGLint (*eglWaitSyncKHR_p)(EGLDisplay dpy, EGLSyncKHR sync, EGLint flags);
extern EGLint (*eglDupNativeFenceFDANDROID_p)(EGLDisplay dpy, EGLSyncKHR sync);

bool egl_load();

#endif //POJAVLAUNCHER_EGL_LOADER_H
