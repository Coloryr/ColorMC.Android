using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Systems;
using Android.Util;
using Avalonia.Android;
using Avalonia.Controls;
using ColorMC.Android.components;
using ColorMC.Android.GLRender;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui;
using ColorMC.Gui.Objs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Process = System.Diagnostics.Process;
using Uri = Android.Net.Uri;

namespace ColorMC.Android;

[Activity(Label = "ColorMC",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    TaskAffinity = "colormc.android.game.main",
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    ScreenOrientation = ScreenOrientation.FullUser)]
public class MainActivity : AvaloniaMainActivity<App>
{
    public static readonly Dictionary<string, GameRender> Games = [];

    public static string NativeLibDir;

    protected override void OnDestroy()
    {
        base.OnDestroy();

        App.Close();
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        ColorMCCore.PhoneGameLaunch = PhoneGameLaunch;
        ColorMCCore.PhoneJvmInstall = PhoneJvmInstall;
        ColorMCCore.PhoneStartJvm = PhoneStartJvm;
        ColorMCCore.PhoneReadFile = PhoneReadFile;
        ColorMCCore.PhoneGetDataDir = PhoneGetDataDir;
        ColorMCCore.PhoneJvmRun = PhoneJvmRun;
        ColorMCCore.PhoneOpenUrl = PhoneOpenUrl;

        ColorMCGui.PhoneGetSetting = PhoneGetSetting;
        ColorMCGui.PhoneGetFrp = PhoneGetFrp;

        ColorMCGui.StartPhone(GetExternalFilesDir(null)!.AbsolutePath + "/");
        PhoneConfigUtils.Init(ColorMCCore.BaseDir);

        NativeLibDir = ApplicationInfo.NativeLibraryDir;

        base.OnCreate(savedInstanceState);

        ResourceUnPack.StartUnPack(this);

        BackRequested += MainActivity_BackRequested;
    }

    private string PhoneGetFrp(FrpType type)
    {
        if (type == FrpType.OpenFrp)
        {
            return ApplicationInfo!.NativeLibraryDir + "/" + "libfrpc_openfrp.so";
        }
        return ApplicationInfo!.NativeLibraryDir + "/" + "libfrpc.so";
    }

    private void MainActivity_BackRequested(object? sender, AndroidBackRequestedEventArgs e)
    {
        if (App.AllWindow is { } window)
        {
            window.Model.BackClick();
            e.Handled = true;
        }
    }

    public Control PhoneGetSetting()
    {
        return new PhoneControl(this);
    }

    public Process PhoneJvmRun(GameSettingObj obj, JavaInfo jvm, string dir, List<string> arg, Dictionary<string, string> env)
    {
        var p = PhoneStartJvm(jvm.Path);
        string dir1 = obj.GetLogPath();
        if (!Directory.Exists(dir1))
        {
            Directory.CreateDirectory(dir1);
        }

        foreach (var item in env)
        {
            p.StartInfo.Environment.Add(item.Key, item.Value);
        }

        var file = new FileInfo(jvm.Path);
        var path = Path.GetFullPath(file.Directory.Parent.FullName);

        p.StartInfo.WorkingDirectory = dir;
        p.StartInfo.ArgumentList.Add("-Djava.home=" + path);
        p.StartInfo.ArgumentList.Add("-Djava.io.tmpdir=" + ApplicationContext.CacheDir.AbsolutePath);
        p.StartInfo.ArgumentList.Add("-Djna.boot.library.path=" + ApplicationInfo.NativeLibraryDir);
        p.StartInfo.ArgumentList.Add("-Duser.home=" + ApplicationContext.GetExternalFilesDir(null).AbsolutePath);
        p.StartInfo.ArgumentList.Add("-Duser.language=" + Java.Lang.JavaSystem.GetProperty("user.language"));
        p.StartInfo.ArgumentList.Add("-Dos.name=Linux");
        p.StartInfo.ArgumentList.Add("-Dos.version=Android-" + ColorMCCore.Version);
        p.StartInfo.ArgumentList.Add("-Duser.timezone=" + Java.Util.TimeZone.Default.ID);
        arg.ForEach(p.StartInfo.ArgumentList.Add);

        return p;
    }

    public void PhoneJvmInstall(Stream stream, string file, Action<string, int, int>? zip)
    {
        new JavaUnpack() { ZipUpdate = zip }.Unpack(stream, file);
    }

    public string PhoneGetDataDir()
    {
        return AppContext.BaseDirectory;
    }

