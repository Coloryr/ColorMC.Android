//
// Created by 40206 on 2024/1/10.
//

#ifndef COLORMCNATIVE_GAME_SOCK_H
#define COLORMCNATIVE_GAME_SOCK_H

#include <stdbool.h>

enum COMMAND_TYPE {
    /**
     * 开始启动游戏
     */
    COMMAND_RUN = 0,
    /**
     * 设置显示分辨率
     */
    COMMAND_SET_SIZE,
    /**
     * 显示就绪
     */
    COMMAND_DISPLAY_READY,
    /**
     * 发送字符
     */
    COMMAND_SEND_CHAR,
    /**
     * 发送字符模式
     */
    COMMAND_SEND_CHAR_MODS,
    /**
     * 发送光标位置
     */
    COMMAND_SEND_CURSOR_POS,
    /**
     * 发送键盘按键
     */
    COMMAND_SEND_KEY,
    /**
     * 发送鼠标按键
     */
    COMMAND_SEND_MOUSE_BUTTON,
    /**
     * 发送鼠标滚动
     */
    COMMAND_SEND_SCROLL
};

extern bool can_run;

bool game_sock_server();
void send_data(enum COMMAND_TYPE type);

#endif //COLORMCNATIVE_GAME_SOCK_H
