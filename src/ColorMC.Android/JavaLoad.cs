using Android.Content;
using Android.Systems;
using Android.Util;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using Java.Lang;
using Java.Util;
using Net.Kdt.Pojavlaunch;
using Net.Kdt.Pojavlaunch.Extra;
using Net.Kdt.Pojavlaunch.Prefs;
using Org.Lwjgl.Glfw;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Android.Icu.Text.IDNA;
using Console = System.Console;

namespace ColorMC.Android.Lib;

public static class JavaLoad
{
    public const int UNSUPPORTED_ARCH = -1;

    public static string JavaHomeLib;
    public static string JavaLibPath;
    public static string JavaLDPath;
    public static string NativeLibPath;
    
    public static void Run(GameSettingObj obj, JavaInfo info, List<string> args)
    {
        string dir = obj.GetLogPath();
        string log = Path.GetFullPath(dir + "/" + "phone.log");
        
        JavaLog.Start(log);

        InitLibDir(info);
        SetJavaEnvironment(obj, info);
        LoadLib(info);
        JavaExit.GameExit += JavaExit_GameExit;
        JavaExit.Start();
        var args1 = new List<string>() { "java" };
        args1.AddRange(GenBaseArg(obj, info));

        string render = GameRender.LoadGraphicsLibrary();


        args1.AddRange(args);
        var res = NativeHook.JavaRunInit(obj.GetGamePath(), args1.ToArray(), args1.Count);
    }

    private static void JavaExit_GameExit(int obj)
    {
        Console.WriteLine($"GameExit: {obj}");
    }

    public static JavaInfo? ReadJava(string path)
    {
        var release = new FileInfo(path + "/release");
        if (!release.Exists)
        {
            return null;
        }
        try
        {
            var content = PathHelper.ReadText(release.FullName)!;
            var javaVersion = StringHelper.GetString(content, "JAVA_VERSION=\"", "\"");
            var osArch = StringHelper.GetString(content, "OS_ARCH=\"", "\"");
            if (javaVersion != null && osArch != null)
            {
                string[] javaVersionSplit = javaVersion.Split(".");
                int javaVersionInt;
                if (javaVersionSplit[0] == "1")
                {
                    javaVersionInt = int.Parse(javaVersionSplit[1]);
                }
                else
                {
                    javaVersionInt = int.Parse(javaVersionSplit[0]);
                }
                return new()
                {
                    Path = path,
                    MajorVersion = javaVersionInt,
                    Type = "openjdk",
                    Version = javaVersion,
                    Arch = osArch switch
                    {
                        "aarch64" => ArchEnum.aarch64,
                        "arm" => ArchEnum.arm,
                        "x86" => ArchEnum.x86,
                        "x86_64" => ArchEnum.x86_64,
                        _ => ArchEnum.unknow
                    }
                };
            }
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
        }
        return null;
    }

    public static List<string> GenBaseArg(GameSettingObj obj, JavaInfo info)
    {
        string resolvFile = Path.GetFullPath("resolv.conf");
        return new List<string>()
        {
            "-Djava.home=" + info.Path,
            "-Djava.io.tmpdir=" + obj.GetGameTempPath(),
            "-Djna.boot.library.path=" + NativeLibPath,
            "-Duser.home=" + obj.GetGamePath(),
            "-Duser.language=zh-cn",
            "-Dos.name=Linux",
            "-Dos.version=Android-13",
            "-Duser.timezone=" + Java.Util.TimeZone.Default?.ID,

            "-Dorg.lwjgl.vulkan.libname=libvulkan.so",
            //LWJGL 3 DEBUG FLAGS
            //"-Dorg.lwjgl.util.Debug=true",
            //"-Dorg.lwjgl.util.DebugFunctions=true",
            //"-Dorg.lwjgl.util.DebugLoader=true",

            // GLFW Stub width height
            //"-Dglfwstub.windowWidth=" + Tools.getDisplayFriendlyRes(currentDisplayMetrics.widthPixels, LauncherPreferences.PREF_SCALE_FACTOR / 100F),
            //"-Dglfwstub.windowHeight=" + Tools.getDisplayFriendlyRes(currentDisplayMetrics.heightPixels, LauncherPreferences.PREF_SCALE_FACTOR / 100F),
            //"-Dglfwstub.initEgl=false",
            "-Dext.net.resolvPath=" + resolvFile,
            "-Dlog4j2.formatMsgNoLookups=true", //Log4j RCE mitigation

            "-Dnet.minecraft.clientmodname=ColorMC",
            "-Dfml.earlyprogresswindow=false", //Forge 1.14+ workaround
            "-Dloader.disable_forked_guis=true"
        };
    }

