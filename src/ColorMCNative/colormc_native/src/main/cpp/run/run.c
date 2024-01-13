#include <stdio.h>
#include <sys/socket.h>
#include <sys/un.h>
#include <unistd.h>
#include <setjmp.h>
#include <dlfcn.h>
#include <stdlib.h>
#include <string.h>
#include <EGL//egl.h>
#include <stdbool.h>
#include <pthread.h>
#include <android/hardware_buffer.h>

#include "GL/gl.h"

#include "render_sock.h"
#include "game_sock.h"
#include "render_test.h"
#include "run_shared_texture.h"

#include "dl_loader/egl_loader.h"
#include "dl_loader/gl_loader.h"
#include "dl_loader/ah_loader.h"
#include "events.h"

#define EXTERNAL_API __attribute__((used))

EGLDisplay display = EGL_NO_DISPLAY;
EGLSurface surface = EGL_NO_SURFACE;
EGLContext context = EGL_NO_CONTEXT;
EGLConfig config;
EGLint format;

GLuint texture;

int width = 640;
int height = 480;

GLuint fbo;

bool can_run = false;

enum RENDER_STATE {
    RENDER_RUN,
    RENDER_CHANGE_SIZE
};

uint8_t render_state;

AHardwareBuffer *a_buffer = NULL;
EGLImageKHR eglImage = EGL_NO_IMAGE_KHR;

const char *getEGLError() {
    switch (eglGetError_p()) {
        case EGL_SUCCESS:
            return "EGL_SUCCESS";
        case EGL_NOT_INITIALIZED:
            return "EGL_NOT_INITIALIZED";
        case EGL_BAD_ACCESS:
            return "EGL_BAD_ACCESS";
        case EGL_BAD_ALLOC:
            return "EGL_BAD_ALLOC";
        case EGL_BAD_ATTRIBUTE:
            return "EGL_BAD_ATTRIBUTE";
        case EGL_BAD_CONTEXT:
            return "EGL_BAD_CONTEXT";
        case EGL_BAD_CONFIG:
            return "EGL_BAD_CONFIG";
        case EGL_BAD_CURRENT_SURFACE:
            return "EGL_BAD_CURRENT_SURFACE";
        case EGL_BAD_DISPLAY:
            return "EGL_BAD_DISPLAY";
        case EGL_BAD_SURFACE:
            return "EGL_BAD_SURFACE";
        case EGL_BAD_MATCH:
            return "EGL_BAD_MATCH";
        case EGL_BAD_PARAMETER:
            return "EGL_BAD_PARAMETER";
        case EGL_BAD_NATIVE_PIXMAP:
            return "EGL_BAD_NATIVE_PIXMAP";
        case EGL_BAD_NATIVE_WINDOW:
            return "EGL_BAD_NATIVE_WINDOW";
        case EGL_CONTEXT_LOST:
            return "EGL_CONTEXT_LOST";
        default:
            return "Unknown error";
    }
}

void make_buffer() {
    AHardwareBuffer_Desc desc = {
            width,
            height,
            1,
            AHARDWAREBUFFER_FORMAT_R8G8B8A8_UNORM,
            AHARDWAREBUFFER_USAGE_CPU_READ_NEVER
            | AHARDWAREBUFFER_USAGE_CPU_WRITE_NEVER
            | AHARDWAREBUFFER_USAGE_GPU_SAMPLED_IMAGE
            | AHARDWAREBUFFER_USAGE_GPU_COLOR_OUTPUT,
            0,
            0,
            0};
    int errCode = AHardwareBuffer_allocate_p(&desc, &a_buffer);
    if (errCode != 0 || !a_buffer) {
        printf("[ColorMC Error] Make AHardwarea allocate failed error: %d\n", errCode);
    } else {
        printf("[ColorMC Info] create AHardwareBuffer size %d x %d\n", width, height);
    }

    fflush(stdout);
}

void egl_swap_interval(int swapInterval) {
    eglSwapInterval_p(display, swapInterval);
}

