using Android.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.GLRender;

public class RenderLog
{
    public static void Error(string tag, string data)
    {
        Log.Error(tag, data);
    }

    public static void Info(string tag, string data)
    {
        Log.Info(tag, data);
    }

    public static void Warn(string tag, string data)
    {
        Log.Warn(tag, data);
    }
}