    public static void LoadLib(JavaInfo info)
    {
        NativeHook.LoadNative(FindFromLdPath("libjli.so"));
        if (!NativeHook.LoadNative("libjvm.so"))
        {
            Log.Warn("DynamicLoader", "Failed to load with no path, trying with full path");
            NativeHook.LoadNative(JavaLibPath + "/libjvm.so");
        }
        NativeHook.LoadNative(FindFromLdPath("libverify.so"));
        NativeHook.LoadNative(FindFromLdPath("libjava.so"));
        // dlopen(findInLdLibPath("libjsig.so"));
        NativeHook.LoadNative(FindFromLdPath("libnet.so"));
        NativeHook.LoadNative(FindFromLdPath("libnio.so"));
        NativeHook.LoadNative(FindFromLdPath("libawt.so"));
        NativeHook.LoadNative(FindFromLdPath("libawt_headless.so"));
        NativeHook.LoadNative(FindFromLdPath("libfreetype.so"));
        NativeHook.LoadNative(FindFromLdPath("libfontmanager.so"));
        foreach (var file in PathHelper.GetAllFile(info.Path + "/" + JavaHomeLib))
        {
            if (file.Extension == ".so")
            {
                NativeHook.LoadNative(file.FullName);
            }
        }
        NativeHook.LoadNative(NativeLibPath + "/libopenal.so");
    }

    public static string FindFromLdPath(string name)
    {
        foreach (string path in JavaLDPath.Split(':'))
        {
            var path1 = Path.GetFullPath(path + "/" + name);
            if (File.Exists(path1))
            {
                return path1;
            }
        }

        return name;
    }

    public static void InitLibDir(JavaInfo java)
    {
        string arch;
        if (java.Arch == ArchEnum.x86)
        {
            arch = "i386/i486/i586";
        }
        else
        {
            arch = java.Arch.ToString();
        }

        JavaHomeLib = "lib";

        foreach (string arch1 in arch.Split("/"))
        {
            var dir = new DirectoryInfo(java.Path + "/lib/" + arch1);
            if (dir.Exists)
            {
                JavaHomeLib = "lib/" + arch1;
                break;
            }
        }

        string libName = SystemInfo.Is64Bit ? "lib64" : "lib";

        JavaLDPath = java.Path +
                "/" + JavaHomeLib +
                "/jli:" + java.Path + "/" + JavaHomeLib +
                ":" +
                "/system/" + libName + ":" +
                "/vendor/" + libName + ":" +
                "/vendor/" + libName + "/hw:" +
                NativeLibPath;

        var serverFile = Path.GetFullPath(java.Path + "/" + JavaHomeLib + "/server/libjvm.so");
        JavaLibPath = java.Path + "/" + JavaHomeLib + "/" + (File.Exists(serverFile) ? "server" : "client");
        Log.Debug("DynamicLoader", "Base LD_LIBRARY_PATH: " + JavaLDPath);
        Log.Debug("DynamicLoader", "Internal LD_LIBRARY_PATH: " + JavaLibPath + ":" + JavaLDPath);
        NativeHook.SetNativeLd(JavaLibPath + ":" + JavaLDPath);
    }

    public static void SetJavaEnvironment(GameSettingObj obj, JavaInfo java)
    {
        var envs = new Dictionary<string, string>
        {
            { "GAME_NATIVEDIR", NativeLibPath },
            { "JAVA_HOME", java.Path },
            { "HOME", obj.GetBasePath() },
            { "TMPDIR", obj.GetGameTempPath() },
            { "LIBGL_MIPMAP", "3" },

            // Prevent OptiFine (and other error-reporting stuff in Minecraft) from balooning the log
            { "LIBGL_NOERROR", "1" },

            // On certain GLES drivers, overloading default functions shader hack fails, so disable it
            { "LIBGL_NOINTOVLHACK", "1" },

            // Fix white color on banner and sheep, since GL4ES 1.1.5
            { "LIBGL_NORMALIZE", "1" },

            { "FORCE_VSYNC", "false" },
            { "MESA_GLSL_CACHE_DIR", obj.GetGameCachePath() },
            { "force_glsl_extensions_warn", "true" },
            { "allow_higher_compat_version", "true" },
            { "allow_glsl_extension_directive_midshader", "true" },
            { "MESA_LOADER_DRIVER_OVERRIDE", "zink" },
            { "VTEST_SOCKET_NAME", obj.GetGameCachePath() + "/.virgl_test" },
            { "LD_LIBRARY_PATH", JavaLDPath },
            { "PATH", java.Path + "/bin:" + Os.Getenv("PATH") },
            { "REGAL_GL_VENDOR", "Android" },
            { "REGAL_GL_RENDERER", "Regal" },
            { "REGAL_GL_VERSION", "4.5" }
        };

        foreach (var item in envs)
        {
            JavaLog.Log("Added custom env: " + item.Key + "=" + item.Value);
            try
            {
                Os.Setenv(item.Key, item.Value, true);
            }
            catch (System.Exception ex)
            {
                Log.Error("JavaLoad", ex.ToString());
            }
        }

        foreach (var item in GameRender.GetRenderEnv())
        {
            JavaLog.Log("Added custom env: " + item.Key + "=" + item.Value);
            try
            {
                Os.Setenv(item.Key, item.Value, true);
            }
            catch (System.Exception ex)
            {
                Log.Error("JavaLoad", ex.ToString());
            }
        }
    }
}
