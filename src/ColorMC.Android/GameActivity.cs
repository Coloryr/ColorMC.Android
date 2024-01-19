using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using ColorMC.Android.GLRender;
using ColorMC.Android.UI;
using System;

namespace ColorMC.Android;

[Activity(Label = "GameActivity",
    Theme = "@style/MyTheme.NoActionBar",
    TaskAffinity = "colormc.android.game.render",
    ScreenOrientation = ScreenOrientation.SensorLandscape,
    Icon = "@drawable/icon")]
public class GameActivity : AppCompatActivity
{
    private Button button;
    private GLSurface view;
    private bool IsEdit;
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        var panel = FindViewById<RelativeLayout>(Resource.Id.surface_view);
        var button = FindViewById<Button>(Resource.Id.button1);

        button.Click += Button_Click;

        var display = AndroidHelper.GetDisplayMetrics(this);

        string uuid = Intent.GetStringExtra("GAME_UUID");
        var game = MainActivity.Games[uuid];
        view = new GLSurface(ApplicationContext, display);
        view.SetGame(game);
        panel.AddView(view);

        //panel.AddView(new TestSurface(ApplicationContext));
    }

    private void Button_Click(object? sender, EventArgs e)
    {
        DisplaySetting();
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        string uuid = intent.GetStringExtra("GAME_UUID");
        var game = MainActivity.Games[uuid];
        view.SetGame(game);
    }

    private void DisplaySetting()
    {
        var dialogFragment = new TabsDialogFragment();
        dialogFragment.Show(SupportFragmentManager, "tabs_dialog");
    }

    public override bool DispatchKeyEvent(KeyEvent? e)
    {
        if (IsEdit)
        {
            if (e.KeyCode == Keycode.Back)
            {
                if (e.Action == KeyEventActions.Down)
                {
                    //mControlLayout.askToExit(this);
                }
                return true;
            }
            return base.DispatchKeyEvent(e);
        }
        bool handleEvent = false;
        //if (!(handleEvent = view.processKeyEvent(e)))
        //{
        //    // ‰»ÎøÚ
        //    //&& !touchCharInput.isEnabled()
        //    if (e.KeyCode == Keycode.Back)
        //    {
        //        view.NowGame.SendKey(LwjglKeycode.GLFW_KEY_ESCAPE, e.Action == KeyEventActions.Down);
        //        return true;
        //    }
        //}
        return handleEvent;
    }
}