using Android.Graphics.Drawables;
using Android.Hardware.Lights;
using Android.Health.Connect.DataTypes.Units;
using Android.Views;
using ColorMC.Android.GLRender.Bridges;
using ColorMC.Core.Helpers;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using static Android.Icu.Text.ListFormatter;

namespace ColorMC.Android.GLRender;

public class Utf8Buffer : SafeHandle
{
    private GCHandle _gcHandle;
    private byte[]? _data;

    public Utf8Buffer(string? s) : base(IntPtr.Zero, true)
    {
        if (s == null)
            return;
        _data = Encoding.UTF8.GetBytes(s);
        _gcHandle = GCHandle.Alloc(_data, GCHandleType.Pinned);
        handle = _gcHandle.AddrOfPinnedObject();
    }

    public int ByteLen => _data?.Length ?? 0;

    protected override bool ReleaseHandle()
    {
        if (handle != IntPtr.Zero)
        {
            handle = IntPtr.Zero;
            _data = null;
            _gcHandle.Free();
        }
        return true;
    }

    public override bool IsInvalid => handle == IntPtr.Zero;

    public static unsafe string? StringFromPtr(IntPtr s)
    {
        var pstr = (byte*)s;
        if (pstr == null)
            return null;
        int len;
        for (len = 0; pstr[len] != 0; len++) ;

        var bytes = ArrayPool<byte>.Shared.Rent(len);

        try
        {
            Marshal.Copy(s, bytes, 0, len);
            return Encoding.UTF8.GetString(bytes, 0, len);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }
}

public static class RenderTest
{
    public static void Init(IntPtr window)
    {
        RenderType type = RenderType.ZINK;

        if (type == RenderType.ZINK)
        {
            OSMBase.Init(window, type);
        }
        else
        {
            GLBase.Init(type);
            EGLBase.EglInit(window, type);
        }
    }

    public static void ChangeSize(int width, int height)
    {
        GLBase.GetVersion();

        //设置视口大小
        GL.Viewport(0, 0, width, height);
        GL.ClearColor(1.0f, 1.0f, 0, 1.0f);
        GL.Clear(GL.GL_COLOR_BUFFER_BIT);
        EGLBase.SwapBuffers();
    }
}