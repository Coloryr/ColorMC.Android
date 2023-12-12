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
    public const string ver =
@"#version 300 es
in vec4 aPosition;
in vec2 aTexCoord;
out vec2 TexCoord;
void main() {
  TexCoord = aTexCoord;
  gl_Position = aPosition;
}";

    // 片元着色器
    public const string fragment =
@"#version 300 es
precision mediump float;
out vec4 FragColor;
in vec2 TexCoord;
uniform sampler2D ourTexture;
void main()
{
    FragColor = texture(ourTexture, TexCoord);
}";

    public static readonly float[] VERTICES_AND_TEXTURE =
    [
        0.5f, -0.5f, // 右下
        // 纹理坐标
        1.0f, 1.0f,
        0.5f, 0.5f, // 右上
        // 纹理坐标
        1.0f, 0.0f,
        -0.5f, -0.5f, // 左下
        // 纹理坐标
        0.0f, 1.0f,
        -0.5f, 0.5f, // 左上
        // 纹理坐标
        0.0f, 0.0f
    ];

    // 真正的纹理坐标在图片的左下角
    public static readonly float[] FBO_VERTICES_AND_TEXTURE =
    [
        1.0f, -1.0f, // 右下
        // 纹理坐标
        1.0f, 0.0f,
        1.0f, 1.0f, // 右上
        // 纹理坐标
        1.0f, 1.0f,
        -1.0f, -1.0f, // 左下
        // 纹理坐标
        0.0f, 0.0f,
        -1.0f, 1.0f, // 左上
        // 纹理坐标
        0.0f, 1.0f
    ];

    // 使用byte类型比使用short或者int类型节约内存
    public static readonly byte[] indices =
    [
        // 注意索引从0开始!
        // 此例的索引(0,1,2,3)就是顶点数组vertices的下标，
        // 这样可以由下标代表顶点组合成矩形
        0, 1, 2, // 第一个三角形
        1, 2, 3  // 第二个三角形
    ];

    //public static unsafe void GetProgramiv(int index, int type, int* length)
    //{
    //    GL.GetProgramiv(index, type, length);
    //}

    //public static unsafe void GetProgramInfoLog(int index, out string log)
    //{
    //    int logLength;
    //    GetProgramiv(index, GL_INFO_LOG_LENGTH, &logLength);
    //    var logData = new byte[logLength];
    //    int len;
    //    fixed (void* ptr = logData)
    //        GetProgramInfoLog(index, logLength, out len, ptr);
    //    log = Encoding.UTF8.GetString(logData, 0, len);
    //}

    //public static bool LoadShaders(int type, string code)
    //{
    //    // 按照类型，创建着色器
    //    int shader = CreateShader(type);

    //    using var b = new Utf8Buffer(code);
    //    var ptr = b.DangerousGetHandle();
    //    var len = new IntPtr(b.ByteLen);
    //    ShaderSource(shader, 1, new IntPtr(&ptr), new IntPtr(&len));

    //    // 编译
    //    CompileShader(shader);
    //    // 检测编译状态
    //    int result = GL_FALSE;
    //    GetShaderiv(shader, GL_COMPILE_STATUS, &result);
    //    if (result != GL_TRUE)
    //    {
    //        GLint infoLen = 0;
    //        GetShaderiv(shader, GL_INFO_LOG_LENGTH, &infoLen);
    //        char error[infoLen + 1];
    //        // 获取编译错误
    //        GetShaderInfoLog(shader, sizeof(error) / sizeof(error[0]), &infoLen, error);
    //        LOGE("着色器编译失败:%s,%s", error, code);
    //    }
    //    return shader;
    //}

    //public static void GlProgram()
    //{
    //    positionHandle = glGetAttribLocation(program, "aPosition");
    //    textureHandle = glGetAttribLocation(program, "aTexCoord");
    //    textureSampler = glGetUniformLocation(program, "ourTexture");
    //    LOGD("program:%d", program);
    //    LOGD("positionHandle:%d", positionHandle);
    //    LOGD("textureHandle:%d", textureHandle);
    //    LOGD("textureSample:%d", textureSampler);
    //}

    public static void ChangeSize(int width, int height)
    {
        GLBase.GetVersion();

        //设置视口大小
        GL.Viewport(0, 0, width, height);
        GL.Clear(GL.GL_COLOR_BUFFER_BIT);
        EGLBase.SwapBuffers();
    }
}