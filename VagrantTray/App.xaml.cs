using System.Windows;
using MikeWaltonWeb.VagrantTray.Business;

namespace MikeWaltonWeb.VagrantTray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            VagrantManager.CreateInstance(this);
        }
    }
}
