﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Forms;
using GalaSoft.MvvmLight.Command;
using MikeWaltonWeb.VagrantTray.Business.Utility.Comparers;
using MikeWaltonWeb.VagrantTray.Business.Utility.Windows;
using MikeWaltonWeb.VagrantTray.Model;
using MikeWaltonWeb.VagrantTray.UI;
using MikeWaltonWeb.VagrantTray.ViewModel;
using MessageBox = System.Windows.Forms.MessageBox;

namespace MikeWaltonWeb.VagrantTray.Business
{
    public class SettingsManager
    {
        private SettingsWindow _settingsWindow;
        private BookmarkSettingsWindow _bookmarkWindow;

        /// <summary>
        /// Application data used by main application.
        /// </summary>
        private readonly ApplicationData _applicationData;

        /// <summary>
        /// Application data that is manipulated during settings changes
        /// </summary>
        private ApplicationData _tempApplicationData;

        public SettingsManager(ApplicationData applicationData)
        {
            _applicationData = applicationData;
            ResetSettings();
        }

        private void ResetSettings()
        {
            _tempApplicationData = new ApplicationData
            {
                Bookmarks =
                    new ObservableCollection<Bookmark>(
                        _applicationData.Bookmarks.Select(
                            b => b.Clone()))
            };
        }

        public void ShowSettingsWindow()
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow();
                
                var cancelCommand = new RelayCommand(() =>
                {
                    Properties.Settings.Default.Reload();
                    DestroySettingsWindow();
                    ResetSettings();
                });

                var settingsViewModel = new SettingsViewModel(_tempApplicationData)
                {
                    CancelCommand = cancelCommand,
                    CloseCommand = cancelCommand,
                    OkCommand = new RelayCommand(() =>
                    {
                        SaveSettings();
                        DestroySettingsWindow();
                    }),
                    EditBookmarkCommand = new RelayCommand<BookmarkViewModel>(EditBookmark),
                    DeleteBookmarkCommand = new RelayCommand<BookmarkViewModel>(DeleteBookmark),
                    NewBookmarkCommand = new RelayCommand(AddNewBookmark)
                };
                _settingsWindow.DataContext = settingsViewModel;
            }

