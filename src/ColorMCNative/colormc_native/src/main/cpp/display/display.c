#include <jni.h>
#include <android/hardware_buffer.h>
#include <android/hardware_buffer_jni.h>
#include <android/native_window.h>
#include <android/native_window_jni.h>
#include <unistd.h>
#include <sys/socket.h>
#include <linux/un.h>
#include <malloc.h>
#include <string.h>
#include <android/log.h>
#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <stddef.h>
#include <stdlib.h>
#include <dlfcn.h>

#include <GLES2/gl2.h>

#define LOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, "ColorMC_Android_Display", __VA_ARGS__))
#define LOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, "ColorMC_Android_Display", __VA_ARGS__))

#define EXTERNAL_API __attribute__((used))

extern void *createFromBuffer(AHardwareBuffer *buffer);
extern bool bindSharedTexture(void *ctx, int texId);
extern void destroy(void *ctx);

EXTERNAL_API void* getBuffer(char *name) {
    struct sockaddr_un addr;
    AHardwareBuffer *buffer = NULL;
    int sockfd;

    LOGI("get buffer from %s", name);

    // 创建socket
    sockfd = socket(AF_UNIX, SOCK_STREAM, 0);
    if (sockfd == -1) {
        LOGW("socket create fail");
        return false;
    }

    // 设置socket地址
    memset(&addr, 0, sizeof(struct sockaddr_un));
    addr.sun_family = AF_UNIX;
    strncpy(addr.sun_path, name, sizeof(addr.sun_path) - 1);

    // 连接到服务器
    if (connect(sockfd, (struct sockaddr *) &addr, sizeof(struct sockaddr_un)) == -1) {
        LOGW("connect fail");
        close(sockfd);
        return false;
    }

    if (AHardwareBuffer_recvHandleFromUnixSocket(sockfd, &buffer) != 0) {
        LOGW("buffer read fail");
        buffer = NULL;
    } else {
        LOGI("buffer is read");
    }

    close(sockfd);

    return buffer;
}

EXTERNAL_API bool bindTexture(unsigned int texId, void* buffer,
                              int* width_out, int* height_out, void** texture) {
    if (buffer == NULL) {
        LOGW("buffer is null");
        return false;
    }
    AHardwareBuffer_Desc desc;
    AHardwareBuffer_describe(buffer, &desc);

    LOGI("desc layers: %d", desc.layers);
    LOGI("desc format: %d", desc.format);
    LOGI("desc width: %d", desc.width);
    LOGI("desc height: %d", desc.height);

    if (width_out != NULL) {
        *width_out = desc.width;
    }
    if (height_out != NULL) {
        *height_out = desc.height;
    }

    *texture = createFromBuffer(buffer);
    return bindSharedTexture(*texture, texId);
}

EXTERNAL_API bool deleteBuffer(void* buffer, void* texture) {
    if (buffer == NULL || texture == NULL) {
        return false;
    }
    destroy(texture);
    return true;
}