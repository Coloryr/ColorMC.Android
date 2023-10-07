#pragma once

#include <android/native_window_jni.h>

#define RENDERER_GL4ES 1
#define RENDERER_VK_ZINK 2
#define RENDERER_VIRGL 3
#define RENDERER_VULKAN 4

extern int game_render;

extern ANativeWindow* game_window;