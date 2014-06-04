using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VagrantTray
{
    public class VagrantManager
    {
        private Process _process;
        private List<string> _outputLines = new List<string>();

        private List<VagrantInstance> _instances = new List<VagrantInstance>(); 

        public VagrantManager()
        {
            Init();
        }

        private void Init()
        {
            _process = new Process();
            var startInfo = new ProcessStartInfo
            {
                FileName = "vagrant.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            _process.StartInfo = startInfo;
            _process.EnableRaisingEvents = true;
        }

        public List<VagrantInstance> GetInstances()
        {
            _instances = new List<VagrantInstance>();
            GetGlobalStatus();
            return _instances;
        }

        private void GetGlobalStatus()
        {
            _process.StartInfo.Arguments = "global-status";

            _process.OutputDataReceived += ProcessOnGlobalStatusOutputDataReceived;
            _process.ErrorDataReceived += ProcessOnErrorDataReceived;
            _process.Exited += ProcessOnGlobalStatusExited;
            

            _process.Start();
            _process.BeginOutputReadLine();
            _process.WaitForExit();
        }

        private void ProcessOnGlobalStatusOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (dataReceivedEventArgs.Data != null)
            {
                _outputLines.Add(dataReceivedEventArgs.Data.Trim());
            }
        }

        private void ProcessOnGlobalStatusExited(object sender, EventArgs eventArgs)
        {
            var instanceLines = _outputLines.SkipWhile(l => !l.Contains("-----")).Skip(1).TakeWhile(l => !l.Equals(String.Empty));

            foreach (var line in instanceLines)
            {
                var data = line.Split(' ');
                _instances.Add(new VagrantInstance
                {
                    Id = data[0],
                    Name = data[1],
                    Provider = data[2],
                    State = data[3],
                    Directory = data[4]
                });
            }

            _outputLines = new List<string>();
        }

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            Console.WriteLine("Error: " + dataReceivedEventArgs.Data);
        }
    }
}
