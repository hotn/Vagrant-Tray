using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private VagrantSystemTrayMenu _menu;

        private readonly List<string> _messages = new List<string>();

        private static VagrantManager _instance;

        private readonly App _mainApplication;

        private ApplicationData _applicationData;

        private SettingsManager _settingsManager;

        private readonly Dictionary<Bookmark, VagrantProcess> _runningProcesses = new Dictionary<Bookmark, VagrantProcess>();

        private VagrantManager(App mainApplication)
        {
            _mainApplication = mainApplication;

            Init();
        }

        public static VagrantManager CreateInstance(App mainApplication)
        {
            if (_instance != null)
            {
                throw new InvalidOperationException("Cannot create more than one instance of VagrantManager");
            }

            _instance = new VagrantManager(mainApplication);

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

            _applicationData.Bookmarks.CollectionChanged += (sender, args) => RebuildList();
        }

        private void LoadApplicationData()
        {
            _applicationData = new ApplicationData();

            _applicationData.Bookmarks = LoadBookmarks();
        }

        private static ObservableCollection<Bookmark> LoadBookmarks()
        {
            using (var ms = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.SavedBookmarks)))
            {
                if (ms.Length != 0)
                {
                    var bf = new BinaryFormatter();
                    //return new ObservableCollection<Bookmark>((List<Bookmark>)bf.Deserialize(ms));
                    return (ObservableCollection<Bookmark>) bf.Deserialize(ms);
                }
            }

            return new ObservableCollection<Bookmark>();
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
                var worker = new BackgroundWorker();
                worker.DoWork += (sender, args) =>
                {
                    var process = new VagrantStatusProcess(bookmark.VagrantInstance);
                    process.Success += state =>
                    {
                        _mainApplication.Dispatcher.Invoke(() => bookmark.VagrantInstance.CurrentState = state);
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
                };
                worker.RunWorkerAsync();
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
                                process.Success += state => _mainApplication.Dispatcher.Invoke(() =>
                                {
                                    bookmark.VagrantInstance.CurrentState = state;
                                    _menu.ShowMessageBalloon(bookmark.Name + " current state: " +
                                                             bookmark.VagrantInstance.CurrentState);
                                });

                                useProcess(process);
                            }
                        };
                        worker.RunWorkerAsync();
                    })
                },
                {
                    "SSH", (() =>
                    {
                        var worker = new BackgroundWorker();

                        worker.DoWork += (sender, args) =>
                        {
                            using (var process = new VagrantSshProcess(bookmark.VagrantInstance))
                            {
                                process.Start();
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
                            process.Success += (sender, args) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Running;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess) sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

                            useProcess(process);
                        }
                    })
                },
                {
                    "Reload", (() =>
                    {
                        using (var process = new VagrantReloadProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender, args) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Running;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess) sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

                            useProcess(process);
                        }
                    })
                },
                {
                    "Suspend", (() =>
                    {
                        using (var process = new VagrantSuspendProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender, args) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Saved;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess) sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

                            useProcess(process);
                        }
                    })
                },
                {
                    "Resume", (() =>
                    {
                        using (var process = new VagrantResumeProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender, args) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Running;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess) sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

                            useProcess(process);
                        }
                    })
                },
                {
                    "Halt", (() =>
                    {
                        using (var process = new VagrantHaltProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender, args) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Poweroff;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess) sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

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
                            process.Success += (sender, args) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Poweroff;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess) sender).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

                            useProcess(process);
                        }
                    })
                }
            };
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

        private void TerminateApplication()
        {
            _menu.Dispose();
            Application.Current.Shutdown();
        }
    }
}
