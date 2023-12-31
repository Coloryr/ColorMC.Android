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

    int sockfd;
    struct sockaddr_un addr;
    char buffer[1024] = "Hello from C client!";
    char recvBuffer[1024];

    // 创建socket
    if ((sockfd = socket(AF_UNIX, SOCK_STREAM, 0)) < 0)
    {
        perror("socket error");
        exit(EXIT_FAILURE);
    }

    // 设置地址结构
    memset(&addr, 0, sizeof(addr));
    addr.sun_family = AF_UNIX;
    strncpy(addr.sun_path, arg, sizeof(addr.sun_path) - 1);

    // 连接到服务器
    if (connect(sockfd, (struct sockaddr*)&addr, sizeof(addr)) < 0)
    {
        perror("connect error");
        exit(EXIT_FAILURE);
    }

    // 发送数据
    write(sockfd, buffer, strlen(buffer));
    printf("Message sent: %s\n", buffer);

    // 接收数据
    ssize_t receivedBytes;
    if ((receivedBytes = read(sockfd, recvBuffer, sizeof(recvBuffer) - 1)) > 0)
    {
        recvBuffer[receivedBytes] = '\0'; // 确保字符串结束
        printf("Received: %s\n", recvBuffer);
    }

    // 关闭socket
    close(sockfd);

    return 0;
}