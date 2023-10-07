using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AndroidX.AppCompat.App;
using ColorMC.Android.Lib;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android;

[Activity]
public class GameActivity : AppCompatActivity
{
    private GameSettingObj obj;
    private JavaInfo info;
    private MinecraftGLSurface _gameView;
    private RelativeLayout _layout;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var intent = Intent!;
        obj = InstancesPath.GetGame(intent.GetStringExtra("GAME"))!;
        info = JvmPath.GetInfo(intent.GetStringExtra("JAVA"))!;
        var args = intent.GetStringArrayExtra("ARG")!;

        _layout = new(this);
        _gameView = new(this)
        {
            Run = () =>
            {
                JavaLoad.Run(obj, info, args.ToList());
            }
        };

        _layout.AddView(_gameView);

        SetContentView(_layout);

        _gameView.Start();
    }
}
