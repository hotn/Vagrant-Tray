using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Documents;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe
{
    [System.ComponentModel.DesignerCategory("Code")]
    public abstract class VagrantProcess : Process
    {
        protected VagrantInstance Instance;
        protected List<string> OutputData = new List<string>();
        protected List<string> ErrorData = new List<string>();

        public VagrantProcess(VagrantInstance instance)
        {
            Instance = instance;

            var startInfo = new ProcessStartInfo
            {
                FileName = "vagrant.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = instance.Directory
            };
            
            StartInfo = startInfo;
            EnableRaisingEvents = true;

            OutputDataReceived += OnOutputDataReceived;
            ErrorDataReceived += OnErrorDataReceived;
            Exited += OnExited;
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (dataReceivedEventArgs.Data != null)
            {
                OutputData.Add(dataReceivedEventArgs.Data.Trim());
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (dataReceivedEventArgs.Data != null)
            {
                ErrorData.Add(dataReceivedEventArgs.Data.Trim());
            }
        }

        protected abstract void OnExited(object sender, EventArgs eventArgs);
    }
}
