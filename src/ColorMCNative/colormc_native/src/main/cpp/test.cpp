#include <stdio.h>
#include <stdlib.h>
#include <dlfcn.h>
#include <jni.h>
#include <dlfcn.h>
#include <unistd.h>
#include <string.h>
#include <pthread.h>
#include <dirent.h>

//int main(int argc, char** args)
//{
//    printf("[ColorMC Info] game load\n");
//    char * env = getenv("RUN_SO");
//    if(!env)
//    {
//        printf("[ColorMC Error] no RUN_SO\n");
//        return 1; // 返回一个错误码
//    }
//    else
//    {
//        printf("[ColorMC Info] RUN_SO: %s\n", env);
//    }
//    void * ptr = dlopen(env, RTLD_LAZY);
//    if (!ptr)
//    {
//        printf("[ColorMC Error] Error loading library: %s\n", dlerror());
//        return 1; // 返回一个错误码
//    }
//    int (*run)(int, char**) = (int (*)(int, char **))dlsym(ptr, "main");
//    if (!run)
//    {
//        printf("[ColorMC Error] Error finding 'run' function: %s\n", dlerror());
//        dlclose(ptr); // 关闭库
//        return 1; // 返回一个错误码
//    }
//    printf("[ColorMC Info] game start\n");
//    // 调用run函数
//    int result = run(argc, args);
//
//    // 关闭库
//    dlclose(ptr);
//
//    return result;
//}

#define FULL_VERSION "1.8.0-internal"
#define DOT_VERSION "1.8"

static const char* const_progname = "java";
static const char* const_launcher = "openjdk";
static const char** const_jargs = NULL;
static const char** const_appclasspath = NULL;
static const jboolean const_javaw = JNI_FALSE;
static const jboolean const_cpwildcard = JNI_TRUE;
static const jint const_ergo_class = 0; // DEFAULT_POLICY

typedef jint JLI_Launch_func(int argc, char ** argv, /* main argc, argc */
                             int jargc, const char** jargv,          /* java args */
                             int appclassc, const char** appclassv,  /* app classpath */
                             const char* fullversion,                /* full version defined */
                             const char* dotversion,                 /* dot version defined */
                             const char* pname,                      /* program name */
                             const char* lname,                      /* launcher name */
                             jboolean javaargs,                      /* JAVA_ARGS */
                             jboolean cpwildcard,                    /* classpath wildcard*/
                             jboolean javaw,                         /* windows-only javaw */
                             jint ergo                               /* ergonomics class policy */
);

const char* load_first[]={
        "libjli.so", "libjvm.so","libverify.so",
        "libjava.so","libnet.so","libnio.so",
        "libawt.so", "libawt_headless.so","libfreetype.so",
        "libfontmanager.so" };

const int num_libs = sizeof(load_first) / sizeof(load_first[0]);

bool load_so_first(char *dir_path, const char* lib_name) {
    DIR *dir;
    struct dirent *entry;

    if (!(dir = opendir(dir_path))) {
        return false;
    }

    while ((entry = readdir(dir)) != NULL) {
        if (entry->d_type == DT_DIR) {
            char path[1024];
            if (strcmp(entry->d_name, ".") == 0 || strcmp(entry->d_name, "..") == 0)
                continue;
            snprintf(path, sizeof(path), "%s/%s", dir_path, entry->d_name);
            if (!load_so_first(path, lib_name)) {
                return false;
            }
        } else {
            if (strcmp(entry->d_name, lib_name) == 0) {
                char full_path[1024];
                snprintf(full_path, sizeof(full_path), "%s/%s", dir_path, entry->d_name);
                void *handle = dlopen(full_path, RTLD_GLOBAL | RTLD_LAZY);
                if (handle) {
                    printf("Loaded: %s\n", full_path);
                } else {
                    printf("dlopen fail :%s\n", dlerror());
                    return false;
                }
                break; // Found and loaded the library, no need to continue in this directory.
            }
        }
    }
    closedir(dir);

    return true;
}

