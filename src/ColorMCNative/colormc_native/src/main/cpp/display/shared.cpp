#include <jni.h>

#include "shared_texture.h"

extern "C" bool available() {
    return SharedTexture::available();
}

extern "C" void *createFromBuffer(AHardwareBuffer *buffer) {
    return SharedTexture::MakeAdopted(buffer);
}

extern "C" bool bindSharedTexture(void *ctx, int texId) {
    return ((SharedTexture *) ctx)->bindTexture(texId);
}

extern "C" int createEGLFence() {
    return SharedTexture::createEGLFence();
}

extern "C" bool waitEGLFence(int fenceFd) {
    return SharedTexture::waitEGLFence(fenceFd);
}

extern "C" void destroy(void *ctx) {
    delete (SharedTexture *) ctx;
}
