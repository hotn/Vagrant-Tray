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
            
        }

        private void CreateProcess()
        {
            if (_process != null)
            {
                _process.Close();
                _process.Dispose();
            }

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

        private void GetGlobalStatus()
        {
            CreateProcess();

            _process.StartInfo.Arguments = "global-status";

            _process.OutputDataReceived += ProcessOnGlobalStatusOutputDataReceived;
            _process.ErrorDataReceived += ProcessOnErrorDataReceived;
            _process.Exited += ProcessOnGlobalStatusExited;
            

            _process.Start();
            try
            {
                _process.BeginOutputReadLine();
            }
            catch (Exception) { }
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
                var datalist = new List<string>(data).Where(d => !d.Trim().Equals(String.Empty)).ToList();
                _instances.Add(new VagrantInstance
                {
                    Id = datalist.ElementAt(0),
                    Name = datalist.ElementAt(1),
                    Provider = datalist.ElementAt(2),
                    State = datalist.ElementAt(3),
                    Directory = datalist.ElementAt(4)
                });
            }

            _outputLines = new List<string>();
        }

        private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            Console.WriteLine("Error: " + dataReceivedEventArgs.Data);
        }

        public List<VagrantInstance> GetInstances()
        {
            _instances = new List<VagrantInstance>();
            GetGlobalStatus();
            return _instances;
        }

        private void RunInstanceCommand(string args)
        {
            CreateProcess();

            _process.StartInfo.Arguments = args;

            _process.OutputDataReceived += (sender, eventArgs) => Console.WriteLine(eventArgs.Data);
            _process.ErrorDataReceived += ProcessOnErrorDataReceived;
            _process.Exited += (sender, eventArgs) => Console.WriteLine("Command complete");


            _process.Start();
            try
            {
                _process.BeginOutputReadLine();
            }
            catch (Exception) { }
            _process.WaitForExit();
        }

        public Action GetActionForVagrantInstanceCommand(VagrantInstance instance, VagrantCommand command)
        {
            return () =>
            {
                Console.WriteLine("Command for " + command + " " + instance.Id);
                RunInstanceCommand(command.ToString().ToLower() + " " + instance.Id);
                RunInstanceCommand(VagrantCommand.Status.ToString().ToLower() + " " + instance.Id);
            };
        }
    }
}
