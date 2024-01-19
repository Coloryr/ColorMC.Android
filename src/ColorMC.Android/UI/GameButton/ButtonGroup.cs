using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.UI.GameButton;

public record ButtonGroup
{
    public string Name { get; set; }

    public List<ButtonData> Buttons { get; set; }
}
