using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.UI.GameButton;

public record ButtonLayout
{
    public string Name { get; set; }
    public List<ButtonGroup> Groups { get; set; }
    public string MainGroup { get; set; }

    public static ButtonLayout GenDefault()
    {
        return new()
        {
            Name = "Default",
            Groups =
            [
                new()
                {
                    Name = "Group1",
                    Buttons =
                    [
                        new()
                        {
                            Type = ButtonData.ButtonType.Setting,
                            Width = 50,
                            Height = 50,
                            Horizontal = HorizontalAlignment.Right,
                            Vertical = VerticalAlignment.Top,
                            Margin = new(5),
                            BackGroud = "#343434",
                            Alpha = 1
                        },
                        new()
                        {
                            Type = ButtonData.ButtonType.LastGroup,
                            Width = 50,
                            Height = 50,
                            Horizontal = HorizontalAlignment.Right,
                            Vertical = VerticalAlignment.Bottom,
                            Margin = new(5, 5, 60, 5),
                            TextSize = 40,
                            Content = "«",
                            BackGroud = "#343434",
                            Foreground = "#FFFFFF",
                            Alpha = 0.5f
                        },
                        new()
                        {
                            Type = ButtonData.ButtonType.NextGroup,
                            Width = 50,
                            Height = 50,
                            Horizontal = HorizontalAlignment.Right,
                            Vertical = VerticalAlignment.Bottom,
                            Margin = new(5),
                            TextSize = 40,
                            Content = "»",
                            BackGroud = "#343434",
                            Foreground = "#FFFFFF",
                            Alpha = 0.5f
                        }
                    ]
                }
            ],
            MainGroup = "Group1"
        };
    }
}
