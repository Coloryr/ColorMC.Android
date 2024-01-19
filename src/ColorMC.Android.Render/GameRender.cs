using Android.Graphics;
using System.Diagnostics;
using System.Net.Sockets;
using static Java.Util.Jar.Attributes;

namespace ColorMC.Android.GLRender;

public class GameSock
{
    public enum CommandType : byte
    {
        Run = 0,
        SetSize = 1,
        DisplayReady = 2,
        SendChar = 3,
        SendCharMods = 4,
        SendCursorPos = 5,
        SendKey = 6,
        SendMouseButton = 7,
        SendScroll = 8,
        SetGrabbing = 9
    }

    private static readonly byte[] MagicHead = [(byte)'c', (byte)'o', (byte)'l', (byte)'o', (byte)'r', (byte)'y'];

    private readonly UnixDomainSocketEndPoint _socketPath;
    private readonly Socket _socket;

    public Action<CommandType, byte[], int>? CommandRead;

    public GameSock(string socketPath)
    {
        _socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

        _socketPath = new UnixDomainSocketEndPoint(socketPath);

    }

    public bool Connect()
    {
        try
        {
            _socket.Connect(_socketPath);

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
        try
        {
            var buffer = new byte[1024];
            while (_socket.Connected)
            {
                var size = await _socket.ReceiveAsync(buffer);
                if (size <= 0)
                {
                    return;
                }
                for (int a = 0; a < size - 6; a++)
                {
                    if (buffer[a] == MagicHead[0]
                        && buffer[a + 1] == MagicHead[1]
                        && buffer[a + 2] == MagicHead[2]
                        && buffer[a + 3] == MagicHead[3]
                        && buffer[a + 4] == MagicHead[4]
                        && buffer[a + 5] == MagicHead[5])
                    {
                        CommandRead?.Invoke((CommandType)buffer[a + 6], buffer, a + 7);
                    }
                }
            }
        }
        catch
        {

        }
    }

    public void ChangeSize(ushort width, ushort height)
    {
        var list = new List<byte>(MagicHead)
        {
            (byte)CommandType.SetSize,
        };
        list.AddRange(BitConverter.GetBytes(width));
        list.AddRange(BitConverter.GetBytes(height));

        _socket.Send(list.ToArray());
    }

    public void Start()
    {
        var list = new List<byte>(MagicHead)
        {
            (byte)CommandType.Run
        };
        _socket.Send(list.ToArray());
    }

    public void MouseKeyCode(int key, int mode, bool value)
    {
        var list = new List<byte>(MagicHead)
        {
            (byte)CommandType.SendMouseButton,
        };

        list.AddRange(BitConverter.GetBytes(key));
        list.AddRange(BitConverter.GetBytes(mode));
        list.Add(value ? (byte)1 : (byte)0);

        _socket.Send(list.ToArray());
    }

    public void CursorPos(float x, float y)
    {
        var list = new List<byte>(MagicHead)
        {
            (byte)CommandType.SendCursorPos,
        };

        list.AddRange(BitConverter.GetBytes(x));
        list.AddRange(BitConverter.GetBytes(y));

        _socket.Send(list.ToArray());
    }

    public void Close()
    {
        _socket?.Close();
        _socket?.Dispose();
    }

    public void SendScroll(float hScroll, float vScroll)
    {
        var list = new List<byte>(MagicHead)
        {
            (byte)CommandType.SendScroll,
        };

        list.AddRange(BitConverter.GetBytes(hScroll));
        list.AddRange(BitConverter.GetBytes(vScroll));

        _socket.Send(list.ToArray());
    }

    public void SendKeyPress(int key, int mode, bool value)
    {
        var list = new List<byte>(MagicHead)
        {
            (byte)CommandType.SendKey,
        };

        list.AddRange(BitConverter.GetBytes(key));
        list.AddRange(BitConverter.GetBytes(mode));
        list.Add(value ? (byte)1 : (byte)0);

        _socket.Send(list.ToArray());
    }
}

public class GameRender
{
    public enum RenderType
    {
        gl4es = 0,
        angle = 1,
        zink = 2
    }

    public enum DisplayType
    {
        None,
        Full,
        Scale
    }

    private const string Render = "render.sock";
    private const string Game = "game.sock";

    private IntPtr buffer;
    private IntPtr texture;

    private readonly GameSock Sock;

    private readonly string _render, _game;
    private readonly string _uuid;

    public int TexId { get; private set; }

    public ushort GameWidth { get; private set; }
    public ushort GameHeight { get; private set; }

