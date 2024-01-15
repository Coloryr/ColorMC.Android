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

bool load_so_files(char *path) {
    DIR *dir;
    struct dirent *entry;
    void *handle;

    printf("[ColorMC Info] start open dir :%s\n", path);

    // 打开目录
    if ((dir = opendir(path)) == NULL) {
        return false;
    }

    // 读取目录内的每个文件/子目录
    while ((entry = readdir(dir)) != NULL) {
        // 构建完整的文件路径
        char fullpath[1024];
        snprintf(fullpath, sizeof(fullpath), "%s/%s", path, entry->d_name);

        // 检查文件扩展名是否为.so
        if (strstr(entry->d_name, ".so") != NULL) {
            printf("Found .so file: %s\n", fullpath);

            // 尝试打开.so文件
            handle = dlopen(fullpath, RTLD_LAZY);
            if (!handle) {
                printf("dlopen fail %s\n", dlerror());
                continue;
            }
        }
    }

    // 关闭目录
    closedir(dir);

    return true;
}

int main(int argc, char** args) {
    char *dir = getenv("JAVA_HOME");
    if (dir == NULL) {
        printf("[ColorMC Error] no JAVA_HOME\n");
        return 1;
    }

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