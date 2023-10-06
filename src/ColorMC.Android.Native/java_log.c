#include "log.h"
#include "java_log.h"

#include <stdlib.h>
#include <stdbool.h>

static log log_handel;

static int pfd[2];

static void set_log_handel(log* handel)
{
    log_handel = handel;
    LOGI("Log handel set");
}

static void start_java_log()
{
    setvbuf(stdout, 0, _IOLBF, 0); // make stdout line-buffered
    setvbuf(stderr, 0, _IONBF, 0); // make stderr unbuffered

    /* create the pipe and redirect stdout and stderr */
    pipe(pfd);
    dup2(pfd[1], 1);
    dup2(pfd[1], 2);
}

static void java_log_read()
{
    char buf[4096];
    ssize_t  rsize;
    while ((rsize = read(pfd[0], buf, sizeof(buf) - 1)) > 0)
    {
        if (buf[rsize - 1] == '\n') {
            rsize = rsize - 1; //truncate
        }
        buf[rsize] = 0x00;
        if (log_handel)
        {
            log_handel(buf, rsize);
        }
    }
}