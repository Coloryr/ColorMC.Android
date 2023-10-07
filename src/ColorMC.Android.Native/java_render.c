#include <jni.h>
#include <android/native_window_jni.h>

#include "abi_info.h"
#include "log.h"

ANativeWindow* game_window;

#define RENDERER_GL4ES 1
#define RENDERER_VK_ZINK 2
#define RENDERER_VIRGL 3
#define RENDERER_VULKAN 4

int game_render;

void release_window() 
{
    ANativeWindow_release(game_window);
}

void setup_window(jobject surface) 
{
    game_window = ANativeWindow_fromSurface(env, surface);
    if (game_render == RENDERER_GL4ES)
    {
        //gl_setup_window();
    }

    if (game_window == NULL)
    {
        LOGE("Window Error");
    }
    else
    {
        LOGI("Window Set");
    }
}