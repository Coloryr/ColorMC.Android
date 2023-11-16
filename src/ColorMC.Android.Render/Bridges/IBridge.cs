using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Android.Render.Bridges;

public interface IBridge<T> where T : BasicRenderWindow
{
    public bool Init();
    public T? InitContext(T share);
    public void MakeCurrent(T bundle);
    public T? GetCurrent();
    public void SwapBuffers();
    public void SetupWindow();
    public void WwapInterval(int swapInterval);
}
