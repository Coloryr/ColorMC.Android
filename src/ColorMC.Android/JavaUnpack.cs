using Android.Runtime;
using Android.Systems;
using Android.Util;
using ColorMC.Android.UI.Activity;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ColorMC.Android;

public class JavaUnpack
{
    private int Size = 0;
    private int Now = 0;

    public ColorMCCore.ZipUpdate? ZipUpdate;

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

        IntPtr classHandle = JNIEnv.FindClass("org/tukaani/xz/XZInputStream");
        IntPtr constructorId = JNIEnv.GetMethodID(classHandle, "<init>", "(Ljava/io/InputStream;)V");

        try
        {
            var native_p0 = new InputStreamAdapter(stream);
            var objectHandle = JNIEnv.NewObject(classHandle, constructorId, new JValue(native_p0));

            var stream1 = new TarInputStream(InputStreamInvoker.FromJniHandle(objectHandle, JniHandleOwnership.TransferLocalRef), TarBuffer.DefaultBlockFactor, Encoding.UTF8);
            var tarArchive = TarArchive.CreateInputTarArchive(stream1, Encoding.UTF8);

            Size = tarArchive.RecordSize;
            tarArchive.ProgressMessageEvent += TarArchive_ProgressMessageEvent;

            var fullDistDir = Path.GetFullPath(path).TrimEnd('/', '\\');

            while (true)
            {
                TarEntry entry = stream1.GetNextEntry();

                if (entry == null)
                {
                    break;
                }

                if (entry.TarHeader.TypeFlag == TarHeader.LF_LINK || entry.TarHeader.TypeFlag == TarHeader.LF_SYMLINK)
                {
                    try
                    {
                        Os.Symlink(entry.TarHeader.Name, entry.TarHeader.LinkName);
                    }
                    catch
                    {

                    }
                    TarArchive_ProgressMessageEvent(null, entry, null);
                    continue;
                }

                ExtractEntry(stream1, fullDistDir, entry, false);
            }

            tarArchive.Close();
            stream.Close();
        }
        catch (Exception e)
        {

        }

        Unpack(path + "/");
        Rename(path);

        File.Copy(MainActivity.NativeLibDir + "/libawt_xawt.so", path + "/lib/libawt_xawt.so");
    }

    public static string GetLibPath(string path)
    {
        string arch = "";

        path += "/lib";

        if (Directory.Exists(path + "/amd64"))
        {
            arch = "amd64";
        }
        else if (Directory.Exists(path + "/aarch64"))
        {
            arch = "aarch64";
        }
        else if (Directory.Exists(path + "/aarch32"))
        {
            arch = "aarch32";
        }
        else if (Directory.Exists(path + "/i386"))
        {
            arch = "i386";
        }
        else if (Directory.Exists(path + "/i486"))
        {
            arch = "i486";
        }
        else if (Directory.Exists(path + "/i586"))
        {
            arch = "i586";
        }

        path += "/" + arch;

        return path;
    }

    private static void Rename(string path)
    {
        path = GetLibPath(path);
        if (File.Exists($"{path}/libfreetype.so.6"))
        {
            File.Move($"{path}/libfreetype.so.6", $"{path}/libfreetype.so");
        }
    }

    private static void EnsureDirectoryExists(string directoryName)
    {
        if (!Directory.Exists(directoryName))
        {
            try
            {
                Directory.CreateDirectory(directoryName);
            }
            catch (Exception e)
            {
                throw new TarException("Exception creating directory '" + directoryName + "', " + e.Message, e);
            }
        }
    }

    private void ExtractEntry(TarInputStream tarIn, string destDir, TarEntry entry, bool allowParentTraversal)
    {
        TarArchive_ProgressMessageEvent(null, entry, null);

        string name = entry.Name;

        if (Path.IsPathRooted(name))
        {
            name = name.Substring(Path.GetPathRoot(name).Length);
        }

        name = name.Replace('/', Path.DirectorySeparatorChar);

        string destFile = Path.Combine(destDir, name);
        var destFileDir = Path.GetDirectoryName(Path.GetFullPath(destFile)) ?? "";

        var isRootDir = entry.IsDirectory && entry.Name == "";

        if (!allowParentTraversal && !isRootDir && !destFileDir.StartsWith(destDir, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new InvalidNameException("Parent traversal in paths is not allowed");
        }

        if (entry.IsDirectory)
        {
            EnsureDirectoryExists(destFile);
        }
        else
        {
            string parentDirectory = Path.GetDirectoryName(destFile);
            EnsureDirectoryExists(parentDirectory);

            bool process = true;
            var fileInfo = new FileInfo(destFile);

            if (process)
            {
                using var outputStream = File.Create(destFile);
                tarIn.CopyEntryContents(outputStream);
            }
        }
    }

    private static void Unpack(string runtimePath)
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
