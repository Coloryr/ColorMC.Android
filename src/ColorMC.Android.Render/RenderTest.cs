using Android.Opengl;
using Java.Nio;
using System.Runtime.InteropServices;

namespace ColorMC.Android.GLRender;

public class QuadRenderer
{
    private static readonly float[] squareCoords =
    [
        -1, -1, -1, 1, 1, 1, -1, -1, 1, 1, 1, -1
    ];
    private static readonly float[] textureVertices =
    [
        0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0
    ];
    private static float[] textureVerticesFlipY =
    [
        0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1
    ];
    private const string vertexShaderCode =
            "precision mediump float;\n" +
            "attribute vec4 vPosition;\n" +
            "attribute vec4 inputTexCoordinate;\n" +
            "varying vec4 texCoordinate;\n" +
            "void main() {\n" +
            "  gl_Position = vPosition;\n" +
            "  texCoordinate = inputTexCoordinate;\n" +
            "}";
    private const string fragmentShaderCode =
            "precision mediump float;\n" +
            "varying vec4 texCoordinate;\n" +
            "uniform sampler2D s_texture;\n" +
            "void main() {\n" +
            "  gl_FragColor = texture2D(s_texture, vec2(texCoordinate.x,texCoordinate.y));\n" +
            "}";

    private readonly FloatBuffer _vertexBuffer, _textureVerticesBuffer;
    private readonly int _program, _positionHandle,
        _texCoordHandle, _textureLocation, _frameBuffer;

    public QuadRenderer() : this(false)
    {

    }

    public QuadRenderer(bool filpY)
    {
        // vertex
        ByteBuffer bb = ByteBuffer.AllocateDirect(squareCoords.Length * 4);
        bb.Order(ByteOrder.NativeOrder()!);
        _vertexBuffer = bb.AsFloatBuffer();
        _vertexBuffer.Put(squareCoords);
        _vertexBuffer.Position(0);

        // texture
        float[] targetTextureVertices = filpY ? textureVerticesFlipY : textureVertices;
        ByteBuffer bb2 = ByteBuffer.AllocateDirect(targetTextureVertices.Length * 4);
        bb2.Order(ByteOrder.NativeOrder()!);
        _textureVerticesBuffer = bb2.AsFloatBuffer();
        _textureVerticesBuffer.Put(targetTextureVertices);
        _textureVerticesBuffer.Position(0);

        // shader
        int vertexShader = LoadShader(GLES20.GlVertexShader, vertexShaderCode);
        int fragmentShader = LoadShader(GLES20.GlFragmentShader, fragmentShaderCode);
        _program = GLES20.GlCreateProgram();
        GLES20.GlAttachShader(_program, vertexShader);
        GLES20.GlAttachShader(_program, fragmentShader);
        GLES20.GlLinkProgram(_program);
        GLES20.GlDeleteShader(vertexShader);
        GLES20.GlDeleteShader(fragmentShader);

        GLES20.GlUseProgram(_program);
        _textureLocation = GLES20.GlGetUniformLocation(_program, "s_texture");
        _positionHandle = GLES20.GlGetAttribLocation(_program, "vPosition");
        _texCoordHandle = GLES20.GlGetAttribLocation(_program, "inputTexCoordinate");
    }

    public void DrawTexture(int inputTexture, int width, int height,
        int renderWidth, int renderHeight, bool fill)
    {
        GLES20.GlUseProgram(_program);

        GLES20.GlActiveTexture(GLES20.GlTexture0);
        GLES20.GlBindTexture(GLES20.GlTexture2d, inputTexture);
        GLES20.GlUniform1i(_textureLocation, 0);

        GLES20.GlEnableVertexAttribArray(_positionHandle);
        GLES20.GlVertexAttribPointer(_positionHandle, 2, GLES20.GlFloat, false, 0, _vertexBuffer);

        GLES20.GlEnableVertexAttribArray(_texCoordHandle);
        GLES20.GlVertexAttribPointer(_texCoordHandle, 2, GLES20.GlFloat, false, 0, _textureVerticesBuffer);

        // draw
        if (fill)
        {
            GLES20.GlViewport(0, 0, renderWidth, renderHeight);
        }
        else
        {
            int x = width / 2 - renderWidth / 2;
            int y = height / 2 - renderHeight / 2;
            GLES20.GlViewport(x, y, renderWidth, renderHeight);
        }
        GLES20.GlDrawArrays(GLES20.GlTriangleStrip, 0, 6);

        GLES20.GlDisableVertexAttribArray(_positionHandle);
        GLES20.GlDisableVertexAttribArray(_texCoordHandle);
        GLES20.GlBindTexture(GLES20.GlTexture0, 0);
        GLES20.GlUseProgram(0);
    }

    private static int LoadShader(int type, string shaderCode)
    {
        int shader = GLES20.GlCreateShader(type);
        GLES20.GlShaderSource(shader, shaderCode);
        GLES20.GlCompileShader(shader);
        return shader;
    }

    public void Close()
    {
        GLES20.GlDeleteProgram(_program);
    }
}

public static partial class RenderNative
{
    [LibraryImport("libcolormcnative_display.so", EntryPoint = "getBuffer", 
        StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr GetBuffer(string path);

    [LibraryImport("libcolormcnative_display.so", EntryPoint = "bindTexture")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool BindTexture(int texid, IntPtr buffer,
        out int width, out int height, out IntPtr texture);

    [LibraryImport("libcolormcnative_display.so", EntryPoint = "available")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool Available();

    [LibraryImport("libcolormcnative_display.so", EntryPoint = "deleteBuffer")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool DeleteBuffer(IntPtr buffer, IntPtr texture);
}