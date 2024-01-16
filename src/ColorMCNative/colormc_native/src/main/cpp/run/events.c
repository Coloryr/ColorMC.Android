//
// Created by 40206 on 2024/1/11.
//

#include <stddef.h>
#include <math.h>
#include <stdatomic.h>
#include <jni.h>
#include <stdio.h>
#include <stdlib.h>

#include "events.h"

#define EVENT_WINDOW_SIZE 8000 / 2

GLFWInputEvent events[EVENT_WINDOW_SIZE];

GLFW_invoke_Char_func* GLFW_invoke_Char;
GLFW_invoke_CharMods_func* GLFW_invoke_CharMods;
GLFW_invoke_CursorEnter_func* GLFW_invoke_CursorEnter;
GLFW_invoke_CursorPos_func* GLFW_invoke_CursorPos;
GLFW_invoke_FramebufferSize_func* GLFW_invoke_FramebufferSize;
GLFW_invoke_Key_func* GLFW_invoke_Key;
GLFW_invoke_MouseButton_func* GLFW_invoke_MouseButton;
GLFW_invoke_Scroll_func* GLFW_invoke_Scroll;
GLFW_invoke_WindowSize_func* GLFW_invoke_WindowSize;

size_t outEventIndex = 0; // Point to the current event that has yet to be pumped out to MC
size_t outTargetIndex = 0; // Point to the newt index to stop by

atomic_size_t eventCounter; // Count the number of events to be pumped out
size_t inEventIndex; // Point to the next event that has to be filled
size_t inEventCount; // Count registered right before pumping OUT events. Used as a cache.

double cursorX, cursorY, cLastX, cLastY;

bool isInputReady, isCursorEntered, isUseStackQueueCall, isPumpingEvents;
bool isGrabbing;

extern long showingWindow;

extern jclass vmGlfwClass;
extern JNIEnv* jre_env;
extern jmethodID method_internalWindowSizeChanged;

void event_init() {
    char *temp = getenv("GAME_V2");
    if (temp != NULL) {
        printf("[ColorMC Info] UseStackQueueCall\n");
        fflush(stdout);
        isUseStackQueueCall = true;
    } else {
        isUseStackQueueCall = false;
    }
}

void handleFramebufferSizeJava(long window, int w, int h) {
    printf("[ColorMC Info] events set window size\n");
    fflush(stdout);
    (*jre_env)->CallStaticVoidMethod(jre_env, vmGlfwClass, method_internalWindowSizeChanged, (long)window, w, h);
}

void start_event(void* window) {
    if (isPumpingEvents == true) {
        return;
    }
    isPumpingEvents = true;

    size_t index = outEventIndex;
    size_t targetIndex = outTargetIndex;

    while (targetIndex != index) {
        GLFWInputEvent event = events[index];
        switch (event.type) {
            case EVENT_TYPE_CHAR:
                if (GLFW_invoke_Char)
                    GLFW_invoke_Char(window, event.i1);
                break;
            case EVENT_TYPE_CHAR_MODS:
                if (GLFW_invoke_CharMods)
                    GLFW_invoke_CharMods(window, event.i1, event.i2);
                break;
            case EVENT_TYPE_KEY:
                if (GLFW_invoke_Key)
                    GLFW_invoke_Key(window, event.i1, event.i2, event.i3, event.i4);
                break;
            case EVENT_TYPE_MOUSE_BUTTON:
                if (GLFW_invoke_MouseButton)
                    GLFW_invoke_MouseButton(window, event.i1, event.i2, event.i3);
                break;
            case EVENT_TYPE_SCROLL:
                if (GLFW_invoke_Scroll)
                    GLFW_invoke_Scroll(window, event.f1, event.f2);
                break;
            case EVENT_TYPE_FRAMEBUFFER_SIZE:
                handleFramebufferSizeJava(showingWindow, event.i1, event.i2);
                if (GLFW_invoke_FramebufferSize)
                    GLFW_invoke_FramebufferSize(window, event.i1, event.i2);
                break;
            case EVENT_TYPE_WINDOW_SIZE:
                handleFramebufferSizeJava(showingWindow, event.i1, event.i2);
                if (GLFW_invoke_WindowSize)
                    GLFW_invoke_WindowSize(window, event.i1, event.i2);
                break;
        }

        index++;
        if (index >= EVENT_WINDOW_SIZE)
            index -= EVENT_WINDOW_SIZE;
    }
    if ((cLastX != cursorX ||
         cLastY != cursorY) && GLFW_invoke_CursorPos) {
        cLastX = cursorX;
        cLastY = cursorY;
        GLFW_invoke_CursorPos(window, cursorX, cursorY);
    }

    isPumpingEvents = false;
}

void get_event() {
    outEventIndex = outTargetIndex;

    // New events may have arrived while pumping, so remove only the difference before the start and end of execution
    atomic_fetch_sub_explicit(&eventCounter, inEventCount, memory_order_acquire);
}

void compute_event() {
    size_t counter = atomic_load_explicit(&eventCounter, memory_order_acquire);
    size_t index = outEventIndex;

    unsigned targetIndex = index + counter;
    if (targetIndex >= EVENT_WINDOW_SIZE)
        targetIndex -= EVENT_WINDOW_SIZE;

    // Only accessed by one unique thread, no need for atomic store
    inEventCount = counter;
    outTargetIndex = targetIndex;
}

