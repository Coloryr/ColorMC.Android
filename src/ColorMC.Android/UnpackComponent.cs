using Android.Content;
using Android.Content.Res;
using Android.Util;
using ColorMC.Core.Helpers;
using Java.Util.Concurrent;
using Net.Kdt.Pojavlaunch;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android;

public static class UnpackComponent
{
    public static string RunDir;

    public static void Init(string dir)
    {
        RunDir = dir + "components";
    }

    private static void StartUnpack(Context ctx, string component)
    {
        var am = ctx.Assets!;

        var path = new FileInfo(RunDir + "/" + component + "/version");
        using var stream = File.OpenRead(path.FullName);
        if (!path.Exists) 
        {
            if (path.Directory?.Exists == true) 
            {
                path.Directory.Delete(true);
            }
            path.Directory?.Create();

            Log.Info("UnpackComponent", component + ": Pack was installed manually, or does not exist, unpacking new...");
            var fileList = am.List("components/" + component);
            if (fileList == null)
            {
                return;
            }
            foreach (string s in fileList) 
            {
                PathHelper.CopyFile("components/" + component + "/" + s, RunDir + "/" + component);
            }
        } 
        else
        {
            var release1 = PathHelper.ReadText(path.FullName);
            var release2 = PathHelper.ReadText(am.Open("components/" + component + "/version"));
            if (release1 != release2)
            {
                if (path.Directory?.Exists == true)
                {
                    path.Directory.Delete(true);
                }
                path.Directory?.Create();

                var fileList = am.List("components/" + component);
                if (fileList == null)
                {
                    return;
                }
                foreach (string s in fileList)
                {
                    PathHelper.CopyFile("components/" + component + "/" + s, RunDir + "/" + component);
                }
            }
            else
            {
                Log.Info("UnpackComponent", component + ": Pack is up-to-date with the launcher, continuing...");
            }
        }
    }

    public static void UnpackTask(Context ctx)
    {
        Task.Run(()=> 
        {
            try
            {
                StartUnpack(ctx, "caciocavallo");
                StartUnpack(ctx, "caciocavallo17");
                // Since the Java module system doesn't allow multiple JARs to declare the same module,
                // we repack them to a single file here
                StartUnpack(ctx, "lwjgl3");
                StartUnpack(ctx, "security");
            }
            catch (IOException e)
            {
                Log.Error("StartUnpack", "Failed o unpack components !", e);
            }
        });
    }
}
