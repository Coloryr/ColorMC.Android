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

#include "render_sock.h"
#include "run_shared_texture.h"

pthread_t render_tid;

int server_fd;

extern bool sendTexture(int sock);

void* pthread_render_run(void* arg) {
    printf("[ColorMC Info] start render unix socket accept\n");
    fflush(stdout);
    for (;;) {
        // 接受连接
        int client_fd = accept(server_fd, NULL, NULL);
        if (client_fd == -1) {
            close(server_fd);
            printf("[ColorMC Error] socket accept fail\n");
            fflush(stdout);
            return NULL;
        }

        printf("[ColorMC Info] send render buffer to client\n");
        fflush(stdout);
        // 向客户端发送数据

        if (sendTexture(client_fd) == false) {
            printf("[ColorMC Error] send render buffer fail\n");
        }
        // 关闭socket连接
        close(client_fd);
    }
}

bool render_sock_server() {
    char *name = getenv("RENDER_SOCK");
    if (name == NULL) {
        printf("[ColorMC Error] no RENDER_SOCK\n");
        fflush(stdout);
        return false;
    }

    struct sockaddr_un server_addr;

    // 创建一个 Unix socket
    server_fd = socket(AF_UNIX, SOCK_STREAM, 0);
    if (server_fd == -1) {
        printf("[ColorMC Error] render socket create fail\n");
        fflush(stdout);
        return false;
    }

    // 设置 sockaddr_un 结构
    memset(&server_addr, 0, sizeof(server_addr));
    server_addr.sun_family = AF_UNIX;
    strncpy(server_addr.sun_path, name, sizeof(server_addr.sun_path) - 1);
    unlink(name); // 确保路径未被使用

    // 绑定 socket 到路径
    if (bind(server_fd, (struct sockaddr *) &server_addr, sizeof(server_addr)) == -1) {
        close(server_fd);
        printf("[ColorMC Error] render socket open fail\n");
        fflush(stdout);
        return false;
    }

    // 监听连接
    if (listen(server_fd, 5) == -1) {
        close(server_fd);
        printf("[ColorMC Error] render socket listen fail\n");
        fflush(stdout);
        return false;
    }

    printf("[ColorMC Info] start render unix socket in %s\n", name);
    fflush(stdout);

    int res = pthread_create(&render_tid, NULL, pthread_render_run, NULL);

    return res == 0;
}