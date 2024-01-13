#include <stdlib.h>

#pragma pack(2)//影响了“对齐”。可以实验前后 sizeof(BITMAPFILEHEADER) 的差别

typedef unsigned char  BYTE;
typedef unsigned short WORD;
typedef unsigned long  DWORD;
typedef long    LONG;

//BMP文件头（14字节）
typedef struct                       /**** BMP file header structure ****/
{
    unsigned int   bfSize;           /* Size of file */
    unsigned short bfReserved1;      /* Reserved */
    unsigned short bfReserved2;      /* ... */
    unsigned int   bfOffBits;        /* Offset to bitmap data */
} MyBITMAPFILEHEADER;

//位图信息头（40字节）
typedef struct                       /**** BMP file info structure ****/
{
    unsigned int   biSize;           /* Size of info header */
    int            biWidth;          /* Width of image */
    int            biHeight;         /* Height of image */
    unsigned short biPlanes;         /* Number of color planes */
    unsigned short biBitCount;       /* Number of bits per pixel */
    unsigned int   biCompression;    /* Type of compression to use */
    unsigned int   biSizeImage;      /* Size of image data */
    int            biXPelsPerMeter;  /* X pixels per meter */
    int            biYPelsPerMeter;  /* Y pixels per meter */
    unsigned int   biClrUsed;        /* Number of colors used */
    unsigned int   biClrImportant;   /* Number of important colors */
} MyBITMAPINFOHEADER;

//一个像素的颜色信息
typedef struct RGBColor
{
    char B;		//蓝
    char G;		//绿
    char R;		//红
} RGBColor;

//将颜色数据写到一个BMP文件中
//FileName:文件名
//ColorBuffer:颜色数据
//ImageWidth:图像宽度
//ImageHeight:图像长度
void WriteBMP(const char* name, RGBColor* buffer, int width, int height)
{
    MyBITMAPFILEHEADER bfh;
    MyBITMAPINFOHEADER bih;
    /* Magic number for file. It does not fit in the header structure due to alignment requirements, so put it outside */
    unsigned short bfType = 0x4d42;
    bfh.bfReserved1 = 0;
    bfh.bfReserved2 = 0;
    bfh.bfSize = sizeof(MyBITMAPFILEHEADER) + sizeof(MyBITMAPINFOHEADER) + width * height * 3;
    bfh.bfOffBits = 0x36;

    bih.biSize = sizeof(MyBITMAPINFOHEADER);
    bih.biWidth = width;
    bih.biHeight = -height;
    bih.biPlanes = 1;
    bih.biBitCount = 24;
    bih.biCompression = 0;
    bih.biSizeImage = 0;
    bih.biXPelsPerMeter = 5000;
    bih.biYPelsPerMeter = 5000;
    bih.biClrUsed = 0;
    bih.biClrImportant = 0;

    FILE* fp;//文件指针

    //打开文件（没有则创建）
    fp = fopen(name, "wb");
    if (fp == NULL)
    {
        printf("file is null");
        return;
    }

    fwrite(&bfType, sizeof(bfType), 1, fp);
    fwrite(&bfh, sizeof(bfh), 1, fp);
    fwrite(&bih, sizeof(bih), 1, fp);

    fwrite(buffer, width * height * 3, 1, fp);

    fclose(fp);
}