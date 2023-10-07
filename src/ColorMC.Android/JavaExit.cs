using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.Lib;

public static class JavaExit
{
    private static int s_exitCode;
    public static event Action<int>? GameExit;

    public static void Start()
    {
        NativeHook.JavaExitSetHandel(OnExit, OnExit);
        NativeHook.JavaOnExitInit();
    }

    public static void OnExit(int code)
    {
        s_exitCode = code;
        GameExit?.Invoke(s_exitCode);
    }

    public static void OnExit()
    {
        GameExit?.Invoke(s_exitCode);
    }
}
