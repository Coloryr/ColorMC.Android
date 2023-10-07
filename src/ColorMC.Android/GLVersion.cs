using Android.Util;
using Java.Util;
using Net.Kdt.Pojavlaunch.Extra;
using Net.Kdt.Pojavlaunch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android;

public static class GLVersion
{
    public static string SelectOpenGlVersion(string time)
    {
        // 1309989600 is 2011-07-07  2011-07-07T22:00:00+00:00
        if (string.IsNullOrWhiteSpace(time))
        {
            return "2";
        }

        try
        {
            var time1 = DateTime.Parse(time);

            string openGlVersion = time1 < new DateTime(2011,6,8) ? "1" : "2";
            Log.Info("GL_SELECT", openGlVersion);
            return openGlVersion;
        }
        catch (Exception ex)
        {
            Log.Error("GL_SELECT", ex.ToString());
        }

        return "2";
    }
}
