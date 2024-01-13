//
// Created by 40206 on 2024/1/10.
//
#include <stdio.h>
#include <stdlib.h>
#include <stdbool.h>
#include <dlfcn.h>

#include "../GL/gl.h"
#include "gl_loader.h"

void (*glGenFramebuffers_p)(GLsizei, GLuint*);
void (*glBindFramebuffer_p)(GLenum, GLuint);
void (*glFramebufferTexture2D_p)(GLenum, GLenum, GLenum, GLuint, GLint);
GLenum(*glCheckFramebufferStatus_p)(GLenum);
//void (*glDeleteFramebuffers_p)(GLsizei, const GLuint*);
void (*glBindTexture_p)(GLenum, GLuint);
void (*glDeleteTextures_p)(GLsizei n, const GLuint* textures);
void (*glGenTextures_p)(GLsizei, GLuint*);
//GLenum (*glGetError_p)();
void (*glTexParameteri_p)(GLenum, GLenum, GLint);
void (*glTexImage2D_p)(GLenum, GLint, GLint, GLsizei, GLsizei, GLint, GLenum, GLenum, const GLvoid*);
void (*glBlitFramebuffer_p)(GLint srcX0, GLint srcY0, GLint srcX1, GLint srcY1, GLint dstX0, GLint dstY0, GLint dstX1, GLint dstY1, GLbitfield mask, GLenum filter);

//void (*glGenBuffers_p)(GLsizei, GLuint*);
//void (*glBindBuffer_p)(GLenum, GLuint);
//void (*glBufferData_p)(GLenum, GLsizeiptr, const void*, GLenum);
//void (*glVertexAttribPointer_p)(GLuint, GLint, GLenum, GLboolean, GLsizei, const void*);
//void (*glEnableVertexAttribArray_p)(GLuint);
//void (*glUseProgram_p)(GLuint);
//void (*glDrawArrays_p)(GLenum, GLint, GLsizei);
//GLuint(*glCreateShader_p)(GLenum);
//void (*glCompileShader_p)(GLuint);
//GLuint(*glCreateProgram_p)(void);
//void (*glAttachShader_p)(GLuint, GLuint);
//void (*glLinkProgram_p)(GLuint);
//void (*glDeleteShader_p)(GLuint);
//void (*glShaderSource_p)(GLuint shader, GLsizei count, const GLchar* const* string, const GLint* length);
//void (*glViewport_p)(GLint x, GLint y,GLsizei width, GLsizei height);
//void (*glClearColor_p)(GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);
//void (*glClear_p)(GLbitfield mask);
//void (*glGetIntegerv_p)(GLenum pname, GLint* params);
//void (*glReadPixels_p)(GLint x, GLint y,GLsizei width, GLsizei height,GLenum format, GLenum type, GLvoid* pixels);
//void (*glGetShaderiv_p)(GLuint shader, GLenum pname, GLint* params);
//void (*glGetShaderInfoLog_p)(GLuint shader, GLsizei bufSize, GLsizei* length, GLchar* infoLog);
//void (*glGetProgramiv_p)(GLuint program, GLenum pname, GLint* params);
//void (*glGetProgramInfoLog_p)(GLuint program, GLsizei bufSize, GLsizei* length, GLchar* infoLog);
//void (*glGenVertexArrays_p)(GLsizei, GLuint*);
//void (*glBindVertexArray_p)(GLuint);
//void (*glFlush_p)();
//void (*glTexSubImage2D_p)(GLenum target, GLint level,GLint xoffset, GLint yoffset,GLsizei width, GLsizei height,
//        GLenum format, GLenum type, const GLvoid *pixels );
//void (*glCopyTexImage2D_p)( GLenum target, GLint level,GLenum internalformat,GLint x, GLint y,
//        GLsizei width, GLsizei height,GLint border );

bool gl_load() {
    char *gl = getenv("GL_SO");
    if (gl == NULL) {
        printf("[ColorMC Error] no GL_SO\n");
        return false;
    }
    void *libgl = dlopen(gl, RTLD_LAZY);
    if (!libgl) {
        printf("[ColorMC Error] Failed to load GL_SO\n");
        return false;
    }

    void *libGLESv2 = dlopen("libGLESv2.so", RTLD_LAZY);
    if (!libGLESv2) {
        printf("[ColorMC Error] Failed to load libGLESv2.so\n");
        return false;
    }

    glGenFramebuffers_p = dlsym(libGLESv2, "glGenFramebuffers");
    glBindFramebuffer_p = dlsym(libGLESv2, "glBindFramebuffer");
    glFramebufferTexture2D_p = dlsym(libGLESv2, "glFramebufferTexture2D");
    glCheckFramebufferStatus_p = dlsym(libGLESv2, "glCheckFramebufferStatus");
//    glDeleteFramebuffers_p = dlsym(libGLESv2, "glDeleteFramebuffers");
    glBindTexture_p = dlsym(libGLESv2, "glBindTexture");
    glDeleteTextures_p = dlsym(libGLESv2, "glDeleteTextures");
    glGenTextures_p = dlsym(libGLESv2, "glGenTextures");
//    glGetError_p = dlsym(libGLESv2, "glGetError");
//    glFlush_p = dlsym(libGLESv2, "glFlush");
    glTexParameteri_p = dlsym(libGLESv2, "glTexParameteri");
    glBlitFramebuffer_p = dlsym(libGLESv2, "glBlitFramebuffer");
//    glTexSubImage2D_p = dlsym(libGLESv2, "glTexSubImage2D");
//    glCopyTexImage2D_p = dlsym(libGLESv2, "glCopyTexImage2D");
    glTexImage2D_p = dlsym(libGLESv2, "glTexImage2D");

//    glReadPixels_p = dlsym(libgl, "glReadPixels");
//    glGenBuffers_p = dlsym(libgl, "glGenBuffers");
//    glBindBuffer_p = dlsym(libgl, "glBindBuffer");
//    glBufferData_p = dlsym(libgl, "glBufferData");
//    glVertexAttribPointer_p = dlsym(libgl, "glVertexAttribPointer");
//    glEnableVertexAttribArray_p = dlsym(libgl, "glEnableVertexAttribArray");
//    glCreateShader_p = dlsym(libgl, "glCreateShader");
//    glShaderSource_p = dlsym(libgl, "glShaderSource");
//    glCompileShader_p = dlsym(libgl, "glCompileShader");
//    glCreateProgram_p = dlsym(libgl, "glCreateProgram");
//    glAttachShader_p = dlsym(libgl, "glAttachShader");
//    glLinkProgram_p = dlsym(libgl, "glLinkProgram");
//    glDeleteShader_p = dlsym(libgl, "glDeleteShader");
//    glUseProgram_p = dlsym(libgl, "glUseProgram");
//    glDrawArrays_p = dlsym(libgl, "glDrawArrays");
//    glViewport_p = dlsym(libgl, "glViewport");
//    glClearColor_p = dlsym(libgl, "glClearColor");
//    glClear_p = dlsym(libgl, "glClear");
//    glGetIntegerv_p = dlsym(libgl, "glGetIntegerv");
//    glGetShaderiv_p = dlsym(libgl, "glGetShaderiv");
//    glGetShaderInfoLog_p = dlsym(libgl, "glGetShaderInfoLog");
//    glGetProgramiv_p = dlsym(libgl, "glGetProgramiv");
//    glGetProgramInfoLog_p = dlsym(libgl, "glGetProgramInfoLog");
//    glGenVertexArrays_p = dlsym(libgl, "glGenVertexArrays");
//    glBindVertexArray_p = dlsym(libgl, "glBindVertexArray");

    return true;
}