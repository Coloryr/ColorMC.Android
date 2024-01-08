#include <stdio.h>

#include "gl_loader/gl_bridge.h"

#define EXTERNAL_API __attribute__((used))

EXTERNAL_API int run(int argc, char** args)
{
	printf("run start");
	if (gl_init() == false)
	{
		printf("gl init fail\n");
	}

	printf("run exit");
	return 0;
}