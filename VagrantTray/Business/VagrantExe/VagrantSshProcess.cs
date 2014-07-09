using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe
{
    public class VagrantSshProcess : VagrantProcess
    {
        public VagrantSshProcess(VagrantInstance instance) : base(instance, Command.Ssh, true)
        {
        }
    }
}
