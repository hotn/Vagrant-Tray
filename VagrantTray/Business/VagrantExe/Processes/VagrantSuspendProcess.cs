using System;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes
{
    public class VagrantSuspendProcess : VagrantProcess, IVagrantEventProcess
    {
        public event EventHandler Success;
        public event EventHandler Fail;

        public VagrantSuspendProcess(VagrantInstance instance) : base(instance, Command.Suspend)
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