            _settingsWindow.Show();
            _settingsWindow.Activate();
        }

        private void DestroySettingsWindow()
        {
            if (_settingsWindow != null)
            {
                _settingsWindow.Close();
                _settingsWindow = null;
            }
        }

        private void AddNewBookmark()
        {


            var path = ShowVagrantFolderBrowser(_settingsWindow);
            if (path.Equals(String.Empty))
            {
                return;
            }

            //See if we can reuse another vagrant instance, since multiple bookmarks might share the same vagrant instance.
            var matchingBookmark =
                _applicationData.Bookmarks.FirstOrDefault(b => b.VagrantInstance.Directory.Equals(path));

            var newBookmark = new Bookmark
            {
                VagrantInstance =
                    matchingBookmark != null
                        ? matchingBookmark.VagrantInstance
                        : new VagrantInstance {Directory = path},
                Name = Directory.GetParent(path).Name
            };

            _bookmarkWindow = new BookmarkSettingsWindow();

            var bookmarkViewModel = new BookmarkViewModel(newBookmark)
            {
                SaveCommand = new RelayCommand(() =>
                {
                    _tempApplicationData.Bookmarks.Add(newBookmark);
                    DestroyBookmarkWindow();
                }),
                CancelCommand = new RelayCommand(DestroyBookmarkWindow)
            };

            _bookmarkWindow.DataContext = bookmarkViewModel;

            _bookmarkWindow.Show();
            _bookmarkWindow.Activate();
        }

        private void EditBookmark(BookmarkViewModel bookmarkViewModel)
        {
            var bookmarkClone =
                _tempApplicationData.Bookmarks.First(b => b.Name.Equals(bookmarkViewModel.BookmarkName)).Clone();
            var viewModelCopy = new BookmarkViewModel(bookmarkClone);
            _bookmarkWindow = new BookmarkSettingsWindow();

            //TODO: validate settings
            viewModelCopy.BrowseCommand = new RelayCommand<string>(s =>
            {
                var newPath = ShowVagrantFolderBrowser(_bookmarkWindow, s);
                if (!newPath.Equals(String.Empty))
                {
                    viewModelCopy.VagrantInstanceLocation = newPath;
                }
            });
            viewModelCopy.SaveCommand = new RelayCommand(() =>
            {
                bookmarkViewModel.BookmarkName = viewModelCopy.BookmarkName;
                bookmarkViewModel.VagrantInstanceLocation = viewModelCopy.VagrantInstanceLocation;
                DestroyBookmarkWindow();
            });
            viewModelCopy.CancelCommand = new RelayCommand(DestroyBookmarkWindow);

            _bookmarkWindow.DataContext = viewModelCopy;

            _bookmarkWindow.Show();
            _bookmarkWindow.Activate();
        }

        private void DestroyBookmarkWindow()
        {
            if (_bookmarkWindow != null)
            {
                _bookmarkWindow.Close();
                _bookmarkWindow = null;
            }
        }

        private void DeleteBookmark(BookmarkViewModel bookmarkViewModel)
        {
            var result = MessageBox.Show(new Win32Wrapper(_settingsWindow), "Are you sure you want to delete this bookmark?",
                "Delete Bookmark", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
            {
                return;
            }

            _tempApplicationData.Bookmarks.Remove(
                _tempApplicationData.Bookmarks.First(b => b.Name == bookmarkViewModel.BookmarkName));
        }

        private void SaveSettings()
        {
            using (var ms = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, _tempApplicationData.Bookmarks);
                ms.Position = 0;
                var buffer = new byte[(int)ms.Length];
                ms.Read(buffer, 0, buffer.Length);
                Properties.Settings.Default.SavedBookmarks = Convert.ToBase64String(buffer);
            }

            Properties.Settings.Default.Save();

            const string shortcutName = "VagrantTray.lnk";
            var startupPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var startupShortcut = Path.Combine(startupPath, shortcutName);
            
            if (Properties.Settings.Default.LaunchOnWindowsStartup)
            {
                if (!File.Exists(startupShortcut))
                {
                    var t = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")); //Windows Script Host Shell Object
                    dynamic shell = Activator.CreateInstance(t);
                    try
                    {
                        var lnk = shell.CreateShortcut(startupShortcut);
                        try
                        {
                            lnk.TargetPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                            lnk.IconLocation = "shell32.dll, 1";
                            lnk.Save();
                        }
                        finally
                        {
                            Marshal.FinalReleaseComObject(lnk);
                        }
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(shell);
                    }
                }
            }
            else if (File.Exists(startupShortcut))
            {
                File.Delete(startupShortcut);
            }

            var newBookmarks =
                _tempApplicationData.Bookmarks.Where(
                    b => !_applicationData.Bookmarks.Contains(b, new BookmarkEqualityComparer())).ToList();
            var removedBookmarks =
                _applicationData.Bookmarks.Where(
                    b => !_tempApplicationData.Bookmarks.Contains(b, new BookmarkEqualityComparer())).ToList();

            foreach (var removedBookmark in removedBookmarks)
            {
                _applicationData.Bookmarks.Remove(removedBookmark);
            }
            foreach (var newBookmark in newBookmarks)
            {
                _applicationData.Bookmarks.Add(newBookmark);
            }
        }

        private static string ShowVagrantFolderBrowser(Window parentWindow, string initialPath = "")
        {
            var winWrapper = new Win32Wrapper(parentWindow);

            var folderDialog = new FolderBrowserDialog();
            folderDialog.Description = "Select Vagrant Folder";
            folderDialog.ShowNewFolderButton = false;
            folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            if (!initialPath.Equals(String.Empty))
            {
                folderDialog.SelectedPath = initialPath;
            }


            if (folderDialog.ShowDialog(winWrapper) == DialogResult.OK)
            {
                if (!File.Exists(Path.Combine(folderDialog.SelectedPath, "Vagrantfile")))
                {
                    MessageBox.Show(winWrapper,
                        "The folder you have selected is not a valid Vagrant folder. Please try again.");
                    return String.Empty;
                }
            }
            return folderDialog.SelectedPath;
        }
    }
}
