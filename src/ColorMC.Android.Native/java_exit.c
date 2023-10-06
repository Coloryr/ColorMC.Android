#include "java_exit.h"
#include "xhook/xhook.h"

#include <stdlib.h>

static exit1 exit_handel;
static exit2 exit1_handel;

void java_set_exit_handel(exit1 handel, exit2 handel1)
{
	exit_handel = handel;
}

void java_on_exit_init()
{
	xhook_enable_debug(0);
	xhook_register(".*\\.so$", "exit", exit_handel, NULL);
	xhook_refresh(1);
	atexit(exit1_handel);
}