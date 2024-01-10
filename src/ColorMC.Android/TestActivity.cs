using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using ColorMC.Android.GLRender;

namespace ColorMC.Android;

[Activity(Label = "TestActivity",
    //MainLauncher = true,
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon")]
public class TestActivity : Activity, View.IOnClickListener
{
    private EditText width, height;
    private GLSurface view;
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        var panel = FindViewById<RelativeLayout>(Resource.Id.surface_view);
        width = FindViewById<EditText>(Resource.Id.width_setting);
        height = FindViewById<EditText>(Resource.Id.height_setting);

        var button = FindViewById<Button>(Resource.Id.setting);

        button.SetOnClickListener(this);

        view = new GLSurface(ApplicationContext);
        panel.AddView(view);
    }

    public void OnClick(View? v)
    {
        view.SetSize(width.Text, height.Text);
    }
}