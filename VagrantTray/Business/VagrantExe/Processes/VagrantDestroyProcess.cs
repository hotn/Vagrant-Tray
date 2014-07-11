using System;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes
{
    public class VagrantDestroyProcess : VagrantProcess
    {
        public event EventHandler Success;

        public VagrantDestroyProcess(VagrantInstance instance)
            : base(instance, Command.Destroy)
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