void send_int_data(int type, int i1, int i2, int i3, int i4) {
    GLFWInputEvent *event = &events[inEventIndex];
    event->type = type;
    event->i1 = i1;
    event->i2 = i2;
    event->i3 = i3;
    event->i4 = i4;

    if (++inEventIndex >= EVENT_WINDOW_SIZE)
        inEventIndex -= EVENT_WINDOW_SIZE;

    atomic_fetch_add_explicit(&eventCounter, 1, memory_order_acquire);
}

void send_float_data(int type, float f1, float f2, float f3, float f4) {
    GLFWInputEvent *event = &events[inEventIndex];
    event->type = type;
    event->f1 = f1;
    event->f2 = f2;
    event->f3 = f3;
    event->f4 = f4;

    if (++inEventIndex >= EVENT_WINDOW_SIZE)
        inEventIndex -= EVENT_WINDOW_SIZE;

    atomic_fetch_add_explicit(&eventCounter, 1, memory_order_acquire);
}

void send_char(char codepoint) {
    if (GLFW_invoke_Char && isInputReady) {
        if (isUseStackQueueCall) {
            send_int_data(EVENT_TYPE_CHAR, codepoint, 0, 0, 0);
        } else {
            GLFW_invoke_Char((void*) showingWindow, (unsigned int) codepoint);
        }
    }
}

void send_char_mods(char codepoint, int mods) {
    if (GLFW_invoke_CharMods && isInputReady) {
        if (isUseStackQueueCall) {
            send_int_data(EVENT_TYPE_CHAR_MODS, (int) codepoint, mods, 0, 0);
        } else {
            GLFW_invoke_CharMods((void*) showingWindow, codepoint, mods);
        }
    }
}

void send_cursor_pos(float x, float y) {
    if (GLFW_invoke_CursorPos && isInputReady) {
        if (!isCursorEntered) {
            if (GLFW_invoke_CursorEnter) {
                isCursorEntered = true;
                if (isUseStackQueueCall) {
                    send_int_data(EVENT_TYPE_CURSOR_ENTER, 1, 0, 0, 0);
                } else {
                    GLFW_invoke_CursorEnter((void*) showingWindow, 1);
                }
            } else if (isGrabbing) {
                // Some Minecraft versions does not use GLFWCursorEnterCallback
                // This is a smart check, as Minecraft will not in grab mode if already not.
                isCursorEntered = true;
            }
        }

        if (!isUseStackQueueCall) {
            GLFW_invoke_CursorPos((void*) showingWindow, (double) (x), (double) (y));
        } else {
            cursorX = x;
            cursorY = y;
        }
    }
}

extern jbyte* keyDownBuffer;
extern jbyte* mouseDownBuffer;

#define max(a,b) \
   ({ __typeof__ (a) _a = (a); \
       __typeof__ (b) _b = (b); \
     _a > _b ? _a : _b; })
void send_key(int key, int scancode, int action, int mods) {
    if (GLFW_invoke_Key && isInputReady) {
        keyDownBuffer[max(0, key-31)] = (jbyte) action;
        if (isUseStackQueueCall) {
            send_int_data(EVENT_TYPE_KEY, key, scancode, action, mods);
        } else {
            GLFW_invoke_Key((void*) showingWindow, key, scancode, action, mods);
        }
    }
}

void send_mouse_button(int button, uint8_t action, int mods) {
    if (GLFW_invoke_MouseButton && isInputReady) {
        mouseDownBuffer[max(0, button)] = (jbyte) action;
        if (isUseStackQueueCall) {
            send_int_data(EVENT_TYPE_MOUSE_BUTTON, button, action, mods, 0);
        } else {
            GLFW_invoke_MouseButton((void*) showingWindow, button, action, mods);
        }
    }
}

void send_screen_size(int width, int height) {
    printf("[ColorMC Info] start change size");
    if (isInputReady) {
        if (GLFW_invoke_FramebufferSize) {
            printf("[ColorMC Info] send buffer change");
            if (isUseStackQueueCall) {
                send_int_data(EVENT_TYPE_FRAMEBUFFER_SIZE, width, height, 0, 0);
            } else {
                GLFW_invoke_FramebufferSize((void *) showingWindow, width, height);
            }
        }

        if (GLFW_invoke_WindowSize) {
            printf("[ColorMC Info] send window change");
            if (isUseStackQueueCall) {
                send_int_data(EVENT_TYPE_WINDOW_SIZE, width, height, 0, 0);
            } else {
                GLFW_invoke_WindowSize((void *) showingWindow, width, height);
            }
        }
    }

    fflush(stdout);
}

void send_scroll(float xoffset, float yoffset) {
    if (GLFW_invoke_Scroll && isInputReady) {
        if (isUseStackQueueCall) {
            send_float_data(EVENT_TYPE_SCROLL, xoffset, yoffset, 0, 0);
        } else {
            GLFW_invoke_Scroll((void *) showingWindow, xoffset, yoffset);
        }
    }
}