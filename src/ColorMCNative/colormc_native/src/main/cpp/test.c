#include <sys/mman.h>
#include <fcntl.h>
#include <linux/ashmem.h>

#include <sys/socket.h>
#include <sys/un.h>

#include <stdlib.h>
#include <stdio.h>
#include <dlfcn.h>
#include <unistd.h>
#include <string.h>

int main(int argc, char** args)
{
    void * ptr = dlopen("libColorMC_Android_RenderNative.so", RTLD_LAZY);
    int (*run)(int, char) = dlsym(ptr, "run");
    return run(argc, args);
}