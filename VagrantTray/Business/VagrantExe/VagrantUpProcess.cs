using System;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe
{
    public class VagrantUpProcess : VagrantProcess
    {
        public event EventHandler Success;

        public VagrantUpProcess(VagrantInstance instance) : base(instance, Command.Up)
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
