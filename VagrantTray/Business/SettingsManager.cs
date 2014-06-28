using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Forms;
using GalaSoft.MvvmLight.Command;
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
                    OkCommand = new RelayCommand(() =>
                    {
                        SaveSettings();
                        _settingsWindow.Close();
                    }),
                    EditBookmarkCommand = new RelayCommand<BookmarkViewModel>(EditBookmark),
                    DeleteBookmarkCommand = new RelayCommand<BookmarkViewModel>(DeleteBookmark)
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


            var path = ShowVagrantFolderBrowser(_settingsWindow);
            if (path.Equals(String.Empty))
            {
                return;
            }

            _editingBookmark = new Bookmark
            {
                VagrantInstance = new VagrantInstance {Directory = path, Name = "Test", Id = "1234"},
                Name = "New Bookmark"
            };

            _bookmarkWindow = new BookmarkSettingsWindow();

            var bookmarkViewModel = new BookmarkViewModel(_editingBookmark);
            //TODO: validate settings
            bookmarkViewModel.SaveCommand = new RelayCommand(() =>
            {
                _applicationData.Bookmarks.Add(_editingBookmark);
                SaveSettings();
                _bookmarkWindow.Close();
            });
            bookmarkViewModel.CancelCommand = new RelayCommand(() => _bookmarkWindow.Close());

            _bookmarkWindow.DataContext = bookmarkViewModel;

            _bookmarkWindow.Show();
            _bookmarkWindow.Activate();
        }

        private void EditBookmark(BookmarkViewModel bookmarkViewModel)
        {
            _bookmarkWindow = new BookmarkSettingsWindow();

            //TODO: validate settings
            bookmarkViewModel.BrowseCommand = new RelayCommand<string>(s =>
            {
                var newPath = ShowVagrantFolderBrowser(_bookmarkWindow, s);
                if (!newPath.Equals(String.Empty))
                {
                    //TODO: this setting immediately persists in model, even if it doesn't get saved. Store as a temp value somewhere.
                    bookmarkViewModel.VagrantInstanceLocation = newPath;
                }
            });
            bookmarkViewModel.SaveCommand = new RelayCommand(() =>
            {
                SaveSettings();
                _bookmarkWindow.Close();
            });
            bookmarkViewModel.CancelCommand = new RelayCommand(() => _bookmarkWindow.Close());

            _bookmarkWindow.DataContext = bookmarkViewModel;

            _bookmarkWindow.Show();
            _bookmarkWindow.Activate();
        }

        private void DeleteBookmark(BookmarkViewModel bookmarkViewModel)
        {
            var result = MessageBox.Show(new Win32Wrapper(_settingsWindow), "Are you sure you want to delete this bookmark?",
                "Delete Bookmark", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Cancel)
            {
                return;
            }

            _applicationData.Bookmarks.Remove(
                _applicationData.Bookmarks.First(b => b.Name == bookmarkViewModel.BookmarkName));
        }

        private void SaveSettings()
        {
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