bool egl_create() {
    printf("[ColorMC Info] egl create\n");
    display = eglGetDisplay_p(EGL_DEFAULT_DISPLAY);
    if (display == EGL_NO_DISPLAY) {
        printf("[ColorMC Error] eglGetDisplay_p() returned EGL_NO_DISPLAY: %s\n", getEGLError());
        fflush(stdout);
        return false;
    }
    if (eglInitialize_p(display, NULL, NULL) != EGL_TRUE) {
        printf("[ColorMC Error] eglInitialize_p() failed: %s\n", getEGLError());
        fflush(stdout);
        return false;
    }
    return true;
}

void* egl_create_context(void * share) {
    printf("[ColorMC Info] egl_create_context input: %p\n", share);

    // 配置EGL属性
    EGLint configAttribs[] = {
            EGL_BLUE_SIZE, 8,
            EGL_GREEN_SIZE, 8,
            EGL_RED_SIZE, 8,
            EGL_ALPHA_SIZE, 8,
            EGL_DEPTH_SIZE, 24,
            EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
            EGL_NONE, 0,
            EGL_NONE
    };

    // 选择EGL配置
    EGLint numConfigs;
    if (eglChooseConfig_p(display, configAttribs,
                          &config, 1, &numConfigs) != EGL_TRUE) {
        printf("[ColorMC Error] eglChooseConfig_p() failed: %s\n", getEGLError());
        fflush(stdout);
        return NULL;
    }

    if (numConfigs == 0) {
        printf("[ColorMC Error] eglChooseConfig_p() found no matching config\n");
        fflush(stdout);
        return NULL;
    }

    EGLBoolean bindResult;
    char *temp = getenv("GAME_RENDERER");
    if (temp != NULL && strcmp(temp, "1") == 0) {
        printf("[ColorMC Info] EGL Binding to desktop OpenGL\n");
        bindResult = eglBindAPI_p(EGL_OPENGL_API);
    } else {
        printf("[ColorMC Info] EGL Binding to OpenGL ES\n");
        bindResult = eglBindAPI_p(EGL_OPENGL_ES_API);
    }
    if (!bindResult) {
        printf("[ColorMC Error] EGL bind failed: %s\n", getEGLError());
    }
    fflush(stdout);

    int gles_version = 3;
    char* temp1 = getenv("GL_ES_VERSION");
    if(temp1 != NULL) {
        gles_version = strtol(temp1, NULL, 0);
        if (gles_version < 0 || gles_version > INT16_MAX) gles_version = 2;
    }
    // 创建EGL上下文
    EGLint contextAttribs[] = {
            EGL_CONTEXT_CLIENT_VERSION, gles_version,
            EGL_NONE
    };
    context = eglCreateContext_p(display, config, NULL, contextAttribs);

    if (context == EGL_NO_CONTEXT) {
        printf("[ColorMC Error] eglCreateContext_p() finished with error: %s", getEGLError());
        fflush(stdout);
        return NULL;
    }

    printf("[ColorMC Info] egl_create_context output: %p\n", context);
    fflush(stdout);

    return context;
}

void* egl_get_context() {
    printf("[ColorMC Info] egl_get_context output: %p\n", context);
    fflush(stdout);
    return context;
}

void egl_create_surface() {
    printf("[ColorMC Info] egl create surface\n");
    // 创建Pbuffer表面
    EGLint pbufferAttribs[] = {
            EGL_WIDTH, width,
            EGL_HEIGHT, height,
            EGL_NONE
    };
    surface = eglCreatePbufferSurface_p(display, config, pbufferAttribs);
    if (surface == NULL) {
        printf("[ColorMC Error] createPbufferSurface failed: %s\n", getEGLError());
    }
    printf("[ColorMC Info] egl surface size %d x %d\n", width, height);
    fflush(stdout);
}

