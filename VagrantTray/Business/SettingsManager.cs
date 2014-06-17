using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using GalaSoft.MvvmLight.Command;
using MikeWaltonWeb.VagrantTray.Business.Utility.Windows;
using MikeWaltonWeb.VagrantTray.Model;
using MikeWaltonWeb.VagrantTray.UI;
using MikeWaltonWeb.VagrantTray.ViewModel;

namespace MikeWaltonWeb.VagrantTray.Business
{
    public class SettingsManager
    {
        private SettingsWindow _settingsWindow;
        private BookmarkSettingsWindow _bookmarkWindow;

        private readonly ApplicationData _applicationData;

        private Bookmark _editingBookmark;

        public SettingsManager(ApplicationData applicationData)
        {
            _applicationData = applicationData;
        }

        public void ShowSettings()
        {
            if (_settingsWindow == null)
            {
                _settingsWindow = new SettingsWindow();
                var settingsViewModel = new SettingsViewModel(_applicationData)
                {
                    CancelCommand = new RelayCommand(_settingsWindow.Close),
                    CloseCommand = new RelayCommand(_settingsWindow.Close),
                    OkCommand = new RelayCommand(SaveSettings)
                };
                settingsViewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == "AddBookmarkSelectedItem")
                    {
                        AddNewBookmark();
                    }
                };
                _settingsWindow.DataContext = settingsViewModel;
            }

            _settingsWindow.Show();
            _settingsWindow.Activate();
        }

        private void AddNewBookmark()
        {
            var winWrapper = new Win32Wrapper(_settingsWindow);

            var folderDialog = new FolderBrowserDialog();
            folderDialog.ShowNewFolderButton = false;
            folderDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            folderDialog.Description = "Select Vagrant Folder";
            if (folderDialog.ShowDialog(winWrapper) == DialogResult.OK)
            {
                if (!File.Exists(Path.Combine(folderDialog.SelectedPath, "Vagrantfile")))
                {
                    MessageBox.Show(winWrapper,
                        "The folder you have selected is not a valid Vagrant folder. Please try again.");
                    return;
                }
            }

            _editingBookmark = new Bookmark
            {
                VagrantInstance = new VagrantInstance {Directory = folderDialog.SelectedPath, Name = "Test", Id = "1234"},
                Name = "New Bookmark"
            };

            _bookmarkWindow = new BookmarkSettingsWindow();

            var bookmarkViewModel = new BookmarkViewModel(_editingBookmark);
            //TODO: validate settings
            bookmarkViewModel.SaveCommand = new RelayCommand(SaveSettings, () => true);
            bookmarkViewModel.CancelCommand = new RelayCommand(() => _bookmarkWindow.Close(), () => true);

            _bookmarkWindow.DataContext = bookmarkViewModel;

            _bookmarkWindow.Show();
            _bookmarkWindow.Activate();
        }

        private void SaveSettings()
        {
            _applicationData.Bookmarks.Add(_editingBookmark);

            using (var ms = new MemoryStream())
            {
                using (var sr = new StreamReader(ms))
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, _applicationData.Bookmarks);
                    ms.Position = 0;
                    var buffer = new byte[(int)ms.Length];
                    ms.Read(buffer, 0, buffer.Length);
                    Properties.Settings.Default.SavedBookmarks = Convert.ToBase64String(buffer);
                }
            }

            Properties.Settings.Default.Save();
        }
    }
}
