using System;
using System.Diagnostics;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe
{
    public class VagrantProcess : Process
    {
        private VagrantInstance _instance;

        public VagrantProcess()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "vagrant.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            StartInfo = startInfo;
            EnableRaisingEvents = true;
        }

        public Command VagrantCommand
        {
            set
            {
                var args = "";

                switch (value)
                {
                    case Command.Up:
                    case Command.Reload:
                    case Command.Provision:
                    case Command.Suspend:
                    case Command.Resume:
                    case Command.Halt:
                    case Command.Destroy:
                    case Command.Status:
                        args = value.ToString().ToLower();
                        break;
                    case Command.GlobalStatus:
                        args = "global-status";
                        break;
                }

                if (_instance != null)
                {
                    args += " " + _instance.Id;
                }

                StartInfo.Arguments = args;
            }
        }

        public VagrantInstance Instance
        {
            get { return _instance; }
            set
            {
                _instance = value;
                if (!StartInfo.Arguments.Equals(String.Empty))
                {
                    StartInfo.Arguments += " " + value.Id;
                }
            }
        }

        public enum Command
        {
            Up,
            Reload,
            Provision,
            Suspend,
            Resume,
            Halt,
            Destroy,
            Status,
            GlobalStatus
        }
    }
}
