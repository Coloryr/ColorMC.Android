using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Systems;
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
using Path = System.IO.Path;
using Process = System.Diagnostics.Process;
using Uri = Android.Net.Uri;

namespace ColorMC.Android.UI.Activity;

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

    public Process PhoneStartJvm(string file)
    {
        var info = new ProcessStartInfo(NativeLibDir + "/libcolormcnative.so");
        var path = Path.GetFullPath(new FileInfo(file).Directory.Parent.FullName);

        var path1 = JavaUnpack.GetLibPath(path);

        var temp1 = Os.Getenv("PATH");

        var LD_LIBRARY_PATH = $"{path1}/{(File.Exists($"{path1}/server/libjvm.so") ? "server" : "client")}"
            + $":{path}:{path1}/jli:{path1}:"
            + "/system/lib64:/vendor/lib64:/vendor/lib64/hw:"
            + NativeLibDir;

        info.Environment.Add("LD_LIBRARY_PATH", LD_LIBRARY_PATH);
        info.Environment.Add("PATH", path1 + "/bin:" + temp1);
        info.Environment.Add("JAVA_HOME", path);
        info.Environment.Add("NATIVE_DIR", NativeLibDir);
        info.Environment.Add("HOME", ColorMCCore.BaseDir);
        info.Environment.Add("TMPDIR", ApplicationContext.CacheDir.AbsolutePath);
        info.ArgumentList.Add("-Djava.home=" + path);

        var p = new Process
        {
            StartInfo = info,
            EnableRaisingEvents = true
        };
        return p;
    }

    public Process PhoneJvmRun(GameSettingObj obj, JavaInfo jvm, string dir, List<string> arg, Dictionary<string, string> env)
    {
        var p = PhoneStartJvm(jvm.Path);

        foreach (var item in env)
        {
            p.StartInfo.Environment.Add(item.Key, item.Value);
        }

        p.StartInfo.WorkingDirectory = dir;
        p.StartInfo.ArgumentList.Add("-Djava.io.tmpdir=" + ApplicationContext.CacheDir.AbsolutePath);
        p.StartInfo.ArgumentList.Add("-Djna.boot.library.path=" + NativeLibDir);
        p.StartInfo.ArgumentList.Add("-Duser.home=" + ApplicationContext.GetExternalFilesDir(null).AbsolutePath);
        p.StartInfo.ArgumentList.Add("-Duser.language=" + Java.Lang.JavaSystem.GetProperty("user.language"));
        p.StartInfo.ArgumentList.Add("-Dos.name=Linux");
        p.StartInfo.ArgumentList.Add("-Dos.version=Android-" + ColorMCCore.Version);
        p.StartInfo.ArgumentList.Add("-Duser.timezone=" + Java.Util.TimeZone.Default.ID);
        arg.ForEach(p.StartInfo.ArgumentList.Add);

        return p;
    }

    public void PhoneJvmInstall(Stream stream, string file, ColorMCCore.ZipUpdate? zip)
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

    protected override void OnActivityResult(int requestCode,
        [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
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
        //var dir = obj.GetConfigPath();
        //Directory.CreateDirectory(dir);
        //var file = dir + "splash.properties";
        //string data = PathHelper.ReadText(file) ?? "enabled=true";
        //if (data.Contains("enabled=true"))
        //{
        //    PathHelper.WriteText(file, data.Replace("enabled=true", "enabled=false"));
        //}
    }

    public void Setting()
    {
        //var mainIntent = new Intent();
        //mainIntent.SetAction("ColorMC.Minecraft.Setting");
        //StartActivity(mainIntent);
    }

    public Process PhoneGameLaunch(GameSettingObj obj, JavaInfo jvm, List<string> list,
        Dictionary<string, string> env)
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

        var render = GameRender.RenderType.gl4es;

        var list1 = new List<string>();
        var display = AndroidHelper.GetDisplayMetrics(this);
        list1.Add("-Dorg.lwjgl.vulkan.libname=libvulkan.so");
        list1.Add("-Dglfwstub.initEgl=false");
        list1.Add("-Dlog4j2.formatMsgNoLookups=true");
        list1.Add("-Dfml.earlyprogresswindow=false");
        list1.Add("-Dloader.disable_forked_guis=true");
        list1.Add($"-Dorg.lwjgl.opengl.libname={render.GetFileName()}");
        ResourceUnPack.GetCacioJavaArgs(list1, display.WidthPixels, display.HeightPixels, jvm.MajorVersion == 8);

        list1.AddRange(list);

        var p = PhoneJvmRun(obj, jvm, obj.GetGamePath(), list1, env);

        p.StartInfo.Environment.Add("glfwstub.windowWidth", $"{display.WidthPixels}");
        p.StartInfo.Environment.Add("glfwstub.windowHeight", $"{display.HeightPixels}");
        p.StartInfo.Environment.Add("ANDROID_VERSION", $"{(int)Build.VERSION.SdkInt}");
        if (CheckHelpers.IsGameVersionV2(version))
        {
            p.StartInfo.Environment.Add("GAME_V2", "1");
        }

        Bitmap bitmap;
        var image = obj.GetIconFile();
        if (File.Exists(image))
        {
            bitmap = BitmapFactory.DecodeFile(image);
        }
        else
        {
            bitmap = BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon);
        }
        var game = new GameRender(ApplicationContext.FilesDir.AbsolutePath, obj.UUID, obj.Name,
            bitmap, p, render);
        game.GameReady += Game_GameReady;

        Games.Remove(obj.UUID);
        Games.Add(obj.UUID, game);

        game.Start();

        p.OutputDataReceived += P_OutputDataReceived;
        p.ErrorDataReceived += P_ErrorDataReceived;

        p.Exited += (a, b) =>
        {
            game.Close();
            Games.Remove(obj.UUID);
        };

        return p;
    }

    private void P_ErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        RenderLog.Error("Pipe", e.Data ?? "null");
    }

    private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        RenderLog.Info("Pipe", e.Data ?? "null");
    }

    private void Game_GameReady(string uuid)
    {
        AndroidHelper.Main.Post(() =>
        {
            var intent = new Intent(this, typeof(GameActivity));
            intent.PutExtra("GAME_UUID", uuid);
            intent.AddFlags(ActivityFlags.SingleTop);
            StartActivity(intent);
        });
    }
}
