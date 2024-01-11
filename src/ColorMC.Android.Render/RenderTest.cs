using Android.Opengl;
using Java.Nio;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace ColorMC.Android.GLRender;

public static class GLHelper
{
    public static int CreateTexture()
    {
        int[] textures = new int[1];
        GLES20.GlGenTextures(1, textures, 0);
        GLES20.GlBindTexture(GLES20.GlTexture2d, textures[0]);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapS, GLES20.GlClampToEdge);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureWrapT, GLES20.GlClampToEdge);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMinFilter, GLES20.GlLinear);
        GLES20.GlTexParameteri(GLES20.GlTexture2d, GLES20.GlTextureMagFilter, GLES20.GlLinear);
        return textures[0];
    }

    public static void DeleteTexture(int texId)
    {
        int[] textures = [texId];
        GLES20.GlDeleteTextures(1, textures, 0);
    }

    public static string GetFileName(this GameRenderType type)
    {
        return type switch
        {
            _ => "libgl4es_114.so"
        };
    }
}

public class QuadRenderer
{
    static readonly float[] squareCoords = 
        [
            -1, -1, -1, 1, 1, 1, -1, -1, 1, 1, 1, -1
        ];
    static readonly float[] textureVertices = {
            0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 0
    };
    static float[] textureVerticesFlipY = {
            0, 1, 0, 0, 1, 0, 0, 1, 1, 0, 1, 1
    };
    private const string vertexShaderCode = "" +
            "precision mediump float;\n" +
            "attribute vec4 vPosition;\n" +
            "attribute vec4 inputTexCoordinate;\n" +
            "varying vec4 texCoordinate;\n" +
            "void main() {\n" +
            "  gl_Position = vPosition;\n" +
            "  texCoordinate = inputTexCoordinate;\n" +
            "}";
    private const string fragmentShaderCode = "" +
            "precision mediump float;\n" +
            "varying vec4 texCoordinate;\n" +
            "uniform sampler2D s_texture;\n" +
            "void main() {\n" +
            "  gl_FragColor = texture2D(s_texture, vec2(texCoordinate.x,texCoordinate.y));\n" +
            "}";
    private FloatBuffer vertexBuffer, textureVerticesBuffer;
    private int mProgram;
    private int mPositionHandle;
    private int mTexCoordHandle;
    private int mTextureLocation;
    private int mFrameBuffer;

    public QuadRenderer() : this(false)
    {
        
    }

    public QuadRenderer(bool filpY)
    {
        // vertex
        ByteBuffer bb = ByteBuffer.AllocateDirect(squareCoords.Length * 4);
        bb.Order(ByteOrder.NativeOrder());
        vertexBuffer = bb.AsFloatBuffer();
        vertexBuffer.Put(squareCoords);
        vertexBuffer.Position(0);

        // texture
        float[] targetTextureVertices = filpY ? textureVerticesFlipY : textureVertices;
        ByteBuffer bb2 = ByteBuffer.AllocateDirect(targetTextureVertices.Length * 4);
        bb2.Order(ByteOrder.NativeOrder());
        textureVerticesBuffer = bb2.AsFloatBuffer();
        textureVerticesBuffer.Put(targetTextureVertices);
        textureVerticesBuffer.Position(0);

        // shader
        int vertexShader = LoadShader(GLES20.GlVertexShader, vertexShaderCode);
        int fragmentShader = LoadShader(GLES20.GlFragmentShader, fragmentShaderCode);
        mProgram = GLES20.GlCreateProgram();
        GLES20.GlAttachShader(mProgram, vertexShader);
        GLES20.GlAttachShader(mProgram, fragmentShader);
        GLES20.GlLinkProgram(mProgram);
        GLES20.GlDeleteShader(vertexShader);
        GLES20.GlDeleteShader(fragmentShader);

        GLES20.GlUseProgram(mProgram);
        mTextureLocation = GLES20.GlGetUniformLocation(mProgram, "s_texture");
        mPositionHandle = GLES20.GlGetAttribLocation(mProgram, "vPosition");
        mTexCoordHandle = GLES20.GlGetAttribLocation(mProgram, "inputTexCoordinate");
    }

    public void DrawTexture(int inputTexture, int width, int height,
        int renderWidth, int renderHeight, bool fill)
    {
        GLES20.GlUseProgram(mProgram);

        GLES20.GlActiveTexture(GLES20.GlTexture0);
        GLES20.GlBindTexture(GLES20.GlTexture2d, inputTexture);
        GLES20.GlUniform1i(mTextureLocation, 0);

        GLES20.GlEnableVertexAttribArray(mPositionHandle);
        GLES20.GlVertexAttribPointer(mPositionHandle, 2, GLES20.GlFloat, false, 0, vertexBuffer);

        GLES20.GlEnableVertexAttribArray(mTexCoordHandle);
        GLES20.GlVertexAttribPointer(mTexCoordHandle, 2, GLES20.GlFloat, false, 0, textureVerticesBuffer);

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

        GLES20.GlDisableVertexAttribArray(mPositionHandle);
        GLES20.GlDisableVertexAttribArray(mTexCoordHandle);
        GLES20.GlBindTexture(GLES20.GlTexture0, 0);
        GLES20.GlUseProgram(0);
    }

    private int LoadShader(int type, string shaderCode)
    {
        int shader = GLES20.GlCreateShader(type);
        GLES20.GlShaderSource(shader, shaderCode);
        GLES20.GlCompileShader(shader);
        return shader;
    }

    public void Close()
    {
        GLES20.GlDeleteProgram(mProgram);
    }
}

public static partial class RenderTest
{
    [DllImport("libcolormcnative_display.so", EntryPoint = "getBuffer")]
    public static extern IntPtr GetBuffer(string path);

    [DllImport("libcolormcnative_display.so", EntryPoint = "bindTexture")]
    public static extern bool BindTexture(int texid, IntPtr buffer,
        out int width, out int height, out IntPtr texture);

    [DllImport("libcolormcnative_display.so", EntryPoint = "available")]
    public static extern bool Available();

    [DllImport("libcolormcnative_display.so", EntryPoint = "deleteBuffer")]
    public static extern bool DeleteBuffer(IntPtr buffer, IntPtr texture);
}