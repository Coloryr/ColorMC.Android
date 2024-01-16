//
// Created by 40206 on 2024/1/10.
//

#include <stddef.h>
#include <stdlib.h>
#include <dlfcn.h>
#include <stdbool.h>
#include <android/hardware_buffer.h>

#include "ah_loader.h"

int (*AHardwareBuffer_allocate_p)(const AHardwareBuffer_Desc *, AHardwareBuffer **);
void (*AHardwareBuffer_release_p)(AHardwareBuffer *);
int (*AHardwareBuffer_lock_p)(AHardwareBuffer *buffer, uint64_t usage, int32_t fence,
                                         const ARect *rect, void **outVirtualAddress);
int (*AHardwareBuffer_unlock_p)(AHardwareBuffer *buffer, int32_t *fence);
void (*AHardwareBuffer_describe_p)(const AHardwareBuffer *buffer,
                                              AHardwareBuffer_Desc *outDesc);
void (*AHardwareBuffer_acquire_p)(AHardwareBuffer *buffer);
int(*AHardwareBuffer_sendHandleToUnixSocket_p)(const AHardwareBuffer* _Nonnull buffer, int socketFd);

bool ah_load() {
    void *al_dl_handel = dlopen("libandroid.so", RTLD_LAZY);
    if (al_dl_handel == NULL) {
        printf("[ColorMC Error] dlopen fail %s\n", dlerror());
        return false;
    }

    AHardwareBuffer_allocate_p = dlsym(al_dl_handel, "AHardwareBuffer_allocate");
    AHardwareBuffer_release_p = dlsym(al_dl_handel, "AHardwareBuffer_release");
    AHardwareBuffer_lock_p = dlsym(al_dl_handel, "AHardwareBuffer_lock");
    AHardwareBuffer_unlock_p = dlsym(al_dl_handel, "AHardwareBuffer_unlock");
    AHardwareBuffer_describe_p = dlsym(al_dl_handel, "AHardwareBuffer_describe");
    AHardwareBuffer_acquire_p = dlsym(al_dl_handel, "AHardwareBuffer_acquire");
    AHardwareBuffer_sendHandleToUnixSocket_p = dlsym(al_dl_handel,
                                                     "AHardwareBuffer_sendHandleToUnixSocket");

    return true;
}