    public Stream? PhoneReadFile(string file)
    {
        var uri = Uri.Parse(file);
        return ContentResolver?.OpenInputStream(uri);
    }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
    }

    public Process PhoneStartJvm(string path)
    {
        var file = new FileInfo(path);
        path = Path.GetFullPath(file.Directory.Parent.FullName + "/lib");

        string arch = "";

        if (Directory.Exists(path + "/amd64"))
        {
            arch = "amd64";
        }
        else if (Directory.Exists(path + "/aarch64"))
        {
            arch = "aarch64";
        }
        else if (Directory.Exists(path + "/aarch32"))
        {
            arch = "aarch32";
        }
        else if (Directory.Exists(path + "/i386"))
        {
            arch = "i386";
        }
        else if (Directory.Exists(path + "/i486"))
        {
            arch = "i486";
        }
        else if (Directory.Exists(path + "/i586"))
        {
            arch = "i586";
        }

        path += "/" + arch;

        var temp1 = Os.Getenv("PATH");

        var LD_LIBRARY_PATH = "/system/lib64:/vendor/lib64:/vendor/lib64/hw:"
            + ApplicationContext.ApplicationInfo.NativeLibraryDir
            + $":{path}:{path}/jli:"
            + temp1;

        LD_LIBRARY_PATH += $":{path}/{(File.Exists($"{path}/server/libjvm.so") ? "server" : "client")}";

        var info = new ProcessStartInfo(file.FullName);
        info.EnvironmentVariables.Add("LD_LIBRARY_PATH", LD_LIBRARY_PATH);
        var p = new Process
        {
            StartInfo = info,
            EnableRaisingEvents = true
        };
        return p;
    }

    public void PhoneOpenUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }
        Uri uri = Uri.Parse(url)!;
        StartActivity(new Intent(Intent.ActionView, uri));
    }

    /// <summary>
    /// 保持splash不开启
    /// </summary>
    /// <param name="obj">游戏实例</param>
    private void ConfigSet(GameSettingObj obj)
    {
        var dir = obj.GetConfigPath();
        Directory.CreateDirectory(dir);
        var file = dir + "splash.properties";
        string data = PathHelper.ReadText(file) ?? "enabled=true";
        if (data.Contains("enabled=true"))
        {
            PathHelper.WriteText(file, data.Replace("enabled=true", "enabled=false"));
        }
    }

    public void Setting()
    {
        var mainIntent = new Intent();
        mainIntent.SetAction("ColorMC.Minecraft.Setting");
        StartActivity(mainIntent);
    }

    public DisplayMetrics GetDisplayMetrics()
    {
        var displayMetrics = new DisplayMetrics();

        if (Build.VERSION.SdkInt >= BuildVersionCodes.N
            && (IsInMultiWindowMode || IsInPictureInPictureMode))
        {
            //For devices with free form/split screen, we need window size, not screen size.
            displayMetrics = Resources.DisplayMetrics;
        }
        else
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
            {
                Display.GetRealMetrics(displayMetrics);
            }
            else
            { // Removed the clause for devices with unofficial notch support, since it also ruins all devices with virtual nav bars before P
                WindowManager.DefaultDisplay.GetRealMetrics(displayMetrics);
            }
        }
        return displayMetrics;
    }

    public Process PhoneGameLaunch(GameSettingObj obj, JavaInfo jvm, List<string> list, Dictionary<string, string> env)
    {
        ConfigSet(obj);

        var version = VersionPath.GetVersion(obj.Version)!;
        string dir = obj.GetLogPath();
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var file = obj.GetOptionsFile();
        if (!File.Exists(file))
        {
            File.WriteAllBytes(file, Resource1.options);
        }

        var native = ApplicationInfo!.NativeLibraryDir;
        var classpath = false;
        for (int a = 0; a < list.Count; a++)
        {
            list[a] = list[a].Replace("%natives_directory%", native);
            if (list[a].StartsWith("-cp"))
            {
                classpath = true;
                continue;
            }
            if (classpath)
            {
                classpath = false;

                string lwjgl = ResourceUnPack.ComponentsDir + "/lwjgl3/lwjgl-glfw-classes.jar";

                if (PhoneConfigUtils.Config.LwjglVk)
                {
                    lwjgl += ":" + ResourceUnPack.ComponentsDir + "/lwjgl3/lwjgl-vulkan.jar" + ":"
                        + ResourceUnPack.ComponentsDir + "/lwjgl3/lwjgl-vulkan-native.jar";
                }

                list[a] = lwjgl + ":" + list[a];
            }
        }

        var list1 = new List<string>();
        var display = GetDisplayMetrics();
        list1.Add("-Dorg.lwjgl.vulkan.libname=libvulkan.so");
        list1.Add("-Dglfwstub.initEgl=false");
        list1.Add("-Dlog4j2.formatMsgNoLookups=true");
        list1.Add("-Dfml.earlyprogresswindow=false");
        list1.Add("-Dloader.disable_forked_guis=true");
        list1.Add($"-Dorg.lwjgl.opengl.libname={GameRenderType.gl4es.GetFileName()}");
        list1.AddRange(list);

        var p = PhoneJvmRun(obj, jvm, obj.GetGamePath(), list1, env);

        p.StartInfo.Environment.Add("glfwstub.windowWidth", $"{display.WidthPixels}");
        p.StartInfo.Environment.Add("glfwstub.windowHeight", $"{display.HeightPixels}");
        p.StartInfo.Environment.Add("ANDROID_VERSION", $"{(int)Build.VERSION.SdkInt}");

        var game = new GameRender(ApplicationContext!.FilesDir!.AbsolutePath, obj.UUID, p, GameRenderType.gl4es);
        game.GameReady += Game_GameReady;

        Games.Remove(obj.UUID);
        Games.Add(obj.UUID, game);

        game.Start();

        return p;
    }

    private readonly Handler Main = new Handler(Looper.MainLooper);

    private void Game_GameReady(string uuid)
    {
        Main.Post(() =>
        {
            var intent = new Intent(this, typeof(GameActivity));
            intent.PutExtra("GAME_UUID", uuid);
            intent.AddFlags(ActivityFlags.SingleTop);
            StartActivity(intent);
        });
    }
}
