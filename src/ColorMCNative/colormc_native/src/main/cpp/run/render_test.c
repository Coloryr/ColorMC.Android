////
//// Created by 40206 on 2024/1/10.
////
//
//#include <stdio.h>
//#include <malloc.h>
//
//#include "GL/gl.h"
//#include "render_test.h"
//#include "dl_loader/gl_loader.h"
//#include "dl_loader/egl_loader.h"
//
//GLuint shaderProgram;
//GLuint VBO, VAO;
//
//const GLchar* vertexShaderSource =
//        "#version 330 core\n"
//        "layout (location = 0) in vec3 position;\n"
//        "void main() {\n"
//        "    gl_Position = vec4(position, 1.0);\n"
//        "}";
//
//const GLchar* fragmentShaderSource =
//        "#version 330 core\n"
//        "out vec4 color;\n"
//        "void main() {\n"
//        "    color = vec4(1.0, 0.5, 0.2, 1.0);\n"
//        "}";
//
//GLuint texture1;
//GLuint fbo1;
//
//extern int width;
//extern int height;
//
//void gl_init() {
//    int success;
//    char infoLog[512];
//
//    printf("gl create shader\n");
//
//    // 创建和编译顶点着色器
//    GLuint vertexShader = glCreateShader_p(GL_VERTEX_SHADER);
//    glShaderSource_p(vertexShader, 1, &vertexShaderSource, NULL);
//    glCompileShader_p(vertexShader);
//
//    glGetShaderiv_p(vertexShader, GL_COMPILE_STATUS, &success);
//    if (!success) {
//        glGetShaderInfoLog_p(vertexShader, 512, NULL, infoLog);
//        printf("shader VERTEX error %s\n", infoLog);
//        return;
//    }
//
//    // 创建和编译片段着色器
//    GLuint fragmentShader = glCreateShader_p(GL_FRAGMENT_SHADER);
//    glShaderSource_p(fragmentShader, 1, &fragmentShaderSource, NULL);
//    glCompileShader_p(fragmentShader);
//    // 检查编译错误 ...
//
//    glGetShaderiv_p(vertexShader, GL_COMPILE_STATUS, &success);
//    if (!success) {
//        glGetShaderInfoLog_p(vertexShader, 512, NULL, infoLog);
//        printf("shader FRAGMENT error %s\n", infoLog);
//        return;
//    }
//
//    // 创建程序，附加着色器，然后链接它们
//    shaderProgram = glCreateProgram_p();
//    glAttachShader_p(shaderProgram, vertexShader);
//    glAttachShader_p(shaderProgram, fragmentShader);
//    glLinkProgram_p(shaderProgram);
//
//    glGetProgramiv_p(shaderProgram, GL_LINK_STATUS, &success);  // 检测链接结果
//    if (!success) {
//        glGetProgramInfoLog_p(shaderProgram, 512, NULL, infoLog);
//        printf("shader link error %s\n", infoLog);
//        return;
//    }
//
//    // 删除着色器，它们已经链接到我们的程序中了，不再需要了
//    glDeleteShader_p(vertexShader);
//    glDeleteShader_p(fragmentShader);
//
//    printf("gl create VAO\n");
//
//    // 设置顶点数据
//    GLfloat vertices[] = {
//            -0.5f, -0.5f, 0.0f, // 左下角
//            0.5f, -0.5f, 0.0f, // 右下角
//            0.0f, 0.5f, 0.0f  // 顶部
//    };
//
//    glGenVertexArrays_p(1, &VAO);
//    glGenBuffers_p(1, &VBO);
//
//    glBindVertexArray_p(VAO);
//    glBindBuffer_p(GL_ARRAY_BUFFER, VBO);
//
//    glBufferData_p(GL_ARRAY_BUFFER, sizeof(vertices), vertices, GL_STATIC_DRAW);
//
//    // 设置顶点属性指针
//    glVertexAttribPointer_p(0, 3, GL_FLOAT, GL_FALSE, 3 * sizeof(GLfloat), (void *) 0);
//    glEnableVertexAttribArray_p(0);
//
//    glBindBuffer_p(GL_ARRAY_BUFFER, 0);
//    glBindVertexArray_p(0);
//
//    glGenTextures_p(1, &texture1);
//    glBindTexture_p(GL_TEXTURE, texture1);
//    glTexImage2D_p(GL_TEXTURE_2D, 0, GL_RGBA, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, NULL);
//    glTexParameteri_p(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
//    glTexParameteri_p(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
//
//    glGenFramebuffers_p(1, &fbo1);
//    glBindFramebuffer_p(GL_FRAMEBUFFER, fbo1);
//    glFramebufferTexture2D_p(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, texture1, 0);
//    GLenum fboStatus = glCheckFramebufferStatus_p(GL_FRAMEBUFFER);
//    if (fboStatus != GL_FRAMEBUFFER_COMPLETE) {
//        printf("[Error] Failed to set up framebuffer: 0x%x\n", fboStatus);
//        fflush(stdout);
//    } else {
//        printf("[Info] gl framebuffer ok\n");
//        fflush(stdout);
//    }
//
//    glBindTexture_p(GL_TEXTURE_2D, 0);
//    glBindFramebuffer_p(GL_FRAMEBUFFER, 0);
//}
//
//float red = 0.0f;
//
////一个像素的颜色信息
//typedef struct RGBColor
//{
//    char B;		//蓝
//    char G;		//绿
//    char R;		//红
//} RGBColor;
//
//void WriteBMP(const char* FileName, RGBColor* ColorBuffer, int ImageWidth, int ImageHeight);
//
//bool first = true;
//
//void gl_draw() {
//    //glBindFramebuffer_p(GL_FRAMEBUFFER, fbo1);
//    glViewport_p(0, 0, width, height);
//    glClearColor_p(red, 0.0, 0.0, 1.0);
//
//    red += 0.01;
//    if (red > 1.0f) {
//        red = 0;
//    }
//
//    glClear_p(GL_COLOR_BUFFER_BIT);
//
//    glBindBuffer_p(GL_ARRAY_BUFFER, VBO);
//    glBindVertexArray_p(VAO);
//
//    glUseProgram_p(shaderProgram);
//
//    // 渲染一个三角形
//    glDrawArrays_p(GL_TRIANGLES, 0, 3);
//
//    glUseProgram_p(0);
//
//    glBindBuffer_p(GL_ARRAY_BUFFER, 0);
//    glBindVertexArray_p(0);
//    //glBindFramebuffer_p(GL_FRAMEBUFFER, 0);
//
//    glFlush_p();
//}
//
//void save_bmp(){
//    if (first) {
//        first = false;
//        RGBColor *data = malloc(640 * 480 * sizeof(RGBColor)); // 4 代表 RGBA
//        if (glReadPixels_p == NULL) {
//            printf("glReadPixels is null\n");
//            return;
//        }
//        glReadPixels_p(0, 0, 640, 480, GL_BGR, GL_UNSIGNED_BYTE, data);
//
//        // 保存为 PNG
//        WriteBMP("/data/user/0/coloryr.colormc.android/files/output.png", data, 640, 480);
//
//        // 释放内存
//        free(data);
//    }
//}