using System;
using System.Windows.Forms;

namespace VagrantTray
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.Run(new TrayForm());
        }
    }
}
