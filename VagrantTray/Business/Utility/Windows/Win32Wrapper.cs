using System;
using System.Windows;
using System.Windows.Interop;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace MikeWaltonWeb.VagrantTray.Business.Utility.Windows
{
    public class Win32Wrapper : IWin32Window
    {
        public Win32Wrapper(Window window)
        {
            Handle = new WindowInteropHelper(window).Handle;
        }

        public IntPtr Handle { get; private set; }
    }
}
