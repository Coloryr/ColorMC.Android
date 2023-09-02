using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Util;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Avalonia.Android;
using ColorMC.Android.Resources;
using ColorMC.Core;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui;
using Esprima.Ast;
using Java.Lang;
using Java.Net;
using Net.Kdt.Pojavview;
using Net.Kdt.Pojavview.Multirt;
using Net.Kdt.Pojavview.Tasks;
using Net.Kdt.Pojavview.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using static Java.Lang.Thread;
using Environment = Android.OS.Environment;
using StringBuilder = System.Text.StringBuilder;
using Uri = Android.Net.Uri;

namespace ColorMC.Android;

[Activity(Label = "ColorMC",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    ScreenOrientation = ScreenOrientation.FullSensor)]
public class MainActivity : AvaloniaMainActivity<App>, IUncaughtExceptionHandler
{
    protected override void AttachBaseContext(Context? context)
    {
        base.AttachBaseContext(LocaleUtils.SetLocale(context));
    }

    public void UncaughtException(Thread t, Throwable e)
    {
        string file = GetExternalFilesDir(null).AbsolutePath + "/" + "latestcrash.txt";
        try
        {
            var crashStream = new StringBuilder();
            crashStream.Append("PojavLauncher crash report\n");
            crashStream.Append(" - Time: ").Append(DateTime.Now.ToString()).Append("\n");
            crashStream.Append(" - Device: ").Append(Build.Product).Append(" ").Append(Build.Model).Append("\n");
            crashStream.Append(" - Android version: ").Append(Build.VERSION.Release).Append("\n");
            crashStream.Append(" - Crash stack trace:\n");
            //crashStream.append(" - Launcher version: " + BuildConfig.VERSION_NAME + "\n");
            crashStream.Append(Log.GetStackTraceString(e));
            File.WriteAllText(file, crashStream.ToString());
        }
        catch (Throwable throwable)
        {
            Log.Error("ColorMC_Crash", " - Exception attempt saving crash stack trace:", throwable);
            Log.Error("ColorMC_Crash", " - The crash stack trace was:", e);
        }
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        DefaultUncaughtExceptionHandler = this;

        ColorMCCore.PhoneGameLaunch = Start;
        ColorMCCore.PhoneJvmIntasll = PhoneJvmIntasll;
        ColorMCCore.PhoneReadJvm = PhoneReadJvm;
        ColorMCCore.PhoneReadFile = PhoneReadFile;
        ColorMCGui.PhoneOpenSetting = Setting;
        ColorMCGui.StartPhone(GetExternalFilesDir(null).AbsolutePath + "/");
        base.OnCreate(savedInstanceState);

        Tools.AppName = "ColorMC";

        if((int)Build.VERSION.SdkInt >= 23 && (int)Build.VERSION.SdkInt < 29 && !IsStorageAllowed()) RequestStoragePermission();
        
        PojavApplication.Init(this);
    }

    public Stream? PhoneReadFile(string file)
    {
        return ContentResolver?.OpenInputStream(Uri.Parse(file));
    }

    public bool IsStorageAllowed()
    {
        //Getting the permission status
        Permission result1 = ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage);
        Permission result2 = ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage);

        //If permission is granted returning true
        return result1 == Permission.Granted &&
                result2 == Permission.Granted;
    }

    private void RequestStoragePermission()
    {
        RequestPermissions(new string[]{ Manifest.Permission.WriteExternalStorage,
                Manifest.Permission.ReadExternalStorage }, 1);
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == 1)
        {
            if (!IsStorageAllowed())
            {
                Toast.MakeText(this, "需要权限才能运行", ToastLength.Long).Show();
                RequestStoragePermission();
            }
        }
    }

    public JavaInfo? PhoneReadJvm(string path)
    {
        var file = new FileInfo(path);
        path = file.Directory.Parent.FullName;
        var info = MultiRTUtils.Read(path);
        if(info == null)
        {
            return null;
        }

        return new()
        {
            Path = path,
            MajorVersion = info.JavaVersion,
            Type = "openjdk",
            Version = info.VersionString!,
            Arch = info.Arch switch 
            {
                "aarch64" => ArchEnum.aarch64,
                "arm" => ArchEnum.armV7,
                "x86" => ArchEnum.x32,
                "x86_64" => ArchEnum.x64,
                _ => ArchEnum.unknow
            }
        };
    }

    public void PhoneJvmIntasll(string path, string name)
    {
        MultiRTUtils.InstallRuntimeNamed(path);
    }

    public void OpenUrl(string url)
    {
        Uri uri = Uri.Parse(url);
        StartActivity(new Intent(Intent.ActionView, uri));
    }

    public void Setting()
    {
        var mainIntent = new Intent();
        mainIntent.SetAction("ColorMC.Minecraft.Setting");
        StartActivity(mainIntent);
    }

    public void Start(GameSettingObj obj, List<string> list)
    {
        var file = obj.GetOptionsFile();
        if (!File.Exists(file))
        {
            File.WriteAllText(file, Resource1.options);
        }
        int i = 0;
        var mainIntent = new Intent();
        mainIntent.SetAction("ColorMC.Minecraft.Launch");
        mainIntent.PutExtra("GAME_DIR", list[i++]);
        mainIntent.PutExtra("JAVA_DIR", list[i++]);
        mainIntent.PutExtra("GAME_VERSION", list[i++]);
        mainIntent.PutExtra("JVM_VERSION", list[i++]);
        mainIntent.PutExtra("GAME_TIME", list[i++]);
        mainIntent.PutExtra("GAME_V2", list[i++] == "true");
        var jvmarg = int.Parse(list[i++]);
        var list1 = new List<string>();
        for (int a = 0; a < jvmarg; a++)
        {
            list1.Add(list[i++]);
        }
        mainIntent.PutExtra("JVM_ARGS", list1.ToArray());
        mainIntent.PutExtra("CLASSPATH", list[i++]);
        mainIntent.PutExtra("MAINCLASS", list[i++]);
        var gamearg = int.Parse(list[i++]);
        var list2 = new List<string>();
        for (int a = 0; a < gamearg; a++)
        {
            list2.Add(list[i++]);
        }
        mainIntent.PutExtra("GAME_ARGS", list2.ToArray());

        mainIntent.AddFlags(ActivityFlags.SingleTop);
        mainIntent.AddFlags(ActivityFlags.NewTask);
        StartActivity(mainIntent);
        //Finish();
    }
}
