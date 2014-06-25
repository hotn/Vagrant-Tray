﻿using System;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe
{
    public class VagrantReloadProcess : VagrantProcess
    {
        public event EventHandler Success;

        public VagrantReloadProcess(VagrantInstance instance)
            : base(instance)
        {
            StartInfo.Arguments = "reload";
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