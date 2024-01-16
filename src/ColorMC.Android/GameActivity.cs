using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using ColorMC.Android.GLRender;

namespace ColorMC.Android;

[Activity(Label = "GameActivity",
    //MainLauncher = true,
    Theme = "@style/MyTheme.NoActionBar",
    TaskAffinity = "colormc.android.game.render",
   
    Icon = "@drawable/icon")]
public class GameActivity : Activity, View.IOnClickListener
{
    private EditText width, height;
    private GLSurface view;
    private bool IsEdit;
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
         
        SetContentView(Resource.Layout.activity_main);

        var panel = FindViewById<RelativeLayout>(Resource.Id.surface_view);
        width = FindViewById<EditText>(Resource.Id.width_setting);
        height = FindViewById<EditText>(Resource.Id.height_setting);

        var button = FindViewById<Button>(Resource.Id.setting);
        
        button.SetOnClickListener(this);

        var display = AndroidHelper.GetDisplayMetrics(this);

        string uuid = Intent.GetStringExtra("GAME_UUID");
        var game = MainActivity.Games[uuid];
        view = new GLSurface(ApplicationContext, display);
        view.SetGame(game);
        panel.AddView(view);

        //panel.AddView(new TestSurface(ApplicationContext));
    }

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        string uuid = intent.GetStringExtra("GAME_UUID");
        var game = MainActivity.Games[uuid];
        view.SetGame(game);
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


    public void OnClick(View? v)
    {
        view.SetSize(width.Text, height.Text);
    }
}