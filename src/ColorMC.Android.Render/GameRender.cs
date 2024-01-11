using Android.Content;
using Android.Systems;
using System.Diagnostics;
using System.Net.Sockets;

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
            RenderLog.Info("Game Sock", "connect fail wait...");
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

public enum GameRenderType
{ 
    gl4es = 0,
    angle = 1
}

public class GameRender
{
    private const string Render = "render.sock";
    private const string Game = "game.sock";

    private IntPtr buffer;
    private IntPtr texture;

    private GameSock Sock;

    public int TexId;

    public ushort Width, Height;
    public int RenderWidth { get; private set; }
    public int RenderHeight{ get; private set; }

    public bool HaveBuffer;
    public bool HaveTexture;

    public event Action<string>? GameReady;

    private string _render, _game;
    private string _uuid;

    public GameRender(string dir, string uuid, Process process, GameRenderType gameRender)
    {
        _uuid = uuid;

        _render = $"{dir}/{uuid}.{Render}";
        _game = $"{dir}/{uuid}.{Game}";
        Sock = new(_game)
        {
            CommandRead = Command
        };
        process.StartInfo.Environment.Add("GAME_SOCK", _game);
        process.StartInfo.Environment.Add("RENDER_SOCK", _render);
        process.StartInfo.Environment.Add("GL_SO", gameRender.GetFileName());
        process.StartInfo.Environment.Add("EGL_SO", "libEGL.so");
        process.StartInfo.Environment.Add("LIBGL_MIPMAP", "3");
        process.StartInfo.Environment.Add("LIBGL_NOERROR", "1");
        process.StartInfo.Environment.Add("LIBGL_NOINTOVLHACK", "1");
        process.StartInfo.Environment.Add("LIBGL_NORMALIZE", "1");
        process.StartInfo.Environment.Add("GL_ES_VERSION", "3");
        process.StartInfo.Environment.Add("GAME_RENDER", $"{(int)gameRender}");
    }

    public void BindTexture()
    {
        if (HaveTexture || !HaveBuffer)
        {
            return; 
        }
        if (TexId == 0)
        {
            TexId = GLHelper.CreateTexture();
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
            case 4:
                ReadBuffer();
                GameReady?.Invoke(_uuid);
                break;
        }
    }

    public void Start()
    {
        Task.Run(() =>
        {
            Thread.Sleep(10000);
            while (true)
            {
                Thread.Sleep(2000);
                if (Sock.Connect())
                {
                    break;
                }
            }
            Sock.Start();
        });
    }

    internal void SetSize()
    {
        HaveBuffer = false;
        HaveTexture = false;
        RenderTest.DeleteBuffer(buffer, texture);
        GLHelper.DeleteTexture(TexId);
        TexId = 0;
        Sock.SetWindowSize(Width, Height);
        Sock.ChangeSize();
    }

    private void ReadBuffer()
    {
        Task.Run(() =>
        {
            while (true)
            {
                buffer = RenderTest.GetBuffer(_render);
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
