using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Avalonia.Android;
using ColorMC.Core;
using ColorMC.Gui;
using System.Collections.Generic;

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
        ColorMCCore.PhoneGameLaunch = Start;
        ColorMCGui.StartPhone(GetExternalFilesDir(null).AbsolutePath);
        base.OnCreate(savedInstanceState);
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
