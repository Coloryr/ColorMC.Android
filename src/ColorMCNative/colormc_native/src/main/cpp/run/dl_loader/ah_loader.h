//
// Created by 40206 on 2024/1/10.
//

#ifndef COLORMCNATIVE_AH_LOADER_H
#define COLORMCNATIVE_AH_LOADER_H

#include <android/hardware_buffer.h>
#include <stdbool.h>

extern int (*AHardwareBuffer_allocate_p)(const AHardwareBuffer_Desc *, AHardwareBuffer **);
extern void (*AHardwareBuffer_release_p)(AHardwareBuffer *);
extern int (*AHardwareBuffer_lock_p)(AHardwareBuffer *buffer, uint64_t usage, int32_t fence,
                              const ARect *rect, void **outVirtualAddress);
extern int (*AHardwareBuffer_unlock_p)(AHardwareBuffer *buffer, int32_t *fence);
extern void (*AHardwareBuffer_describe_p)(const AHardwareBuffer *buffer,
                                   AHardwareBuffer_Desc *outDesc);
extern void (*AHardwareBuffer_acquire_p)(AHardwareBuffer *buffer);
extern int(*AHardwareBuffer_sendHandleToUnixSocket_p)(const AHardwareBuffer* buffer, int socketFd);

bool ah_load();

#endif //COLORMCNATIVE_AH_LOADER_H
