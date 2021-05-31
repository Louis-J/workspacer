using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace workspacer
{
    public interface IWindowsDeferPosHandle : IDisposable
    {
        void DeferWindowPos(Window window, WindowLocation location);
    }
}
