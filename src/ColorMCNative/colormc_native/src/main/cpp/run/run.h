//
// Created by 40206 on 2024/1/11.
//

#ifndef COLORMCNATIVE_RUN_H
#define COLORMCNATIVE_RUN_H

#include <stdbool.h>

bool egl_create();
bool egl_init();
void* egl_create_context(void* context);
void* egl_get_context();
void egl_make_current(void* context);
void egl_swap_interval(int swapInterval);
void egl_close();
void egl_swap_buffers();
void egl_destroy_context(void * input);

#endif //COLORMCNATIVE_RUN_H