void gl_create() {
    printf("[ColorMC Info] gl create\n");

    glGenTextures_p(1, &texture);
    glBindTexture_p(GL_TEXTURE_2D, texture);

    glTexParameteri_p(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri_p(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri_p(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri_p(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);

    EGLClientBuffer clientBuffer = eglGetNativeClientBufferANDROID_p(a_buffer);
    if (!clientBuffer) {
        printf("[ColorMC Error] bindTexture failed: clientBuffer null\n");
        fflush(stdout);
        return;
    }

    EGLint eglImageAttributes[] =
            {
                    EGL_IMAGE_PRESERVED_KHR, EGL_TRUE,
                    EGL_NONE
            };
    eglImage = eglCreateImageKHR_p(display, EGL_NO_CONTEXT,
                                   EGL_NATIVE_BUFFER_ANDROID,
                                   clientBuffer, eglImageAttributes);
    if (eglImage == EGL_NO_IMAGE_KHR) {
        printf("[ColorMC Error] bindTexture failed: eglCreateImageKHR null: %s\n", getEGLError());
        fflush(stdout);
        return;
    }

    glEGLImageTargetTexture2DOES_p(GL_TEXTURE_2D, (GLeglImageOES) eglImage);
    glBindTexture_p(GL_TEXTURE_2D, 0);

    glGenFramebuffers_p(1, &fbo);
    glBindFramebuffer_p(GL_FRAMEBUFFER, fbo);
    glFramebufferTexture2D_p(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, texture, 0);
    GLenum fboStatus = glCheckFramebufferStatus_p(GL_FRAMEBUFFER);
    if (fboStatus != GL_FRAMEBUFFER_COMPLETE) {
        printf("[ColorMC Error] Failed to set up framebuffer: 0x%x\n", fboStatus);
    } else {
        printf("[ColorMC Info] gl framebuffer ok\n");
    }

    glBindTexture_p(GL_TEXTURE_2D, 0);
    glBindFramebuffer_p(GL_FRAMEBUFFER, 0);

    send_data(COMMAND_DISPLAY_READY);

    fflush(stdout);
}

void egl_destroy_context(void * input) {
    printf("[ColorMC Info] egl_destroy_context input: %p\n", input);
    eglDestroyContext_p(display, input);
}

void egl_make_current(void* input) {
    printf("[ColorMC Info] egl_make_current input: %p\n", input);
    if (input == NULL) {
        printf("[ColorMC Info] unbind egl context\n");
        fflush(stdout);
        eglMakeCurrent_p(display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
        return;
    }
    if (surface == EGL_NO_SURFACE) {
        make_buffer();
        egl_create_surface();
        if (eglMakeCurrent_p(display, surface, surface, input) != EGL_TRUE) {
            printf("[ColorMC Error] eglMakeCurrent returned with error: %s\n", getEGLError());
        }
        printf("[ColorMC Info] bind egl context to :%p\n", input);
        gl_create();
    } else {
        if (eglMakeCurrent_p(display, surface, surface, input) != EGL_TRUE) {
            printf("[ColorMC Error] eglMakeCurrent returned with error: %s\n", getEGLError());
        }
        printf("[ColorMC Info] bind egl context to :%p\n", input);
    }
    fflush(stdout);
}

void egl_change_size() {
    printf("[ColorMC Info] egl reload\n");
    fflush(stdout);

    glDeleteTextures_p(1, &texture);
    if (eglImage != EGL_NO_IMAGE_KHR) {
        eglDestroyImageKHR_p(display, eglImage);
        eglImage = EGL_NO_IMAGE_KHR;
    }
    if (a_buffer) {
        AHardwareBuffer_release_p(a_buffer);
        a_buffer = NULL;
    }

    eglMakeCurrent_p(display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
    eglDestroySurface_p(display, surface);
    surface = EGL_NO_SURFACE;

    egl_make_current(context);

    send_data(COMMAND_SET_SIZE);
}

void egl_start_change_size() {
    render_state = RENDER_CHANGE_SIZE;
}

void egl_close() {
    eglMakeCurrent_p(display, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);
    eglDestroySurface_p(display, surface);
    eglDestroyContext_p(display, context);
    eglTerminate_p(display);
    eglReleaseThread_p();
}

bool egl_init() {
    printf("[ColorMC Info] egl init\n");

    char *env = getenv("glfwstub.windowWidth");
    if (env == NULL) {
        printf("[ColorMC Error] no set glfwstub.windowWidth env\n");
        width = 0;
    }
    else {
        width = strtol(env, NULL, 0);
    }

    if (width <= 0) {
        printf("[ColorMC Error] window width error set to 640\n");
        width = 640;
    }

    env = getenv("glfwstub.windowHeight");
    if (env == NULL) {
        printf("[ColorMC Error] no set glfwstub.windowHeight env\n");
        height = 0;
    }
    else {
        height = strtol(env, NULL, 0);
    }

    if (height <= 0) {
        printf("[ColorMC Error] window height error set to 640\n");
        height = 480;
    }

    if (game_sock_server() == false) {
        printf("[ColorMC Error] sock init fail\n");
        fflush(stdout);
        return false;
    }

    if (render_sock_server() == false) {
        printf("[ColorMC Error] sock init fail\n");
        fflush(stdout);
        return false;
    }

    while (!can_run) {
        printf("[ColorMC Info] wait run start\n");
        fflush(stdout);
        sleep(1);
    }

    if (egl_load() == false) {
        printf("[ColorMC Error] egl load fail\n");
        fflush(stdout);
        return false;
    }

    if (gl_load() == false) {
        printf("[ColorMC Error] gl load fail\n");
        fflush(stdout);
        return false;
    }

    if (ah_load() == false) {
        printf("[ColorMC Error] AH load fail\n");
        fflush(stdout);
        return false;
    }

    if (!egl_create()) {
        printf("[ColorMC Error] Egl create fail\n");
        fflush(stdout);
        return false;
    }

    fflush(stdout);

    return true;
}

void egl_swap_buffers() {
    glBindFramebuffer_p(GL_READ_FRAMEBUFFER, 0);
    glBindFramebuffer_p(GL_DRAW_FRAMEBUFFER, fbo);
    glBlitFramebuffer_p(0, 0, width, height, 0, 0, width, height, GL_COLOR_BUFFER_BIT, GL_NEAREST);

    glBindFramebuffer_p(GL_READ_FRAMEBUFFER, 0);
    glBindFramebuffer_p(GL_DRAW_FRAMEBUFFER, 0);

    if(render_state == RENDER_CHANGE_SIZE) {
        render_state = RENDER_RUN;
        send_screen_size(width, height);
        egl_change_size();
    }
}

bool sendTexture(int sock) {
    if (a_buffer == NULL) {
        return false;
    }
    return AHardwareBuffer_sendHandleToUnixSocket_p(a_buffer, sock) == 0;
}

//EXTERNAL_API int run(int argc, char** args) {
//    printf("[Info] run start\n");
//
//    if (game_sock_server() == false) {
//        printf("[Error] sock init fail\n");
//        return 1;
//    }
//
//    while (!can_run) {
//        printf("[Info] wait run start\n");
//        sleep(1);
//    }
//
//    if (egl_load() == false) {
//        printf("[Error] egl load fail\n");
//        return 1;
//    }
//
//    if (gl_load() == false) {
//        printf("[Error] gl load fail\n");
//        return 1;
//    }
//
//    if (ah_load() == false) {
//        printf("[Error] AH load fail\n");
//        return 1;
//    }
//    egl_create();
//    egl_create_context(NULL);
//
//    egl_make_current(context);
//
//    if (render_sock_server() == false) {
//        printf("[Error] sock init fail\n");
//        return 1;
//    }
//
//    //render test
//    gl_init();
//
//    render_state = RENDER_RUN;
//
//    while (true) {
//        switch (render_state) {
//            case RENDER_RUN:
//                gl_draw();
//                egl_swap_buffers();
//                GLenum error;
//                while ((error = glGetError_p()) != GL_NO_ERROR) {
//                    printf("[Error] OpenGL error status: 0x%x\n", error);
//                    fflush(stdout);
//                }
//                break;
//            case RENDER_CHANGE_SIZE:
//                egl_change_size();
//                render_state = RENDER_RUN;
//                break;
//        }
//        usleep(1000);
//    }
//
//    /*printf("gl clear\n");
//    gl_clear();
//
//    printf("egl clear\n");
//    egl_clear();*/
//
//    printf("run exit\n");
//
//    return 0;
//}