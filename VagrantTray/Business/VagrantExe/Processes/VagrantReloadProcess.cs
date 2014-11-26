using System;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes
{
    public class VagrantReloadProcess : VagrantProcess, IVagrantEventProcess
    {
        public event EventHandler Success;
        public event EventHandler Fail;

        public VagrantReloadProcess(VagrantInstance instance)
            : base(instance, Command.Reload)
        {
        }

        protected override void OnExited(object sender, EventArgs eventArgs)
        {
            //TODO: announce errors

            if (Success != null)
            {
                Success(this, EventArgs.Empty);
            }
        }
    }
}