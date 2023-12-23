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
        return 1;
    }

    // 指定服务器套接字的名称
    struct sockaddr_un remote;
    memset(&remote, 0, sizeof(struct sockaddr_un));
    remote.sun_family = AF_UNIX;
    remote.sun_path[0] = 0;
    int len = strlen(arg);
    for (int a = 0; a < len; a++)
    {
        remote.sun_path[a + 1] = arg[a];
    }

    // 连接到服务器
    if (connect(sock, (struct sockaddr*)&remote, sizeof(struct sockaddr_un)) < 0)
    {
        perror("connect error");
        return 1;
    }

    printf("sock connected\n");

    const char* message = "test\n";
    int length = strlen(message);
    ssize_t bytes_sent = write(sock, message, length);

    if (bytes_sent < 0) {
        // 发送失败，处理错误
        perror("send failed");
        return 1;
    }
    else
    {
        printf("sock write:%d\n", bytes_sent);
    }

    printf("release sock\n");
    // 关闭套接字
    close(sock);

    printf("exit\n");

    return 0;
}