using Android.Util;
using ColorMC.Core.Helpers;
using ICSharpCode.SharpZipLib.Tar;
using SharpCompress.Compressors.Xz;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using static Android.InputMethodServices.Keyboard;

namespace ColorMC.Android;

public class JavaUnpack
{
    private int Size = 0;
    private int Now = 0;

    public Action<string, int, int>? ZipUpdate;

    private void TarArchive_ProgressMessageEvent(TarArchive archive, TarEntry entry, string message)
    {
        if (entry != null && message == null)
        {
            Now++;
            ZipUpdate?.Invoke(entry.Name, Now, Size);
        }
    }

    public void Unpack(Stream stream, string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }

        var gzipStream = new XZStream(stream);
        var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);

        Size = tarArchive.RecordSize;
        tarArchive.ProgressMessageEvent += TarArchive_ProgressMessageEvent;

        tarArchive.ExtractContents(path);
        tarArchive.Close();
        gzipStream.Close();
        stream.Close();

        Unpack(path + "/");
    }

    private void Unpack(string runtimePath)
    {
        var list = PathHelper.GetAllFile(runtimePath);
        foreach (var item in list)
        {
            if (item.Extension != ".pack")
            {
                continue;   
            }

            try
            {
                ProcessStartInfo info = new("libunpack200.so")
                {
                    WorkingDirectory = MainActivity.NativeLibDir
                };
                info.ArgumentList.Add("-r");
                info.ArgumentList.Add(item.FullName);
                info.ArgumentList.Add(item.FullName.Replace(".pack", ""));
                var p = Process.Start(info);
                p?.WaitForExit();
            }
            catch (Exception e)
            {
                Log.Error("Unpack", "Failed to unpack the runtime !\n" + e.ToString());
            }
        }
    }
}
