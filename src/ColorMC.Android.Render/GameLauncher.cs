﻿using Android.Content;
using Android.Opengl;
using Android.Speech.Tts;
using Android.Systems;
using Java.IO;
using Java.Lang;
using System.Net.Sockets;
using static Android.Icu.Text.ListFormatter;
using static System.Net.Mime.MediaTypeNames;
using Exception = System.Exception;
using Thread = System.Threading.Thread;

namespace ColorMC.Android.GLRender;

public class GameSock
{
    private static readonly byte[] MagicHead = [0x0E, 0x3A, 0x1E, 0x06, 0x14, 0xC5];

    private readonly string _socketPath;
    private Socket socket;

    public Action<byte>? CommandRead;

    public GameSock(string socketPath)
    {
        _socketPath = socketPath;
    }

    public bool Connect()
    {
        socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

        var unixEp = new UnixDomainSocketEndPoint(_socketPath);

        try
        {
            socket.Connect(unixEp);

            RenderLog.Info("Game Sock", "Connected to the server.");

            new Thread(Read).Start();

            return true;
        }
        catch (Exception e)
        {
            RenderLog.Info("Game Sock", "Exception: " + e.Message);
        }

        return false;
    }

    private async void Read()
    {
        var buffer = new byte[1024];
        while (true)
        {
            var size = await socket.ReceiveAsync(buffer);
            for (int a = 0; a < size - 6; a++)
            {
                if (buffer[a] == MagicHead[0]
                    && buffer[a + 1] == MagicHead[1]
                    && buffer[a + 2] == MagicHead[2]
                    && buffer[a + 3] == MagicHead[3]
                    && buffer[a + 4] == MagicHead[4]
                    && buffer[a + 5] == MagicHead[5])
                {
                    CommandRead?.Invoke(buffer[a + 6]);
                }
            }
        }
    }

    private void SendPack(byte command, ushort data)
    {
        var list = new List<byte>(MagicHead)
        {
            command,
            (byte)((data) & 0xff),
            (byte)((data >> 8) & 0xff),
            command
        };

        socket.Send(list.ToArray());
    }

    public void SetWindowSize(ushort width, ushort height)
    {
        SendPack(0x01, width);
        SendPack(0x02, height);
    }

    public void ChangeSize()
    {
        SendPack(0x03, 0);
    }

    public void Start()
    {
        SendPack(0x00, 0x00);
    }
}

public class GameLauncher
{
    public const string Render = "render.sock";
    public const string Game = "game.sock";

    private IntPtr buffer;
    private IntPtr texture;

    private GameSock Sock;

    public int TexId;

    public ushort Width, Height;
    public int RenderWidth { get; private set; }
    public int RenderHeight{ get; private set; }

    public bool HaveBuffer;
    public bool HaveTexture;

    private string render, game;

    public void BindTexture()
    {
        if (HaveTexture || !HaveBuffer)
        {
            return; 
        }
        if (TexId == 0)
        {
            TexId = EGLCore.CreateTexture();
        }
        HaveTexture = RenderTest.BindTexture(TexId, buffer, out var width, out var height, out texture);
        RenderWidth = width;
        RenderHeight = height; 
    }

    private void Command(byte data)
    {
        switch (data)
        {
            case 3:
                ReadBuffer();
                break;
        }
    }

    public void Start(Context context)
    {
        render = context.FilesDir.AbsolutePath + "/" + Render;
        game = context.FilesDir.AbsolutePath + "/" + Game;
        Sock = new(game)
        { 
            CommandRead = Command
        };
        var temp1 = Os.Getenv("PATH");

        // 获取文件描述符的整数值
        ProcessBuilder process = new(context.ApplicationInfo.NativeLibraryDir + "/" + "libcolormcnative.so");
        process.Environment().Add("GAME_SOCK", game);
        process.Environment().Add("RENDER_SOCK", render);
        process.Environment().Add("RUN_SO", "libcolormcnative_run.so");
        process.Environment().Add("GL_SO", "libgl4es_114.so");
        process.Environment().Add("EGL_SO", "libEGL.so");
        {
            var LD_LIBRARY_PATH = context.ApplicationInfo.NativeLibraryDir + ":" + temp1
                +":" + "/system/lib64";
            process.Environment().Add("LD_LIBRARY_PATH", LD_LIBRARY_PATH);
        }
        process.Environment().Add("LIBGL_MIPMAP", "3");

        // Prevent OptiFine (and other error-reporting stuff in Minecraft) from balooning the log
        process.Environment().Add("LIBGL_NOERROR", "1");

        // On certain GLES drivers, overloading default functions shader hack fails, so disable it
        process.Environment().Add("LIBGL_NOINTOVLHACK", "1");

        // Fix white color on banner and sheep, since GL4ES 1.1.5
        process.Environment().Add("LIBGL_NORMALIZE", "1");

        // The OPEN GL version is changed according
        process.Environment().Add("LIBGL_ES", "3");
        var fileDir = context.FilesDir;
        process.Directory(fileDir);
        process.RedirectErrorStream(true);
        RenderLog.Info("Pipe", "Start Pipe");
        var p = process.Start();
        new Thread(() =>
        {
            try
            {
                string? str;
                using var inputReader = new BufferedReader(new InputStreamReader(p.InputStream));
                while ((str = inputReader.ReadLine()) != null)
                {
                    RenderLog.Info("Pipe", str);
                }
            }
            catch(Exception e)
            { 
                
            }
        }).Start();
        if (!Sock.Connect())
        {
            return;
        }
        Sock.SetWindowSize(Width, Height);
        Sock.Start();
        ReadBuffer();
    }

    internal void SetSize()
    {
        HaveBuffer = false;
        HaveTexture = false;
        RenderTest.DeleteBuffer(buffer, texture);
        EGLCore.DeleteTexture(TexId);
        TexId = 0;
        Sock.SetWindowSize(Width, Height);
        Sock.ChangeSize();
    }

    private void ReadBuffer()
    {
        Task.Run(() =>
        {
            string name1 = render;
            while (true)
            {
                buffer = RenderTest.GetBuffer(name1);
                if (buffer != IntPtr.Zero)
                {
                    HaveBuffer = true;
                    return;
                }
                else
                {
                    Thread.Sleep(500);
                }
            }
        });
    }
}