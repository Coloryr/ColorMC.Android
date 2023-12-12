using Android.App;
using Android.OS;
using Android.Views;
using ColorMC.Android.GLRender;

namespace ColorMC.Android;

[Activity(Label = "TestActivity", 
    MainLauncher = true, 
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon")]
public class TestActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var view = new GLSurface(ApplicationContext);
        SetContentView(view);
    }
}