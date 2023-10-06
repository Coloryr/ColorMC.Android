using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.Lib;

public static class JavaExit
{
    private static int s_exitCode;
    private static event Action<int>? GameExit;

    public static void Start()
    {
        NativeHook.SetJavaExitHandel(OnExit, OnExit);
        NativeHook.JavaOnExitInit();
    }

    public static void OnExit(int code)
    {
        s_exitCode = code;
    }

    public static void OnExit()
    {
        GameExit?.Invoke(s_exitCode);
    }
}
