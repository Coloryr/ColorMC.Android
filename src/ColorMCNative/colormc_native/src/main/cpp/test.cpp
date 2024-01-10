#include <stdio.h>
#include <stdlib.h>
#include <dlfcn.h>

int main(int argc, char** args)
{
    //fflush(stdout);
    setbuf(stdout, NULL); // 设置stdout为无缓冲模式
    printf("game load\n");
    char * env = getenv("RUN_SO");
    if(!env)
    {
        printf("no RUN_SO\n");
        return 1; // 返回一个错误码
    }
    else
    {
        printf("RUN_SO: %s\n", env);
    }
    void * ptr = dlopen(env, RTLD_LAZY);
    if (!ptr)
    {
        fprintf(stderr, "Error loading library: %s\n", dlerror());
        return 1; // 返回一个错误码
    }
    int (*run)(int, char**) = (int (*)(int, char **))dlsym(ptr, "run");
    if (!run)
    {
        fprintf(stderr, "Error finding 'run' function: %s\n", dlerror());
        dlclose(ptr); // 关闭库
        return 1; // 返回一个错误码
    }
    printf("game start\n");
    // 调用run函数
    int result = run(argc, args);

    // 关闭库
    dlclose(ptr);

    return result;
}