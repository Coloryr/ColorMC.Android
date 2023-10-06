using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.Lib;

#pragma warning disable CA1401 // P/Invokes 应该是不可见的
public static partial class NativeHook
{
    public const string LibName = "ColorMCNative.so";

    //java log

    public unsafe delegate void LogHandel(sbyte* str, int size);

    [LibraryImport(LibName, EntryPoint = "set_log_handel")]
    public static partial void SetLogHandel(LogHandel handel);

    [LibraryImport(LibName, EntryPoint = "java_log_read")]
    public static partial void JavaLogRead();

    //java exit

    public unsafe delegate void JavaExitHandel(int code);

    public static partial void SetJavaExitHandel()
    { 
        
    }

    
}
#pragma warning restore CA1401 // P/Invokes 应该是不可见的