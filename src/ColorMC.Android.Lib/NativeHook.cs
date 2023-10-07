using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.Lib;

#pragma warning disable CA1401 // P/Invokes 应该是不可见的
public static partial class NativeHook
{
    public const string LibName = "ColorMCNative.so";

    //java log

    public unsafe delegate void LogHandel(sbyte* str, int size);

    [LibraryImport(LibName, EntryPoint = "java_log_set_handel")]
    public static partial void JavaLogSetHandel(LogHandel handel);

    [LibraryImport(LibName, EntryPoint = "java_log_read")]
    public static partial void JavaLogRead();

    [LibraryImport(LibName, EntryPoint = "java_log_start")]
    public static partial void JavaLogStart();

    //java exit

    public unsafe delegate void JavaExitHandel(int code);
    public unsafe delegate void JavaExitHandel1();

    [LibraryImport(LibName, EntryPoint = "java_set_exit_handel")]
    public static partial void JavaExitSetHandel(JavaExitHandel handel, JavaExitHandel1 handel1);

    [LibraryImport(LibName, EntryPoint = "java_on_exit_init")]
    public static partial void JavaOnExitInit();

    //java run
    [LibraryImport(LibName, EntryPoint = "java_run_init", StringMarshalling = StringMarshalling.Utf8)]
    public static partial int JavaRunInit(string dir, string[] args, int size);

    //native load
    [LibraryImport(LibName, EntryPoint = "load_native", StringMarshalling = StringMarshalling.Utf8)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool LoadNative(string path);

    [LibraryImport(LibName, EntryPoint = "set_native_ld", StringMarshalling = StringMarshalling.Utf8)]
    public static partial void SetNativeLd(string path);
}
#pragma warning restore CA1401 // P/Invokes 应该是不可见的