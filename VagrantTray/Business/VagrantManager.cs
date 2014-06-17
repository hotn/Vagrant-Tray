using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using MikeWaltonWeb.VagrantTray.Business.VagrantExe;
using MikeWaltonWeb.VagrantTray.Model;
using MikeWaltonWeb.VagrantTray.UI;

namespace MikeWaltonWeb.VagrantTray.Business
{
    public class VagrantManager
    {
        private VagrantProcess _process;
        private List<string> _outputLines = new List<string>();

        private List<VagrantInstance> _instances = new List<VagrantInstance>();

        private VagrantSystemTrayMenu _menu;

        private readonly List<string> _messages = new List<string>();

        private static VagrantManager _instance;

        private ApplicationData _applicationData;

        private SettingsManager _settingsManager;

        private VagrantManager()
        {
            Init();
        }

        public static VagrantManager CreateInstance()
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("Cannot create more than one instance of VagrantManager");
            }

            _instance = new VagrantManager();

            return _instance;
        }

        private void Init()
        {
            LoadApplicationData();

            _settingsManager = new SettingsManager(_applicationData);

            _menu = new VagrantSystemTrayMenu();

            //_menu.SettingsClicked += (sender, args) => RebuildList();
            _menu.SettingsClicked += (sender, args) => _settingsManager.ShowSettings();
            _menu.ExitClicked += (sender, args) => TerminateApplication();

            RebuildList();
        }

        private void LoadApplicationData()
        {
            _applicationData = new ApplicationData();

            _applicationData.Bookmarks = LoadBookmarks();
        }

        private static List<Bookmark> LoadBookmarks()
        {
            using (var ms = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.SavedBookmarks)))
            {
                if (ms.Length != 0)
                {
                    var bf = new BinaryFormatter();
                    return (List<Bookmark>) bf.Deserialize(ms);
                }
            }

            return new List<Bookmark>();
        }

        private void RebuildList()
        {
            _menu.Reset();

            var instances = GetInstances();
            foreach (var vagrantInstance in instances)
            {
                _menu.AddInstanceSubmenu(vagrantInstance, GetInstanceCommandActions(vagrantInstance));
            }
        }

        private Dictionary<VagrantProcess.Command, Action> GetInstanceCommandActions(VagrantInstance instance)
        {
            return
                Enum.GetNames(typeof(VagrantProcess.Command))
                    .ToDictionary(name => (VagrantProcess.Command)Enum.Parse(typeof(VagrantProcess.Command), name),
                        name =>
                            GetActionForVagrantInstanceCommand(instance,
                                (VagrantProcess.Command)Enum.Parse(typeof(VagrantProcess.Command), name)));
        }

        private void CreateProcess()
        {
            if (_process != null)
            {
                _process.Close();
                _process.Dispose();
            }

            _process = new VagrantProcess();
            
        }

        private void GetGlobalStatus()
        {
            CreateProcess();

            _process.VagrantCommand = VagrantProcess.Command.GlobalStatus;

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

        private void ProcessOnCommandDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            //Console.WriteLine(dataReceivedEventArgs.Data);
            Console.WriteLine(_messages);
            _messages.Add(dataReceivedEventArgs.Data);

            ShowTrayTip();
        }

        private void ProcessOnCommandExited(object sender, EventArgs dataReceivedEventArgs)
        {
            Console.WriteLine("Command complete");
            _messages.Add("Command complete");

            ShowTrayTip();

            _messages.Clear();
        }

        private void ShowTrayTip()
        {
            while (_messages.Sum(m => m == null ? 0 : m.Length) + _messages.Count > 255)
            {
                _messages.RemoveAt(0);
            }

            var balloonMessage = String.Join(Environment.NewLine, _messages);

            if (balloonMessage.Equals(String.Empty))
            {
                return;
            }

            _menu.ShowMessageBalloon(balloonMessage);
        }

        private void RunInstanceCommand(VagrantInstance instance, VagrantProcess.Command command)
        {
            CreateProcess();

            _process.Instance = instance;
            _process.VagrantCommand = command;

            _process.OutputDataReceived += ProcessOnCommandDataReceived;
            _process.ErrorDataReceived += ProcessOnErrorDataReceived;
            _process.Exited += ProcessOnCommandExited;


            _process.Start();
            try
            {
                _process.BeginOutputReadLine();
            }
            catch (Exception) { }
            
            _process.WaitForExit();
        }

        private void TerminateApplication()
        {
            _menu.Dispose();
            Application.Current.Shutdown();
        }

        public List<VagrantInstance> GetInstances()
        {
            _instances = new List<VagrantInstance>();
            GetGlobalStatus();
            return _instances;
        }

        public Action GetActionForVagrantInstanceCommand(VagrantInstance instance, VagrantProcess.Command command)
        {
            return () =>
            {
                Console.WriteLine("Command for " + command + " " + instance.Id);
                RunInstanceCommand(instance, command);
                RunInstanceCommand(instance, VagrantProcess.Command.Status);
            };
        }
    }
}
