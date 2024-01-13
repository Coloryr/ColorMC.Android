//
// Created by 40206 on 2024/1/11.
//

#ifndef COLORMCNATIVE_EVENTS_H
#define COLORMCNATIVE_EVENTS_H

#include <stddef.h>

#define EVENT_TYPE_CHAR 1000
#define EVENT_TYPE_CHAR_MODS 1001
#define EVENT_TYPE_CURSOR_ENTER 1002
#define EVENT_TYPE_FRAMEBUFFER_SIZE 1004
#define EVENT_TYPE_KEY 1005
#define EVENT_TYPE_MOUSE_BUTTON 1006
#define EVENT_TYPE_SCROLL 1007
#define EVENT_TYPE_WINDOW_SIZE 1008

typedef struct {
    int type;
    int i1;
    int i2;
    int i3;
    int i4;
} GLFWInputEvent;

__attribute__((unused)) typedef void GLFW_invoke_Char_func(void* window, unsigned int codepoint);
typedef void GLFW_invoke_CharMods_func(void* window, unsigned int codepoint, int mods);
typedef void GLFW_invoke_CursorEnter_func(void* window, int entered);
typedef void GLFW_invoke_CursorPos_func(void* window, double xpos, double ypos);
typedef void GLFW_invoke_FramebufferSize_func(void* window, int width, int height);
typedef void GLFW_invoke_Key_func(void* window, int key, int scancode, int action, int mods);
typedef void GLFW_invoke_MouseButton_func(void* window, int button, int action, int mods);
typedef void GLFW_invoke_Scroll_func(void* window, double xoffset, double yoffset);
typedef void GLFW_invoke_WindowSize_func(void* window, int width, int height);

extern GLFW_invoke_Char_func* GLFW_invoke_Char;
extern GLFW_invoke_CharMods_func* GLFW_invoke_CharMods;
extern GLFW_invoke_CursorEnter_func* GLFW_invoke_CursorEnter;
extern GLFW_invoke_CursorPos_func* GLFW_invoke_CursorPos;
extern GLFW_invoke_FramebufferSize_func* GLFW_invoke_FramebufferSize;
extern GLFW_invoke_Key_func* GLFW_invoke_Key;
extern GLFW_invoke_MouseButton_func* GLFW_invoke_MouseButton;
extern GLFW_invoke_Scroll_func* GLFW_invoke_Scroll;
extern GLFW_invoke_WindowSize_func* GLFW_invoke_WindowSize;

extern size_t outEventIndex;
extern size_t outTargetIndex;

extern double cursorX, cursorY, cLastX, cLastY;

extern bool isInputReady, isCursorEntered, isUseStackQueueCall, isPumpingEvents;
extern bool isGrabbing;

void start_event(void* window);
void get_event();
void compute_event();
void sendData(int type, int i1, int i2, int i3, int i4);
void event_init();

void send_char(char codepoint);
void send_char_mods(char codepoint, int mods);
void send_cursor_pos(float x, float y);
void send_key(int key, int scancode, int action, int mods);
void send_mouse_button(int button, uint8_t action, int mods);
void send_screen_size(int width, int height);
void send_scroll(double xoffset, double yoffset);

#endif //COLORMCNATIVE_EVENTS_H
