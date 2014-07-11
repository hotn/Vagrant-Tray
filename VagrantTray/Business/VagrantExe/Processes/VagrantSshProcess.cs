using MikeWaltonWeb.VagrantTray.Business.VagrantExe.Shells;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes
{
    public class VagrantSshProcess : VagrantProcess
    {
        public VagrantSshProcess(VagrantInstance instance) : base(instance, Command.Ssh, true)
        {
            //override process filename
            StartInfo.FileName = Properties.Settings.Default.ShellApplication;

            //override args
            StartInfo.Arguments = ShellCommandFactory.GenerateShellCommand(
                Properties.Settings.Default.ShellApplication, Command.Ssh);
        }
    }
}
