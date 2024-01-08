using Android.Content;
using Android.Net;
using Java.IO;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using File = System.IO.File;

namespace ColorMC.Android.GLRender;

public static class GameLauncher
{
    public static Socket server;

    public const string Name = "colormc.android.game.sock";

    public static void Start(Context context)
    {
        string name = Name;
        if (File.Exists(name))
        {
            File.Delete(name);
        }

        LocalServerSocket socket = new(name);
        Task.Run(() =>
        {
            var local = socket.Accept();
            var sr1 = new InputStreamReader(local.InputStream);
            var sr2 = new BufferedReader(sr1);
            string str;
            while ((str = sr2.ReadLine()) != null)
            {
                RenderLog.Info("Pipe", str);
            }
        });

        // 获取文件描述符的整数值
        ProcessBuilder process = new(context.ApplicationInfo.NativeLibraryDir + "/" + "libcolormcnative.so");
        process.Environment().Add("GAME_FD", name[1..]);
        process.Directory(new(context.ApplicationInfo.NativeLibraryDir));
        RenderLog.Info("Pipe", "Start Pipe");
        var p = process.Start();
        Task.Run(() =>
        {
            string? str;
            using var inputReader = new BufferedReader(new InputStreamReader(p.InputStream));
            while ((str = inputReader.ReadLine()) != null)
            {
                RenderLog.Info("Pipe", str);
            }
            //脚本执行异常时的输出信息
            using var errorReader = new BufferedReader(new InputStreamReader(p.ErrorStream));
            while ((str = errorReader.ReadLine()) != null)
            {
                RenderLog.Error("Pipe", str);
            }
        });
    }
}
