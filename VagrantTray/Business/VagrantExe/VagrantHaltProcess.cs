using System;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe
{
    public class VagrantHaltProcess : VagrantProcess
    {
        public event EventHandler Success;

        public VagrantHaltProcess(VagrantInstance instance)
            : base(instance)
        {
            StartInfo.Arguments = "halt";
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