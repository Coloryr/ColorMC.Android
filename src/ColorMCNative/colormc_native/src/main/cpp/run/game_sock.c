//
// Created by 40206 on 2024/1/10.
//

#include <stdio.h>
#include <sys/socket.h>
#include <sys/un.h>
#include <unistd.h>
#include <stdbool.h>
#include <pthread.h>
#include <stdlib.h>
#include <string.h>

#include "game_sock.h"
#include "events.h"

extern bool can_run;
extern int width;
extern int height;

extern void egl_start_change_size();

int game_server_fd;
int game_client_fd;
pthread_t game_tid;

const uint8_t magic_head[] = {'c', 'o', 'l', 'o', 'r', 'y'};

uint8_t game_buffer[8192];

typedef union {
    uint8_t u8[2];
    uint16_t u16;
} U8_U16;

typedef union {
    uint8_t u8[4];
    int32_t i32;
} U8_I32;

typedef union {
    uint8_t u8[4];
    float f;
} U8_FLOAT;

void game_pack_do(uint8_t* buffer, int size) {
    //指令校验不正确
    U8_U16 change;
    U8_FLOAT change1;
    U8_I32 change2;
    uint8_t command = buffer[0];
    float f1, f2, f3;
    int32_t i1, i2, i3;
    switch (command) {
        case COMMAND_RUN:
            can_run = true;
            printf("[ColorMC Info] game can run\n");
            break;
        case COMMAND_SET_SIZE:
            if (size < 5) {
                printf("[ColorMC Error] set game size pack error\n");
                return;
            }
            change.u8[0] = buffer[1];
            change.u8[1] = buffer[2];
            width = change.u16;
            change.u8[0] = buffer[3];
            change.u8[1] = buffer[4];
            height = change.u16;
            egl_start_change_size();
            printf("[ColorMC Info] set game size %d f1 %d\n", width, height);
            break;
        case COMMAND_SEND_CURSOR_POS:
            if (size < 9) {
                printf("[ColorMC Error] set cursor pos pack error\n");
                return;
            }

            change1.u8[0] = buffer[1];
            change1.u8[1] = buffer[2];
            change1.u8[2] = buffer[3];
            change1.u8[3] = buffer[4];
            f1 = change1.f;
            change1.u8[0] = buffer[5];
            change1.u8[1] = buffer[6];
            change1.u8[2] = buffer[7];
            change1.u8[3] = buffer[8];
            f2 = change1.f;

            send_cursor_pos(f1, f2);

            printf("[ColorMC Info] set cursor pos %f %f\n", f1, f2);

            break;
        case COMMAND_SEND_MOUSE_BUTTON:
            if (size < 10) {
                printf("[ColorMC Error] set mouse button pack error\n");
                return;
            }

            change2.u8[0] = buffer[1];
            change2.u8[1] = buffer[2];
            change2.u8[2] = buffer[3];
            change2.u8[3] = buffer[4];
            i1 = change2.i32;
            change2.u8[0] = buffer[5];
            change2.u8[1] = buffer[6];
            change2.u8[2] = buffer[7];
            change2.u8[3] = buffer[8];
            i2 = change2.i32;
            i3 = buffer[9];

            send_mouse_button(i1, i3, i2);

            printf("[ColorMC Info] set mouse button %d %d %d\n", i1, i2, i3);
            break;
    }

    fflush(stdout);
}

void* pthread_game_run(void* arg) {
    printf("[ColorMC Info] start game unix socket accept\n");
    fflush(stdout);
    for (;;) {
        // 接受连接
        game_client_fd = accept(game_server_fd, NULL, NULL);
        if (game_client_fd == -1) {
            close(game_server_fd);
            printf("[ColorMC Error] socket accept fail\n");
            fflush(stdout);
            return NULL;
        }

        for (;;) {
            int size = read(game_client_fd, game_buffer, 8192);
            if (size < 0) {
                break;
            }
            if (size < 6) {
                continue;
            }
            //粘包处理
            for (int i = 0; i < size - 6; ++i) {
                //查找包头
                if (game_buffer[i] == magic_head[0]
                    && game_buffer[i + 1] == magic_head[1]
                    && game_buffer[i + 2] == magic_head[2]
                    && game_buffer[i + 3] == magic_head[3]
                    && game_buffer[i + 4] == magic_head[4]
                    && game_buffer[i + 5] == magic_head[5]) {
                    //数据被截断了
                    if (i + 7 > size) {
                        printf("[ColorMC Error] socket pack error size: %d\n", size);
                        fflush(stdout);
                        continue;
                    }
                    game_pack_do(game_buffer + i + 6, size - i);
                }
            }
        }

        close(game_client_fd);
    }
}

void send_data(enum COMMAND_TYPE type) {
    if (game_client_fd <= 0) {
        return;
    }
    uint8_t buffer[10] = {0};
    for (int i = 0; i < 6; ++i) {
        buffer[i] = magic_head[i];
    }

    buffer[6] = buffer[9] = type;

    send(game_client_fd, buffer, 10, 0);
}

bool game_sock_server() {
    char *name = getenv("GAME_SOCK");
    if (name == NULL) {
        printf("[ColorMC Error] no GAME_SOCK\n");
        fflush(stdout);
        return false;
    }

    struct sockaddr_un server_addr;

    // 创建一个 Unix socket
    game_server_fd = socket(AF_UNIX, SOCK_STREAM, 0);
    if (game_server_fd == -1) {
        printf("[ColorMC Error] game socket create fail\n");
        fflush(stdout);
        return false;
    }

    // 设置 sockaddr_un 结构
    memset(&server_addr, 0, sizeof(server_addr));
    server_addr.sun_family = AF_UNIX;
    strncpy(server_addr.sun_path, name, sizeof(server_addr.sun_path) - 1);
    unlink(name); // 确保路径未被使用

    // 绑定 socket 到路径
    if (bind(game_server_fd, (struct sockaddr *) &server_addr, sizeof(server_addr)) == -1) {
        close(game_server_fd);
        printf("[ColorMC Error] game socket open fail\n");
        fflush(stdout);
        return false;
    }

    // 监听连接
    if (listen(game_server_fd, 5) == -1) {
        close(game_server_fd);
        printf("[ColorMC Error] game socket listen fail\n");
        fflush(stdout);
        return false;
    }

    printf("[ColorMC Info] start game unix socket in %s\n", name);
    fflush(stdout);

    int res = pthread_create(&game_tid, NULL, pthread_game_run, NULL);

    return res == 0;
}