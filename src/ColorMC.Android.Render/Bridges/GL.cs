using System.Runtime.InteropServices;

namespace ColorMC.Android.GLRender.Bridges;

public static class GL
{
    //GL_APICALL void GL_APIENTRY glViewport (GLint x, GLint y, GLsizei width, GLsizei height);
    public delegate void glViewport(GLint x, GLint y, GLsizei width, GLsizei height);
    //GL_APICALL void GL_APIENTRY glClearColor (GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha);
    public delegate void glClearColor(GLfloat red, GLfloat green, GLfloat blue, GLfloat alpha);
    //GL_APICALL void GL_APIENTRY glClear (GLbitfield mask);
    public delegate void glClear(GLbitfield mask);
    //GL_APICALL const GLubyte *GL_APIENTRY glGetString (GLenum name);
    public unsafe delegate IntPtr glGetString(GLenum name);

    public static glViewport Viewport;
    public static glClearColor ClearColor;
    public static glClear Clear;
    public static glGetString GetString;

    public const int GL_FALSE = 0;
    public const int GL_ZERO = 0;
    public const int GL_ONE = 1;
    public const int GL_TRIANGLES = 0x0004;
    public const int GL_DEPTH_BUFFER_BIT = 0x0100;
    public const int GL_ONE_MINUS_SRC_COLOR = 0x0301;
    public const int GL_SRC_ALPHA = 0x0302;
    public const int GL_ONE_MINUS_SRC_ALPHA = 0x0303;
    public const int GL_DST_COLOR = 0x0306;
    public const int GL_CCW = 0x0901;
    public const int GL_CULL_FACE = 0x0B44;
    public const int GL_FRONT_FACE = 0x0B46;
    public const int GL_DEPTH_TEST = 0x0B71;
    public const int GL_STENCIL_TEST = 0x0B90;
    public const int GL_VIEWPORT = 0x0BA2;
    public const int GL_BLEND = 0x0BE2;
    public const int GL_SCISSOR_TEST = 0x0C11;
    public const int GL_COLOR_WRITEMASK = 0x0C23;
    public const int GL_TEXTURE_2D = 0x0DE1;
    public const int GL_UNSIGNED_BYTE = 0x1401;
    public const int GL_UNSIGNED_SHORT = 0x1403;
    public const int GL_FLOAT = 0x1406;
    public const int GL_RGBA = 0x1908;
    public const int GL_VENDOR = 0x1F00;
    public const int GL_RENDERER = 0x1F01;
    public const int GL_VERSION = 0x1F02;
    public const int GL_LINEAR = 0x2601;
    public const int GL_LINEAR_MIPMAP_LINEAR = 0x2703;
    public const int GL_TEXTURE_MAG_FILTER = 0x2800;
    public const int GL_TEXTURE_MIN_FILTER = 0x2801;
    public const int GL_TEXTURE_WRAP_S = 0x2802;
    public const int GL_TEXTURE_WRAP_T = 0x2803;
    public const int GL_COLOR_BUFFER_BIT = 0x4000;
    public const int GL_TEXTURE_BINDING_2D = 0x8069;
    public const int GL_BLEND_DST_RGB = 0x80C8;
    public const int GL_BLEND_SRC_RGB = 0x80C9;
    public const int GL_BLEND_DST_ALPHA = 0x80CA;
    public const int GL_BLEND_SRC_ALPHA = 0x80CB;
    public const int GL_CLAMP_TO_EDGE = 0x812F;
    public const int GL_TEXTURE0 = 0x84C0;
    public const int GL_TEXTURE1 = 0x84C1;
    public const int GL_ACTIVE_TEXTURE = 0x84E0;
    public const int GL_TEXTURE_MAX_ANISOTROPY_EXT = 0x84FE;
    public const int GL_VERTEX_ATTRIB_ARRAY_ENABLED = 0x8622;
    public const int GL_ARRAY_BUFFER = 0x8892;
    public const int GL_ELEMENT_ARRAY_BUFFER = 0x8893;
    public const int GL_ARRAY_BUFFER_BINDING = 0x8894;
    public const int GL_ELEMENT_ARRAY_BUFFER_BINDING = 0x8895;
    public const int GL_FRAGMENT_SHADER = 0x8B30;
    public const int GL_VERTEX_SHADER = 0x8B31;
    public const int GL_COMPILE_STATUS = 0x8B81;
    public const int GL_LINK_STATUS = 0x8B82;
    public const int GL_VALIDATE_STATUS = 0x8B83;
    public const int GL_INFO_LOG_LENGTH = 0x8B84;
    public const int GL_SHADING_LANGUAGE_VERSION = 0x8B8C;
    public const int GL_CURRENT_PROGRAM = 0x8B8D;
    public const int GL_STATIC_DRAW = 0x88E4;
    public const int GL_FRAMEBUFFER_BINDING = 0x8CA6;
    public const int GL_COLOR_ATTACHMENT0 = 0x8CE0;
    public const int GL_FRAMEBUFFER = 0x8D40;

    public static void Load(string file)
    {
        IntPtr dl_handle = NativeLoader.LoadLibrary(file);

        Viewport = Marshal.GetDelegateForFunctionPointer<glViewport>(NativeLoader.GetProcAddress(dl_handle, "glViewport"));
        ClearColor = Marshal.GetDelegateForFunctionPointer<glClearColor>(NativeLoader.GetProcAddress(dl_handle, "glClearColor"));
        Clear = Marshal.GetDelegateForFunctionPointer<glClear>(NativeLoader.GetProcAddress(dl_handle, "glClear"));
        GetString = Marshal.GetDelegateForFunctionPointer<glGetString>(NativeLoader.GetProcAddress(dl_handle, "glGetString"));
    }
}
