using System;
using System.Collections.Generic;
using System.Diagnostics;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe
{
    [System.ComponentModel.DesignerCategory("Code")]
    public abstract class VagrantProcess : Process
    {
        protected VagrantInstance Instance;
        protected List<string> OutputData = new List<string>();
        protected List<string> ErrorData = new List<string>();
        private readonly Command _command;

        private static readonly Dictionary<Command, string> CommandArguments = new Dictionary<Command, string>
        {
            {Command.Status, "status"},
            {Command.Up, "up"},
            {Command.Reload, "reload"},
            {Command.Suspend, "suspend"},
            {Command.Resume, "resume"},
            {Command.Halt, "halt"},
            {Command.Destroy, "destroy"},
            {Command.Ssh, "ssh"}
        };

        protected VagrantProcess(VagrantInstance instance, Command command, bool showWindow = false)
        {
            Instance = instance;
            _command = command;

            var startInfo = new ProcessStartInfo
            {
                FileName = "vagrant.exe",
                Arguments = CommandArguments[command],
                WorkingDirectory = instance.Directory
            };

            if (!showWindow)
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                
                EnableRaisingEvents = true;

                OutputDataReceived += OnOutputDataReceived;
                ErrorDataReceived += OnErrorDataReceived;
                Exited += OnExited;
            }
            
            StartInfo = startInfo;
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

        public Command Command
        {
            get { return _command; }
        }

        protected virtual void OnExited(object sender, EventArgs eventArgs)
        {
            //do nothing by default and leave it up to subclasses to override
        }
    }

    public enum Command
    {
        Status,
        Up,
        Reload,
        Suspend,
        Resume,
        Halt,
        Destroy,
        Ssh
    }
}
