#include <sys/mman.h>
#include <fcntl.h>
#include <linux/ashmem.h>

#include <sys/socket.h>
#include <sys/un.h>

#include <stdlib.h>
#include <stdio.h>
#include <unistd.h>
#include <string.h>

int main(int argc, char** args)
{
    const char* arg = getenv("GAME_FD");
    if (arg == NULL)
    {
        printf("get fd: fail\n");
        exit(1);
    }

    printf("get fd: %s\n", arg);

    int sock;
    if ((sock = socket(AF_UNIX, SOCK_STREAM, 0)) < 0)
    {
        perror("client socket error");
        exit(1);
    }

    // 指定服务器套接字的名称
    struct sockaddr_un remote;
    memset(&remote, 0, sizeof(remote));
    remote.sun_family = AF_UNIX;
    strcpy(remote.sun_path, arg);

    // 连接到服务器
    if (connect(sock, (struct sockaddr*)&remote, sizeof(struct sockaddr_un)) < 0)
    {
        perror("connect error");
        exit(1);
    }

    const char* message = "test\n";
    int length = strlen(message);
    ssize_t bytes_sent = write(sock, message, length);

    if (bytes_sent < 0) {
        // 发送失败，处理错误
        perror("send failed");
        exit(1);
    }

    printf("release ptr\n");
    // 关闭套接字
    close(sock);

    printf("exit\n");

    exit(0);
}