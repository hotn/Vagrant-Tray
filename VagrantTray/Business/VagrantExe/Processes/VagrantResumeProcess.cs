using System;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes
{
    public class VagrantResumeProcess : VagrantProcess, IVagrantEventProcess
    {
        public event EventHandler Success;
        public event EventHandler Fail;

        public VagrantResumeProcess(VagrantInstance instance)
            : base(instance, Command.Resume)
        {
        }

        protected override void CompleteProcess(bool errorOccurred)
        {
            if (errorOccurred && Fail != null)
            {
                Fail(this, EventArgs.Empty);
            }
            else if (Success != null)
            {
                Success(this, EventArgs.Empty);
            }
        }
    }
}