using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.Core.Content;
using Avalonia.Android;
using ColorMC.Android.Resources;
using ColorMC.Core;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui;
using Net.Kdt.Pojavlaunch;
using Net.Kdt.Pojavlaunch.Multirt;
using Net.Kdt.Pojavlaunch.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Uri = Android.Net.Uri;

namespace ColorMC.Android;

[Activity(Label = "ColorMC",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    ScreenOrientation = ScreenOrientation.FullUser)]
public class MainActivity : AvaloniaMainActivity<App>
{
    private readonly Semaphore _semaphore = new(0, 2);
    private bool _runData;

    protected override void OnDestroy()
    {
        base.OnDestroy();

        App.Close();
    }

    protected override void AttachBaseContext(Context? context)
    {
        base.AttachBaseContext(LocaleUtils.SetLocale(context));
    }

    protected override void OnCreate(Bundle savedInstanceState)
    {
        ColorMCCore.PhoneGameLaunch = Start;
        ColorMCCore.PhoneJvmInstall = PhoneJvmInstall;
        ColorMCCore.PhoneReadJvm = PhoneReadJvm;
        ColorMCCore.PhoneReadFile = PhoneReadFile;
        ColorMCCore.PhoneGetDataDir = PhoneGetDataDir;
        ColorMCCore.PhoneJvmRun = PhoneJvmRun;
        ColorMCCore.PhoneOpenUrl = PhoneOpenUrl;
        ColorMCGui.PhoneOpenSetting = Setting;
        ColorMCGui.StartPhone(GetExternalFilesDir(null).AbsolutePath + "/");

        base.OnCreate(savedInstanceState);

        Tools.AppName = "ColorMC";

        if ((int)Build.VERSION.SdkInt >= 23 && (int)Build.VERSION.SdkInt < 29 && !IsStorageAllowed()) RequestStoragePermission();

        PojavApplication.Init(this);

        BackRequested += MainActivity_BackRequested;
    }

    private void MainActivity_BackRequested(object? sender, AndroidBackRequestedEventArgs e)
    {
        App.AllWindow?.Back();
        e.Handled = true;
    }

    public async Task<bool> PhoneJvmRun(string path, string dir, List<string> arg)
    {
        var mainIntent = new Intent();
        mainIntent.SetAction("ColorMC.Minecraft.JvmRun");
        mainIntent.PutExtra("JAVA_DIR", path);
        mainIntent.PutExtra("JAVA_ARG", arg.ToArray());
        mainIntent.PutExtra("GAME_DIR", dir);
        StartActivityForResult(mainIntent, 400);
        await Task.Run(() =>
        {
            _semaphore.WaitOne();
        });

        return _runData;
    }

    public void PhoneJvmInstall(string path, string file)
    {
        var stream = ContentResolver?.OpenInputStream(Uri.Parse(file));
        MultiRTUtils.InstallRuntimeNamed(path, stream);
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

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode == 400 && resultCode == Result.Ok)
        {
            _runData = data?.GetBooleanExtra("res", false) ?? false;
            _semaphore.Release();
        }
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
        if (info == null)
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

    public void PhoneOpenUrl(string url)
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
    }
}
