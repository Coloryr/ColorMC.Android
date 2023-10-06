using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.Lib;

public static class JavaLog
{
    private static Thread t_read;
    private static FileStream s_stream;
    public unsafe static void Start(string file)
    {
        NativeHook.SetLogHandel(Log);

        if (File.Exists(file))
        {
            File.Delete(file);
        }
        s_stream = File.Open(file, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);

        t_read = new(Read)
        {
            Name = "JavaLog"
        };
        t_read.Start();
    }
    public unsafe static void Log(sbyte* data, int size)
    {
        s_stream.Write(new((byte*)data, size));
    }

    public static void Log(string data)
    {
        s_stream.Write(Encoding.UTF8.GetBytes(data + Environment.NewLine));
    }

    public static void Read()
    {
        NativeHook.JavaLogRead();
    }
}
