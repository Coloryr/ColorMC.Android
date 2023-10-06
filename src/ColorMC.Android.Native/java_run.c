/*
 * Copyright (c) 2013, Oracle and/or its affiliates. All rights reserved.
 * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * This code is free software; you can redistribute it and/or modify it
 * under the terms of the GNU General Public License version 2 only, as
 * published by the Free Software Foundation.  Oracle designates this
 * particular file as subject to the "Classpath" exception as provided
 * by Oracle in the LICENSE file that accompanied this code.
 *
 * This code is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
 * version 2 for more details (a copy is included in the LICENSE file that
 * accompanied this code).
 *
 * You should have received a copy of the GNU General Public License version
 * 2 along with this work; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 *
 * Please contact Oracle, 500 Oracle Parkway, Redwood Shores, CA 94065 USA
 * or visit www.oracle.com if you need additional information or have any
 * questions.
 */
#include <android/log.h>
#include <dlfcn.h>
#include <errno.h>
#include <jni.h>
#include <stdio.h>
#include <stdlib.h>
#include <pthread.h>
#include <unistd.h>
#include <stdbool.h>
 // Boardwalk: missing include
#include <string.h>

#include "log.h"

// Uncomment to try redirect signal handling to JVM
// #define TRY_SIG2JVM

static struct sigaction old_sa[NSIG];

void (*__old_sa)(int signal, siginfo_t* info, void* reserved);
int (*JVM_handle_linux_signal)(int signo, siginfo_t* siginfo, void* ucontext, int abort_if_unrecognized);

void android_sigaction(int signal, siginfo_t* info, void* reserved) {
    printf("process killed with signal %d code %p addr %p\n", signal, info->si_code, info->si_addr);
    if (JVM_handle_linux_signal == NULL) { // should not happen, but still
        __old_sa = old_sa[signal].sa_sigaction;
        __old_sa(signal, info, reserved);
        exit(1);
    }
    else {
        int orig_errno = errno;  // Preserve errno value over signal handler.
        JVM_handle_linux_signal(signal, info, reserved, true);
        errno = orig_errno;
    }
}

void java_run_init() {
#ifdef TRY_SIG2JVM
    void* libjvm = dlopen("libjvm.so", RTLD_LAZY | RTLD_GLOBAL);
    if (NULL == libjvm) {
        LOGE("JVM lib = NULL: %s", dlerror());
        return -1;
    }
    JVM_handle_linux_signal = dlsym(libjvm, "JVM_handle_linux_signal");
#endif

    jint res = 0;
    // int i;
    //Prepare the signal trapper
    struct sigaction catcher;
    memset(&catcher, 0, sizeof(sigaction));
    catcher.sa_sigaction = android_sigaction;
    catcher.sa_flags = SA_SIGINFO | SA_RESTART;
    // SA_RESETHAND;
#define CATCHSIG(X) sigaction(X, &catcher, &old_sa[X])
    CATCHSIG(SIGILL);
    //CATCHSIG(SIGABRT);
    CATCHSIG(SIGBUS);
    CATCHSIG(SIGFPE);
#ifdef TRY_SIG2JVM
    CATCHSIG(SIGSEGV);
#endif
    CATCHSIG(SIGSTKFLT);
    CATCHSIG(SIGPIPE);
    CATCHSIG(SIGXFSZ);
    //Signal trapper ready
}
