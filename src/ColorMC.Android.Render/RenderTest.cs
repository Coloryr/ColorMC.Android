using Android.Opengl;
using Java.Nio;
using System.Runtime.InteropServices;

namespace ColorMC.Android.GLRender;

public class QuadRenderer
{
    // Vertex coordinates
    private static readonly float[] s_vertex =
    [
        -1.0f, -1.0f, 0.0f,
        -1.0f,  1.0f, 0.0f,
         1.0f,  1.0f, 0.0f,
         1.0f, -1.0f, 0.0f,
    ];

    private static readonly float[] s_vertex_y =
    [
        -1.0f,  1.0f, 0.0f,
        -1.0f, -1.0f, 0.0f,
         1.0f, -1.0f, 0.0f,
         1.0f,  1.0f, 0.0f
    ];

    // Texture coordinates
    private static readonly float[] s_uv =
    [
        0.0f, 0.0f,
        0.0f, 1.0f,
        1.0f, 1.0f,
        1.0f, 0.0f
    ];

    private const string vertexShaderCode =
        "precision mediump float;\n" +
        "attribute vec4 vPosition;\n" +
        "attribute vec2 aTextureCoord;\n" +
        "varying vec2 vTexCoord;\n" +
        "void main() {\n" +
        "  gl_Position = vPosition;\n" +
        "  vTexCoord = aTextureCoord;\n" +
        "}";
    private const string fragmentShaderCode =
        "precision mediump float;\n" +
        "varying vec2 vTexCoord;\n" +
        "uniform sampler2D s_texture;\n" +
        "void main() {\n" +
        "  gl_FragColor = texture2D(s_texture, vTexCoord);\n" +
        "}";

    private readonly ShortBuffer _vertexIndexBuffer;
    private readonly FloatBuffer _vertexBuffer, _vertexBufferY, _uvBuffer;
    private readonly int _program, _positionHandle, _texCoordHandle, _frameBuffer;

    public QuadRenderer()
    {
        // texture
        ByteBuffer bb2 = ByteBuffer.AllocateDirect(s_uv.Length * 4);
        bb2.Order(ByteOrder.NativeOrder()!);
        _uvBuffer = bb2.AsFloatBuffer();
        _uvBuffer.Put(s_uv);
        _uvBuffer.Position(0);

        //uv
        ByteBuffer bb4 = ByteBuffer.AllocateDirect(s_vertex.Length * 4);
        bb4.Order(ByteOrder.NativeOrder()!);
        _vertexBuffer = bb4.AsFloatBuffer();
        _vertexBuffer.Put(s_vertex);
        _vertexBuffer.Position(0);

        bb4 = ByteBuffer.AllocateDirect(s_vertex_y.Length * 4);
        bb4.Order(ByteOrder.NativeOrder()!);
        _vertexBufferY = bb4.AsFloatBuffer();
        _vertexBufferY.Put(s_vertex_y);
        _vertexBufferY.Position(0);

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
        _positionHandle = GLES20.GlGetAttribLocation(_program, "vPosition");
        _texCoordHandle = GLES20.GlGetAttribLocation(_program, "aTextureCoord");
    }

    public void DrawTexture(int inputTexture, int width, int height,
        int renderWidth, int renderHeight, GameRender.DisplayType type, bool flipY)
    {
        GLES20.GlUseProgram(_program);

        GLES20.GlEnableVertexAttribArray(_positionHandle);
        GLES20.GlVertexAttribPointer(_positionHandle, 3, GLES20.GlFloat, false, 0, flipY ? _vertexBufferY : _vertexBuffer);

        GLES20.GlEnableVertexAttribArray(_texCoordHandle);
        GLES20.GlVertexAttribPointer(_texCoordHandle, 2, GLES20.GlFloat, false, 0, _uvBuffer);

        // Calculate viewport based on fill and scale parameters
        int viewportWidth = renderWidth, viewportHeight = renderHeight, x = 0, y = 0;
        if (type == GameRender.DisplayType.Scale)
        {
            // Scale to full screen while maintaining aspect ratio
            float aspectRatioTexture = (float)renderWidth / renderHeight;
            float aspectRatioWindow = (float)width / height;
            if (aspectRatioWindow > aspectRatioTexture)
            {
                // Window is wider than texture
                viewportHeight = height;
                viewportWidth = (int)(height * aspectRatioTexture);
            }
            else
            {
                // Window is taller than texture
                viewportWidth = width;
                viewportHeight = (int)(width / aspectRatioTexture);
            }

            // Center the viewport
            x = 0;
            y = 0;
        }
        else if (type == GameRender.DisplayType.Full)
        {
            // Stretch to full screen without maintaining aspect ratio
            viewportWidth = width;
            viewportHeight = height;
            x = 0;
            y = 0;
        }
        else if (type == GameRender.DisplayType.None)
        {
            viewportWidth = renderWidth;
            viewportHeight = renderHeight;

            // Center the viewport
            x = (width - viewportWidth) / 2;
            y = (height - viewportHeight) / 2;
        }

        GLES20.GlViewport(x, y, viewportWidth, viewportHeight);

        GLES20.GlActiveTexture(GLES20.GlTexture0);
        GLES20.GlBindTexture(GLES20.GlTexture2d, inputTexture);

        // draw
        GLES20.GlDrawArrays(GLES20.GlTriangleFan, 0, 4);

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