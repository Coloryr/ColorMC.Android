#include "log.h"
#include "java_log.h"

#include <stdlib.h>
#include <stdbool.h>

log log_handel;

static int pfd[2];

void java_log_set_handel(log handel)
{
    log_handel = handel;
    LOGI("Log handel set");
}

void java_log_start()
{
    setvbuf(stdout, 0, _IOLBF, 0); // make stdout line-buffered
    setvbuf(stderr, 0, _IONBF, 0); // make stderr unbuffered

    /* create the pipe and redirect stdout and stderr */
    pipe(pfd);
    dup2(pfd[1], 1);
    dup2(pfd[1], 2);
}

void java_log_read()
{
    char buf[4096];
    int rsize;
    while ((rsize = read(pfd[0], buf, sizeof(buf) - 1)) > 0)
    {
        if (buf[rsize - 1] == '\n') 
        {
            rsize = rsize - 1; //truncate
        }
        buf[rsize] = 0x00;
        if (log_handel)
        {
            log_handel(buf, rsize);
        }
    }
}