    public string Name { get; init; }
    public Bitmap Icon { get; init; }


    private bool _isGrabbing;

    public bool IsGrabbing
    {
        get { return _isGrabbing; }
        set
        {
            _isGrabbing = value;
            IsGrabbingChange?.Invoke();
        }
    }

    public bool HaveBuffer { get; private set; }
    public bool HaveTexture { get; private set; }

    public event Action<string>? GameReady;
    public event Action? SizeChange;
    public event Action? IsGrabbingChange;
    public event Action? GameClose;

    public bool holdingAlt, holdingCapslock, holdingCtrl,
            holdingNumlock, holdingShift;

    public float MouseX, MouseY;

    public bool IsGameClose { get; private set; }

    public DisplayType ShowType = DisplayType.Scale;
    public bool FlipY;

    public GameRender(string dir, string uuid, string name,
        Bitmap icon, Process process, RenderType gameRender)
    {
        Icon = icon;
        Name = name;
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
        process.StartInfo.Environment.Add("GAME_RENDER", gameRender.GetName());
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
        HaveTexture = RenderNative.BindTexture(TexId, buffer, 
            out var width, out var height, out texture);
        GameWidth = (ushort)width;
        GameHeight = (ushort)height;
        SizeChange?.Invoke();
    }

    private void Command(GameSock.CommandType data, byte[] buffer, int index)
    {
        switch (data)
        {
            case GameSock.CommandType.SetSize:
                ReadBuffer();
                break;
            case GameSock.CommandType.DisplayReady:
                ReadBuffer();
                GameReady?.Invoke(_uuid);
                break;
            case GameSock.CommandType.SetGrabbing:
                IsGrabbing = buffer[index] == 1;
                break;
        }
    }

    public void Start()
    {
        Task.Run(() =>
        {
            Thread.Sleep(3000);
            while (true)
            {
                Thread.Sleep(1000);
                if (Sock.Connect())
                {
                    break;
                }
            }
            Sock.Start();
        });
    }

    public void SetSize(ushort width, ushort height)
    {
        if (IsGameClose)
        {
            return;
        }
        RenderClose();
        Sock.ChangeSize(width, height);
    }

    public void SendCursorPos(float x, float y)
    {
        if (IsGameClose)
        {
            return;
        }
        MouseX = x;
        MouseY = y;
        Sock.CursorPos(x, y);
    }

    public void SendCursorPos()
    {
        SendCursorPos(MouseX, MouseY);
    }

    private void ReadBuffer()
    {
        Task.Run(() =>
        {
            while (true)
            {
                if (IsGameClose)
                {
                    return;
                }
                buffer = RenderNative.GetBuffer(_render);
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

    public void MouseEvent(int key, bool value)
    {
        if (IsGameClose)
        {
            return;
        }
        Sock.MouseKeyCode(key, GetCurrentMods(), value);
    }

    private int GetCurrentMods()
    {
        int currMods = 0;
        if (holdingAlt)
        {
            currMods |= LwjglKeycode.GLFW_MOD_ALT;
        }
        if (holdingCapslock)
        {
            currMods |= LwjglKeycode.GLFW_MOD_CAPS_LOCK;
        }
        if (holdingCtrl)
        {
            currMods |= LwjglKeycode.GLFW_MOD_CONTROL;
        }
        if (holdingNumlock)
        {
            currMods |= LwjglKeycode.GLFW_MOD_NUM_LOCK;
        }
        if (holdingShift)
        {
            currMods |= LwjglKeycode.GLFW_MOD_SHIFT;
        }
        return currMods;
    }

    private void RenderClose()
    {
        HaveBuffer = false;
        HaveTexture = false;
        RenderNative.DeleteBuffer(buffer, texture);
        GLHelper.DeleteTexture(TexId);
        TexId = 0;
    }

    public void Close()
    {
        IsGameClose = true;
        Sock.Close();
        GameClose?.Invoke();
        AndroidHelper.Main.Post(RenderClose);
    }

    public void SendScroll(float hScroll, float vScroll)
    {
        if (IsGameClose)
        {
            return;
        }
        Sock.SendScroll(hScroll, vScroll);
    }

    public void SendKeyPress(short key)
    {
        Sock.SendKeyPress(key, GetCurrentMods(), true);
        Sock.SendKeyPress(key, GetCurrentMods(), false);
    }

    public void SendKey(short key, bool value)
    {
        Sock.SendKeyPress(key, GetCurrentMods(), value);
    }
}
