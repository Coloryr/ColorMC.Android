using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using ColorMC.Android.GLRender;
using ColorMC.Android.UI.GameButton;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace ColorMC.Android.UI.Activity;

[Activity(Label = "GameActivity",
    Theme = "@style/MyTheme.NoActionBar",
    TaskAffinity = "colormc.android.game.render",
    //MainLauncher = true,
    ScreenOrientation = ScreenOrientation.SensorLandscape,
    Icon = "@drawable/icon")]
public class GameActivity : AppCompatActivity, IButtonFuntion
{
    private RelativeLayout _buttonList;
    private GLSurface view;
    private bool isEdit;
    private string nowGrouop;
    private ButtonLayout _layout;
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        SetContentView(Resource.Layout.activity_main);

        _buttonList = FindViewById<RelativeLayout>(Resource.Id.button_view)!;

        var display = AndroidHelper.GetDisplayMetrics(this);

        view = new GLSurface(ApplicationContext, display);
        FindViewById<RelativeLayout>(Resource.Id.surface_view)!
            .AddView(view);

        var uuid = Intent?.GetStringExtra("GAME_UUID");
        if (uuid != null
            && MainActivity.Games.TryGetValue(uuid, out var game))
        {
            view.SetGame(game);
        }

        LoadButtons(ButtonLayout.GenDefault());

        //panel.AddView(new TestSurface(ApplicationContext));
    }

    public override void OnBackPressed()
    {
        if (!view.NowGame.IsGameClose)
        {
            // 这里处理返回事件，比如弹出对话框
            new AlertDialog.Builder(ApplicationContext)
                .SetMessage("是否同时关闭游戏")
                .SetCancelable(false)
                .SetPositiveButton("是", (a, b) =>
                {
                    view.NowGame.Kill();
                })
                .SetNegativeButton("保持运行", (a, b) => 
                {
                    
                })
                .Show();
        }
    }

    private void LoadButtons(ButtonLayout layout)
    {
        _layout = layout;
        nowGrouop = layout.MainGroup;

        LoadGroup();
    }

    private void LoadGroup()
    {
        var group = _layout.Groups.Find(item => item.Name == nowGrouop);
        if (group == null)
        {
            return;
        }

        _buttonList.RemoveAllViews();

        foreach (var item in group.Buttons)
        {
            _buttonList.AddView(new ButtonView(item, this, this));
        }
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        var uuid = intent?.GetStringExtra("GAME_UUID");
        if (uuid != null && 
            MainActivity.Games.TryGetValue(uuid, out var game))
        {
            view.SetGame(game);
        }
    }

    //public override bool DispatchKeyEvent(KeyEvent? e)
    //{
    //    if (isEdit)
    //    {
    //        if (e.KeyCode == Keycode.Back)
    //        {
    //            if (e.Action == KeyEventActions.Down)
    //            {
    //                //mControlLayout.askToExit(this);
    //            }
    //            return true;
    //        }
    //        return base.DispatchKeyEvent(e);
    //    }
    //    bool handleEvent = false;
    //    //if (!(handleEvent = view.processKeyEvent(e)))
    //    //{
    //    //    //输入框
    //    //    //&& !touchCharInput.isEnabled()
    //    //    if (e.KeyCode == Keycode.Back)
    //    //    {
    //    //        view.NowGame.SendKey(LwjglKeycode.GLFW_KEY_ESCAPE, e.Action == KeyEventActions.Down);
    //    //        return true;
    //    //    }
    //    //}
    //    return handleEvent;
    //}

    public void ShowSetting()
    {
        var dialogFragment = new TabsDialogFragment(view);
        dialogFragment.Show(SupportFragmentManager, "tabs_dialog");
    }

    public void NextGroup()
    {
        
    }

    public void LastGroup()
    {
        
    }
}