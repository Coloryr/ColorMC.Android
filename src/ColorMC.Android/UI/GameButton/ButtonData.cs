using Avalonia;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.UI.GameButton;

public record MarginData
{
    public int Left { get; set; }
    public int Top { get; set; }
    public int Right { get; set; }
    public int Bottom { get; set; }

    public MarginData() { }

    public MarginData(int value) 
    {
        Left = value;
        Top = value;
        Right = value;
        Bottom = value;
    }

    public MarginData(int left, int top, int right,int bottom) 
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }
}

public record ButtonData
{
    public enum ButtonType
    {
        Setting, LastGroup, NextGroup, LoopGroup
    }

    public ButtonType Type { get; set; }
    public string Content { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public HorizontalAlignment Horizontal { get; set; }
    public VerticalAlignment Vertical { get; set; }
    public MarginData Margin { get; set; }
    public string BackGroud { get; set; }
    public string Foreground { get; set; }
    public string Image { get; set; }
    public float TextSize { get; set; }
    public float Alpha { get; set; }
}
