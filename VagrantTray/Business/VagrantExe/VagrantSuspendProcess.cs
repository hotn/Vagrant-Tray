using System;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe
{
    public class VagrantSuspendProcess : VagrantProcess
    {
        public event EventHandler Success;

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
