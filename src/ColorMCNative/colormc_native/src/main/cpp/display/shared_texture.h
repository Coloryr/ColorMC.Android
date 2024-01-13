#pragma once

#include <string>
#include <memory>

#include <jni.h>
#include <android/hardware_buffer.h>
#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <GLES/gl.h>
#include <GLES/glext.h>

class SharedTexture {
public:
    static bool available();

    static SharedTexture *MakeAdopted(AHardwareBuffer *buffer);

    virtual ~SharedTexture();

    bool bindTexture(unsigned int texId);

    AHardwareBuffer *getBuffer() const;

    static int createEGLFence();

    static bool waitEGLFence(int fenceFd);

private:
    SharedTexture(AHardwareBuffer *buffer, int width, int height);

private:
    static bool AVAILABLE;

    AHardwareBuffer *buffer_ = nullptr;
    EGLImageKHR eglImage_ = EGL_NO_IMAGE_KHR;
    int width_ = 0;
    int height_ = 0;
    GLuint bindTexId_ = 0;
};