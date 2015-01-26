using System;
using System.Linq;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes
{
    public class VagrantStatusProcess : VagrantProcess
    {
        public event OnSuccessHandler Success;

        public delegate void OnSuccessHandler(VagrantInstance.State state);

        public VagrantStatusProcess(VagrantInstance instance)
            : base(instance, Command.Status)
        {
        }

        protected override void OnExited(object sender, EventArgs eventArgs)
        {
            //TODO: announce errors

            if (Success != null)
            {
                var statusLine = OutputData.ToList().SkipWhile(l => !l.Trim().Equals(String.Empty)).Skip(1).First();
                var statusString = statusLine.Substring(statusLine.IndexOf(' ')).Trim();
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
                        status = VagrantInstance.State.Loading;
                        break;
                }

                //var status = (VagrantInstance.State) Enum.Parse(typeof (VagrantInstance.State), statusString);

                Success(status);
            }
        }
    }
}
