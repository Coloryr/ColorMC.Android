//
// Created by 40206 on 2024/1/10.
//
#ifndef COLORMCNATIVE_GL_LOADER_H
#define COLORMCNATIVE_GL_LOADER_H

#include <stdbool.h>

#include "../GL/gl.h"

extern void (*glGenFramebuffers_p)(GLsizei, GLuint*);
extern void (*glBindFramebuffer_p)(GLenum, GLuint);
extern void (*glFramebufferTexture2D_p)(GLenum, GLenum, GLenum, GLuint, GLint);
extern GLenum(*glCheckFramebufferStatus_p)(GLenum);
extern void (*glDeleteFramebuffers_p)(GLsizei, const GLuint*);
extern void (*glGenTextures_p)(GLsizei, GLuint*);
extern void (*glBindTexture_p)(GLenum, GLuint);
extern void (*glTexParameteri_p)(GLenum, GLenum, GLint);
extern void (*glTexImage2D_p)(GLenum, GLint, GLint, GLsizei, GLsizei, GLint, GLenum, GLenum, const GLvoid*);
extern void (*glGenBuffers_p)(GLsizei, GLuint*);
extern void (*glBindBuffer_p)(GLenum, GLuint);
extern void (*glBufferData_p)(GLenum, GLsizeiptr, const void*, GLenum);
extern void (*glVertexAttribPointer_p)(GLuint, GLint, GLenum, GLboolean, GLsizei, const void*);
extern void (*glEnableVertexAttribArray_p)(GLuint);
extern void (*glUseProgram_p)(GLuint);
extern void (*glDrawArrays_p)(GLenum, GLint, GLsizei);
extern GLuint(*glCreateShader_p)(GLenum);
extern void (*glCompileShader_p)(GLuint);
extern GLuint(*glCreateProgram_p)(void);
extern void (*glAttachShader_p)(GLuint, GLuint);
extern void (*glLinkProgram_p)(GLuint);
extern void (*glDeleteShader_p)(GLuint);
extern void (*glShaderSource_p)(GLuint shader, GLsizei count, const GLchar* const* string, const GLint* length);
extern void (*glDeleteTextures_p)(GLsizei n, const GLuint* textures);
extern void (*glViewport_p)(GLint x, GLint y,GLsizei width, GLsizei height);
extern void (*glClearColor_p)(GLclampf red, GLclampf green, GLclampf blue, GLclampf alpha);
extern void (*glClear_p)(GLbitfield mask);
extern void (*glGetIntegerv_p)(GLenum pname, GLint* params);
extern void (*glReadPixels_p)(GLint x, GLint y,GLsizei width, GLsizei height,GLenum format, GLenum type, GLvoid* pixels);
extern void (*glGetShaderiv_p)(GLuint shader, GLenum pname, GLint* params);
extern void (*glGetShaderInfoLog_p)(GLuint shader, GLsizei bufSize, GLsizei* length, GLchar* infoLog);
extern void (*glGetProgramiv_p)(GLuint program, GLenum pname, GLint* params);
extern void (*glGetProgramInfoLog_p)(GLuint program, GLsizei bufSize, GLsizei* length, GLchar* infoLog);
extern void (*glGenVertexArrays_p)(GLsizei, GLuint*);
extern void (*glBindVertexArray_p)(GLuint);
extern void (*glFlush_p)();
extern GLenum (*glGetError_p)();
extern void (*glBlitFramebuffer_p)(GLint srcX0, GLint srcY0, GLint srcX1, GLint srcY1, GLint dstX0, GLint dstY0, GLint dstX1, GLint dstY1, GLbitfield mask, GLenum filter);
extern void (*glTexSubImage2D_p)(GLenum target, GLint level,GLint xoffset, GLint yoffset,GLsizei width, GLsizei height,
                                 GLenum format, GLenum type, const GLvoid *pixels );
extern void (*glCopyTexImage2D_p)( GLenum target, GLint level,GLenum internalformat,GLint x, GLint y,
                            GLsizei width, GLsizei height,GLint border );

bool gl_load();

#endif //COLORMCNATIVE_GL_LOADER_H
