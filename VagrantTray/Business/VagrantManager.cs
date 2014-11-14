using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using AutoUpdaterDotNET;
using GalaSoft.MvvmLight.Command;
using MikeWaltonWeb.VagrantTray.Business.Utility.Comparers;
using MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes;
using MikeWaltonWeb.VagrantTray.Model;
using MikeWaltonWeb.VagrantTray.Properties;
using MikeWaltonWeb.VagrantTray.UI;
using MikeWaltonWeb.VagrantTray.UI.Tray;
using MikeWaltonWeb.VagrantTray.ViewModel;

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

        private ProcessesWindow _processesWindow;

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
            CheckForUpdates();

            CheckForSettingsUpgrades();

            LoadApplicationData();

            _settingsManager = new SettingsManager(_applicationData);

            _menu = new VagrantSystemTrayMenu();

            _menu.SettingsClicked += (sender, args) => _settingsManager.ShowSettingsWindow();
            _menu.ExitClicked += (sender, args) => TerminateApplication();
            _menu.TrayIconClicked += (sender, args) =>
            {
                string message;
                if (_applicationData.Bookmarks.Select(b => b.VagrantInstance).Count(i => i.CurrentProcess != null) == 0)
                {
                    message = "No processes running.";
                    _menu.ShowMessageBalloon(message);
                }
                else
                {
                    message = "Running processes:" + Environment.NewLine + Environment.NewLine +
                              String.Join(Environment.NewLine,
                                  _applicationData.Bookmarks.Where(b => b.VagrantInstance.CurrentProcess != null).Select(b => b.Name + ": " + b.VagrantInstance.CurrentProcess.Command)) +
                              Environment.NewLine + Environment.NewLine + "Click for full output popup.";

                    _menu.ShowMessageBalloon(message, () =>
                    {
                        if (_processesWindow == null)
                        {
                            _processesWindow = new ProcessesWindow
                            {
                                DataContext = new ProcessesViewModel(_applicationData)
                                {
                                    CloseCommand = new RelayCommand(() => _processesWindow.Close())
                                }
                            };
                        }

                        _processesWindow.Show();
                    });
                }
            };

            RebuildList();

            _applicationData.Bookmarks.CollectionChanged += (sender, args) => RebuildList();
        }

        private static void CheckForUpdates()
        {
            AutoUpdater.Start(Resources.UpdatesURL);
        }

        private static void CheckForSettingsUpgrades()
        {
            if (Properties.Settings.Default.UpgradeRequired)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
        }

        private void LoadApplicationData()
        {
            _applicationData = new ApplicationData {Bookmarks = LoadBookmarks()};
        }

        private static ObservableCollection<Bookmark> LoadBookmarks()
        {
            using (var ms = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.SavedBookmarks)))
            {
                if (ms.Length != 0)
                {
                    var bf = new BinaryFormatter();
                    var bookmarks = (ObservableCollection<Bookmark>) bf.Deserialize(ms);

                    //unify underlying vagrant instances so there are no duplicates
                    var instances =
                        bookmarks.Select(b => b.VagrantInstance)
                            .Distinct(new VagrantInstanceEqualityComparer())
                            .ToList();

                    foreach (var bookmark in bookmarks)
                    {
                        bookmark.VagrantInstance =
                            instances.First(
                                v => new VagrantInstanceEqualityComparer().Equals(v, bookmark.VagrantInstance));
                    }

                    return bookmarks;
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

            //refresh the status of each underlying vagrant instance
            if (_applicationData.Bookmarks.Count > 0)
            {
                _menu.StartWorkingAnimation();
            }

            foreach (var instance in _applicationData.Bookmarks.Select(b => b.VagrantInstance).Distinct(new VagrantInstanceEqualityComparer()))
            {
                var process = new VagrantStatusProcess(instance);

                var bookmarks =
                    _applicationData.Bookmarks.Where(
                        b => new VagrantInstanceEqualityComparer().Equals(b.VagrantInstance, instance)).ToList();

                foreach (var bookmark in bookmarks)
                {
                    bookmark.VagrantInstance.CurrentProcess = process;
                }

                var worker = new BackgroundWorker();
                worker.DoWork += (sender, args) =>
                {
                    process.Success += state =>
                    {
                        _mainApplication.Dispatcher.Invoke(() => instance.CurrentState = state);

                        foreach (var bookmark in bookmarks)
                        {
                            bookmark.VagrantInstance.CurrentProcess = null;
                        }

                        if (_applicationData.Bookmarks.Select(b => b.VagrantInstance).Count(i => i.CurrentProcess != null) == 0)
                        {
                            _menu.StopWorkingAnimation();
                        }

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
                _mainApplication.Dispatcher.Invoke(() =>
                {
                    bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Loading;
                });
                
                bookmark.VagrantInstance.CurrentProcess = process;

                _menu.StartWorkingAnimation();

                process.Start();
                process.Exited += (sender, args) =>
                {
                    bookmark.VagrantInstance.CurrentProcess = null;

                    if (_applicationData.Bookmarks.Select(b => b.VagrantInstance).Count(i => i.CurrentProcess != null) == 0)
                    {
                        _menu.StopWorkingAnimation();
                    }
                };

                try
                {
                    process.BeginOutputReadLine();
                }
                catch (Exception)
                {
                }

                process.WaitForExit();
            };

            Action<Action> initWorker = work =>
            {
                var worker = new BackgroundWorker();

                worker.DoWork += (sender, args) =>
                {
                    work.Invoke();
                };
                worker.RunWorkerAsync();
            };

            return new Dictionary<string, Action>
            {
                {
                    "Status", (() => initWorker(() =>
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
                    }))
                },
                {
                    "SSH", (() => initWorker(() =>
                    {
                        using (var process = new VagrantSshProcess(bookmark.VagrantInstance))
                        {
                            process.Start();
                        }
                    }))
                },
                {
                    "Up", (() => initWorker(() =>
                    {
                        using (var process = new VagrantUpProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender2, args2) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Running;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess)sender2).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

                            useProcess(process);
                        }
                    }))
                },
                {
                    "Reload", (() => initWorker(() =>
                    {
                        using (var process = new VagrantReloadProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender2, args2) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Running;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess)sender2).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

                            useProcess(process);
                        }
                    }))
                },
                {
                    "Suspend", (() => initWorker(() =>
                    {
                        using (var process = new VagrantSuspendProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender2, args2) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Saved;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess)sender2).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

                            useProcess(process);
                        }
                    }))
                },
                {
                    "Resume", (() => initWorker(() =>
                    {
                        using (var process = new VagrantResumeProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender2, args2) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Running;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess)sender2).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

                            useProcess(process);
                        }
                    }))
                },
                {
                    "Halt", (() => initWorker(() =>
                    {
                        using (var process = new VagrantHaltProcess(bookmark.VagrantInstance))
                        {
                            process.Success += (sender2, args2) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Poweroff;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess)sender2).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

                            useProcess(process);
                        }
                    }))
                },
                {
                    "Destroy", (() => initWorker(() =>
                    {
                        using (var process = new VagrantDestroyProcess(bookmark.VagrantInstance))
                        {
                            //TODO: this state isn't accurate
                            process.Success += (sender2, args2) => _mainApplication.Dispatcher.Invoke(() =>
                            {
                                bookmark.VagrantInstance.CurrentState = VagrantInstance.State.Poweroff;
                                _menu.ShowMessageBalloon(bookmark.Name + ": " + ((VagrantProcess)sender2).Command +
                                                         " complete." + Environment.NewLine + Environment.NewLine +
                                                         "Current state: " +
                                                         bookmark.VagrantInstance.CurrentState);
                            });

                            useProcess(process);
                        }
                    }))
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
