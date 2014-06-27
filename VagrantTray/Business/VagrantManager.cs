using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows;
using MikeWaltonWeb.VagrantTray.Business.VagrantExe;
using MikeWaltonWeb.VagrantTray.Model;
using MikeWaltonWeb.VagrantTray.UI;

namespace MikeWaltonWeb.VagrantTray.Business
{
    public class VagrantManager
    {
        private List<string> _outputLines = new List<string>();

        private List<VagrantInstance> _instances = new List<VagrantInstance>();

        private VagrantSystemTrayMenu _menu;

        private readonly List<string> _messages = new List<string>();

        private static VagrantManager _instance;

        private ApplicationData _applicationData;

        private SettingsManager _settingsManager;

        private Dictionary<Bookmark, VagrantProcess> _runningProcesses = new Dictionary<Bookmark, VagrantProcess>();

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

            _menu.SettingsClicked += (sender, args) => _settingsManager.ShowSettings();
            _menu.ExitClicked += (sender, args) => TerminateApplication();
            _menu.TrayIconClicked += (sender, args) =>
            {
                var message = String.Empty;
                if (_runningProcesses.Count == 0)
                {
                    message = "No processes running.";
                }
                else
                {
                    message = "Running processes:" + Environment.NewLine + Environment.NewLine +
                              String.Join(Environment.NewLine,
                                  _runningProcesses.Select(p => p.Key.Name + ": " + p.Value.Command));
                }

                _menu.ShowMessageBalloon(message);
            };

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

            //add each bookmark to the menu
            foreach (var bookmark in _applicationData.Bookmarks)
            {
                _menu.AddBookmarkSubmenu(bookmark, GetInstanceCommandActions(bookmark));
            }

            //refresh the status of each bookmark
            foreach (var bookmark in _applicationData.Bookmarks)
            {
                var process = new VagrantStatusProcess(bookmark.VagrantInstance);
                process.Success += state =>
                {
                    bookmark.VagrantInstance.CurrentState = state;
                    process.Dispose();
                };
                process.Start();
                try
                {
                    process.BeginOutputReadLine();
                }
                catch (Exception)
                {
                }

                process.WaitForExit();
            }
        }

        private Dictionary<string, Action> GetInstanceCommandActions(Bookmark bookmark)
        {
            Action<VagrantProcess> useProcess = process =>
            {
                _runningProcesses[bookmark] = process;

                process.Exited += (sender, args) => _runningProcesses.Remove(bookmark);

                process.Start();
                try
                {
                    process.BeginOutputReadLine();
                }
                catch (Exception)
                {
                }

                process.WaitForExit();
            };

            return new Dictionary<string, Action>
            {
                {
                    "Status", (() =>
                    {
                        var worker = new BackgroundWorker();

                        worker.DoWork += (sender, args) =>
                        {
                            using (var process = new VagrantStatusProcess(bookmark.VagrantInstance))
                            {
                                process.Success += state =>
                                {
                                    bookmark.VagrantInstance.CurrentState = state;
                                    _menu.ShowMessageBalloon(bookmark.Name + " current state: " +
                                                             bookmark.VagrantInstance.CurrentState.ToString());
                                };

                                useProcess(process);
                            }
                        };
                        worker.RunWorkerAsync();
                    })
                },
                {
                    "Up", (() =>
                    {
                        using (var process = new VagrantUpProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender, args) =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Running;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess) sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState.ToString());
                            };

                            useProcess(process);
                        }
                    })
                },
                {
                    "Reload", (() =>
                    {
                        using (var process = new VagrantReloadProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender, args) =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Running;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess)sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState.ToString());
                            };

                            useProcess(process);
                        }
                    })
                },
                {
                    "Suspend", (() =>
                    {
                        using (var process = new VagrantSuspendProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender, args) =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Saved;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess)sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState.ToString());
                            };

                            useProcess(process);
                        }
                    })
                },
                {
                    "Resume", (() =>
                    {
                        using (var process = new VagrantResumeProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender, args) =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Running;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess)sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState.ToString());
                            };

                            useProcess(process);
                        }
                    })
                },
                {
                    "Halt", (() =>
                    {
                        using (var process = new VagrantHaltProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender, args) =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Poweroff;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess)sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState.ToString());
                            };

                            useProcess(process);
                        }
                    })
                },
                {
                    "Destroy", (() =>
                    {
                        using (var process = new VagrantDestroyProcess(bookmark.VagrantInstance))
                        {
                            //TODO: this state isn't accurate
                            process.Success += (sender, args) =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Poweroff;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess)sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState.ToString());
                            };

                            useProcess(process);
                        }
                    })
                }
            };
        }

        private void GetGlobalStatus()
        {
            /*using (var process = new VagrantProcess())
            {
                process.VagrantCommand = VagrantProcess.Command.GlobalStatus;

                process.OutputDataReceived += ProcessOnGlobalStatusOutputDataReceived;
                process.ErrorDataReceived += ProcessOnErrorDataReceived;
                process.Exited += ProcessOnGlobalStatusExited;


                process.Start();

                try
                {
                    process.BeginOutputReadLine();
                }
                catch (Exception) { }
                process.WaitForExit();
            }*/
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
                    CurrentState = (VagrantInstance.State)Enum.Parse(typeof(VagrantInstance.State), datalist.ElementAt(3)),
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

        /*private void RunInstanceCommand(VagrantInstance instance, VagrantProcess.Command command,
            DataReceivedEventHandler outputDataReceivedAction,
            DataReceivedEventHandler errorDataReceivedAction,
            EventHandler exitedAction)
        {
            using (var process = new VagrantProcess())
            {
                process.Instance = instance;
                process.VagrantCommand = command;

                process.OutputDataReceived += outputDataReceivedAction;
                process.ErrorDataReceived += errorDataReceivedAction;
                process.Exited += exitedAction;


                process.Start();
                try
                {
                    process.BeginOutputReadLine();
                }
                catch (Exception)
                {
                }

                process.WaitForExit();
            }
        }*/

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

        /*public Action GetActionForVagrantInstanceCommand(VagrantInstance instance, VagrantProcess.Command command)
        {
            return () =>
            {
                Console.WriteLine("Command for " + command + " " + instance.Id);
                RunInstanceCommand(instance, command);
                RunInstanceCommand(instance, VagrantProcess.Command.Status);
            };
        }*/
    }
}
