using System;
using System.Linq;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes
{
    public class VagrantStatusProcess : VagrantProcess
    {
        public event OnSuccessHandler Success;
        public event EventHandler Fail;

        public delegate void OnSuccessHandler(VagrantInstance.State state);

        public VagrantStatusProcess(VagrantInstance instance)
            : base(instance, Command.Status)
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
                var statusLine = OutputData.ToList().SkipWhile(l => !l.Trim().Equals(String.Empty)).Skip(1).FirstOrDefault();
                var statusString = statusLine != null ? statusLine.Substring(statusLine.IndexOf(' ')).Trim() : "";
                statusString = statusString.Substring(0, statusString.LastIndexOf(' ')).Trim();

                VagrantInstance.State status;

                switch (statusString)
                {
                    case "poweroff":
                        status = VagrantInstance.State.Poweroff;
                        break;
                    case "running":
                        status = VagrantInstance.State.Running;
                        break;
                    case "saved":
                        status = VagrantInstance.State.Saved;
                        break;
                    case "not created":
                        status = VagrantInstance.State.NotCreated;
                        break;
                    default:
                        //TODO: It seems we do sometimes make it here when OnExited() gets called before OutputData has completely processed. Perhaps add a slight delay for OutputData to finish populating?
                        //Shouldn't ever make it here.
                        status = VagrantInstance.State.Unknown;
                        break;
                }

                Success(status);
            }
        }
    }
}
