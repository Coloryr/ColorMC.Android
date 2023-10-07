#include <jni.h>
#include <android/native_window_jni.h>

#include "abi_info.h"
#include "log.h"
#include "render.h"

ANativeWindow* game_window;

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