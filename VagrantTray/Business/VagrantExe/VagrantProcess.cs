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
            {Command.Destroy, "destroy"}
        };

        protected VagrantProcess(VagrantInstance instance, Command command)
        {
            Instance = instance;
            _command = command;

            var startInfo = new ProcessStartInfo
            {
                FileName = "vagrant.exe",
                Arguments = CommandArguments[command],
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

        public Command Command
        {
            get { return _command; }
        }

        protected abstract void OnExited(object sender, EventArgs eventArgs);
    }

    public enum Command
    {
        Status,
        Up,
        Reload,
        Suspend,
        Resume,
        Halt,
        Destroy
    }
}
