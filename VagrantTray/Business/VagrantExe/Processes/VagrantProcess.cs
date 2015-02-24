using System.Collections.Generic;
using System.Diagnostics;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes
{
    [System.ComponentModel.DesignerCategory("Code")]
    public abstract class VagrantProcess : Process
    {
        protected VagrantInstance Instance;
        private readonly List<string> _outputData;
        private readonly List<string> _errorData;
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

            _outputData = new List<string>();
            _errorData = new List<string>();

            var startInfo = new ProcessStartInfo
            {
                FileName = "vagrant.exe",
                Arguments = CommandArguments[command],
                WorkingDirectory = instance.Directory,
                UseShellExecute = false
            };

            if (!showWindow)
            {
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                startInfo.CreateNoWindow = true;
                
                EnableRaisingEvents = true;

                OutputDataReceived += OnOutputDataReceived;
                ErrorDataReceived += OnErrorDataReceived;
            }

            if (Properties.Settings.Default.RunAsAdministrator)
            {
                startInfo.Verb = "runas";
            }
            
            StartInfo = startInfo;
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            HandleDataReceived(dataReceivedEventArgs.Data, false);
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            HandleDataReceived(dataReceivedEventArgs.Data, true);
        }

        private void HandleDataReceived(string data, bool isErrorData)
        {
            if (data != null)
            {
                var destination = isErrorData ? ErrorData : OutputData;
                destination.Add(data.Trim());
            }
            else
            {
                CompleteProcess(isErrorData);
            }
        }

        public Command Command
        {
            get { return _command; }
        }

        public List<string> OutputData
        {
            get { return _outputData; }
        }

        public List<string> ErrorData
        {
            get { return _errorData; }
        } 

        /// <summary>
        /// Do actions that depend on the process execution being complete.
        /// </summary>
        /// <param name="errorOccurred">Whether or not the process experienced errors during execution.</param>
        protected virtual void CompleteProcess(bool errorOccurred)
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
