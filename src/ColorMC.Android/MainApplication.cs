using Android.App;
using Android.Runtime;
using Net.Kdt.Pojavlaunch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android;

[Application(Name = "coloryr.colormc.load.MainApplication")]
public partial class MainApplication : Application
{
    public MainApplication(IntPtr handle, JniHandleOwnership transfer)
        : base(handle, transfer)
    {
        
    }

    public override void OnCreate()
    {
        base.OnCreate();

        PojavApplication.Init(this);
    }
}