bool load_so_files(char *path) {
    DIR *dir;
    struct dirent *entry;

    if ((dir = opendir(path)) == NULL) {
        return false;
    }

    while ((entry = readdir(dir)) != NULL) {
        char fullpath[1024];
        if (entry->d_type == DT_DIR) {
            // Skip the special entries "." and ".."
            if (strcmp(entry->d_name, ".") == 0 || strcmp(entry->d_name, "..") == 0) {
                continue;
            }
            // Recursively call load_so_files with the new path
            snprintf(fullpath, sizeof(fullpath), "%s/%s", path, entry->d_name);
            if(!load_so_files(fullpath)) {
                return false;
            }
        } else if (entry->d_type == DT_REG) {
            // Check if the file has a .so extension
            const char *ext = strrchr(entry->d_name, '.');
            if (ext && strcmp(ext, ".so") == 0) {
                snprintf(fullpath, sizeof(fullpath), "%s/%s", path, entry->d_name);
                // Try to open the shared library
                void *handle = dlopen(fullpath, RTLD_GLOBAL | RTLD_LAZY);
                if (handle) {
                    printf("Loaded: %s\n", fullpath);
                } else {
                    printf("dlopen fail :%s\n", dlerror());
                    return false;
                }
            }
        }
    }

    // 关闭目录
    closedir(dir);

    return true;
}
typedef void (*android_update_LD_LIBRARY_PATH_t)(char*);

void load_dl() {
    android_update_LD_LIBRARY_PATH_t android_update_LD_LIBRARY_PATH;
    char *env = getenv("I_LD_PATH");
    void *libdl_handle = dlopen("libdl.so", RTLD_LAZY);
    void *updateLdLibPath = dlsym(libdl_handle, "android_update_LD_LIBRARY_PATH");
    if (updateLdLibPath == NULL) {
        updateLdLibPath = dlsym(libdl_handle, "__loader_android_update_LD_LIBRARY_PATH");
        if (updateLdLibPath == NULL) {
            char *dl_error_c = dlerror();
            printf("Error getting symbol android_update_LD_LIBRARY_PATH: %s", dl_error_c);
            exit(1);
        }
    }

    android_update_LD_LIBRARY_PATH = (android_update_LD_LIBRARY_PATH_t) updateLdLibPath;
    android_update_LD_LIBRARY_PATH(env);
}

int main(int argc, char** args) {
    char *dir = getenv("JAVA_HOME");
    if (dir == NULL) {
        printf("[ColorMC Error] no JAVA_HOME\n");
        return 1;
    }

    printf("[ColorMC Info] load first\n");
    dlopen("/system/lib64/libjpeg.so", RTLD_LAZY);
    for (int i = 0; i < num_libs; i++) {
        if (!load_so_first(dir, load_first[i])) {
            printf("[ColorMC Info] load first fail\n");
            return 1;
        }
    }

    printf("[ColorMC Info] load all\n");
    if (!load_so_files(dir)) {
        printf("[ColorMC Error] open dir fail\n");
        return 1;
    }

    void *libjli = dlopen("libjli.so", RTLD_LAZY | RTLD_GLOBAL);

    if (NULL == libjli) {
        printf("[ColorMC Error] JLI lib is NULL: %s\n", dlerror());
        return 1;
    }
    printf("[ColorMC Info] Found JLI lib\n");

    JLI_Launch_func *pJLI_Launch =
            (JLI_Launch_func *) dlsym(libjli, "JLI_Launch");
    if (NULL == pJLI_Launch) {
        printf("[ColorMC Error] JLI_Launch = NULL\n");
        return 1;
    }
    printf("[ColorMC Info] Calling JLI_Launch\n");
    fflush(stdout);
    return pJLI_Launch(argc, args,
                       0, NULL, // sizeof(const_jargs) / sizeof(char *), const_jargs,
                       0, NULL, // sizeof(const_appclasspath) / sizeof(char *), const_appclasspath,
                       FULL_VERSION,
                       DOT_VERSION,
                       *args, // (const_progname != NULL) ? const_progname : *margv,
                       *args, // (const_launcher != NULL) ? const_launcher : *margv,
                       (const_jargs != NULL) ? JNI_TRUE : JNI_FALSE,
                       const_cpwildcard, const_javaw, const_ergo_class);
}