using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using ColorMC.Android.GLRender;
using ColorMC.Android.UI.GameButton;
using System.Linq;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace ColorMC.Android.UI.Activity;

[Activity(Label = "GameActivity",
    Theme = "@style/Theme.AppCompat.DayNight.NoActionBar",
    TaskAffinity = "colormc.android.game.render",
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
            game.GameClose = GameClose;
            view.SetGame(game);
        }

        LoadButtons(ButtonLayout.GenDefault());
    }

    private void GameClose()
    {
        AndroidHelper.Main.Post(() =>
        {
            if (MainActivity.Games.Count == 0)
            {
                Finish();
            }
            else
            {
                view.SetGame(MainActivity.Games.Values.ToArray()[0]);
            }
        });
    }

    public override void OnBackPressed()
    {
        if (view.NowGame?.IsClose == false)
        {
            _ = new AlertDialog.Builder(this)!
                .SetMessage(Resource.String.game_info1)!
                .SetCancelable(false)!
                .SetPositiveButton(Resource.String.game_info2, (a, b) =>
                {
                    view.NowGame.Kill();
                    Finish();
                })
                .SetNegativeButton(Resource.String.game_info3, (a, b) =>
                {
                    Finish();
                })
                .Show();
        }
        else
        {
            Finish();
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
            game.GameClose = GameClose;
            view.SetGame(game);
        }
    }

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