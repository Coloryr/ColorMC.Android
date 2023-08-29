using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Avalonia.Android;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Gui;
using Esprima.Ast;
using Java.Lang;
using Java.Security;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Environment = Android.OS.Environment;
using Permission = Android.Content.PM.Permission;

namespace ColorMC.Android;

[Activity(Label = "ColorMC",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode,
    ScreenOrientation = ScreenOrientation.FullSensor)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        ColorMCGui.StartPhone("/storage/emulated/0/ColorMC/");
        RequestPermission();
        base.OnCreate(savedInstanceState);
    }

    private void Start()
    {
        ColorMCCore.PhoneGameLaunch = Start;
        ColorMCGui.PhoneOk();
    }

    private void RequestPermission()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            // 先判断有没有权限
            if (Environment.IsExternalStorageManager)
            {
                Start();
            }
            else
            {
                var intent = new Intent("android.settings.MANAGE_APP_ALL_FILES_ACCESS_PERMISSION");
                intent.SetData(Uri.Parse("package:" + ApplicationContext.PackageName));
                StartActivityForResult(intent, 1);
            }
        }
        else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        {
            // 先判断有没有权限
            if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == Permission.Granted)
            {
                Start();
            }
            else
            {
                ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage }, 1);
            }
        }
        else
        {
            Start();
        }
    }

    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        if (requestCode == 1)
        {
            if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) == Permission.Granted &&
                    ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) == Permission.Granted)
            {
                Start();
            }
            else
            {
                Toast.MakeText(BaseContext, "存储权限获取失败", ToastLength.Short);
            }
        }
    }

    protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);

        if (requestCode == 1 && Build.VERSION.SdkInt >= BuildVersionCodes.R)
        {
            if (Environment.IsExternalStorageManager)
            {
                Start();
            }
            else
            {
                Toast.MakeText(BaseContext, "存储权限获取失败", ToastLength.Short);
            }
        }
    }

    public void OpenUrl(string url)
    {
        Uri uri = Uri.Parse(url);
        StartActivity(new Intent(Intent.ActionView, uri));
    }

    public void Start(List<string> list)
    {
        int i = 0;
        var mainIntent = new Intent();
        mainIntent.SetAction("ColorMC.Minecraft");
        mainIntent.PutExtra("GAME_DIR", list[i++]);
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
