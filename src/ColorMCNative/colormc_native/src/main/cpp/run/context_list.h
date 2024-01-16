//
// Created by 40206 on 2024/1/15.
//

#ifndef COLORMCNATIVE_CONTEXT_LIST_H
#define COLORMCNATIVE_CONTEXT_LIST_H

#include <EGL//egl.h>
#include <stdbool.h>

#include "GL/gl.h"

typedef struct {
    EGLContext context;
    GLuint fbo;
    GLuint texture;
    bool init;
} context_env;

extern context_env *now_env;

void context_list_init();
context_env * context_find_empty();
context_env * context_find_match(EGLContext context);
void context_remove(EGLContext context);

#endif //COLORMCNATIVE_CONTEXT_LIST_H
