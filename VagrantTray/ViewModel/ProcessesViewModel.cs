using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.ViewModel
{
    /// <summary>
    /// This class contains properties that a View can data bind to.
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class ProcessesViewModel : ViewModelBase
    {
        private readonly ApplicationData _applicationData;

        public List<string> Bookmarks
        {
            get
            {
                if (IsInDesignMode)
                {
                    return new List<string> {"Bookmark 1", "Bookmark 2", "Bookmark 3"};
                }

                if (_applicationData.Bookmarks == null || _applicationData.Bookmarks.Select(b => b.VagrantInstance).Count(i => i.CurrentProcess != null) == 0)
                {
                    return new List<string> {"No Bookmarks"};
                }

                return _applicationData.Bookmarks.Select(b => b.Name).ToList();
            }
        }

        private Bookmark _selectedBookmark;

        public string SelectedBookmark
        {
            get
            {
                if (IsInDesignMode)
                {
                    return "Bookmark 2";
                }

                if (_selectedBookmark == null)
                {
                    return "No Bookmarks";
                }

                return _selectedBookmark.Name;
            }

            set
            {
                //remove any existing listener
                if (_selectedBookmark != null && _selectedBookmark.VagrantInstance.CurrentProcess != null)
                {
                    _selectedBookmark.VagrantInstance.CurrentProcess.OutputDataReceived -=
                        CurrentProcessOnOutputDataReceived;
                }

                _selectedBookmark = _applicationData.Bookmarks.First(b => b.Name == value);
                _selectedBookmark.VagrantInstance.CurrentProcess.OutputDataReceived += CurrentProcessOnOutputDataReceived;
                RaisePropertyChanged();
                RaisePropertyChanged("BookmarkProcessOutput");
            }
        }

        private void CurrentProcessOnOutputDataReceived(object sender, DataReceivedEventArgs dataReceivedEventArgs)
        {
            RaisePropertyChanged("BookmarkProcessOutput");
        }

        public string BookmarkProcessOutput
        {
            get
            {
                if (IsInDesignMode)
                {
                    return "Output here." + Environment.NewLine + "There will be lots.";
                }

                if (_selectedBookmark == null)
                {
                    return "No bookmark selected.";
                }

                if (_selectedBookmark.VagrantInstance.CurrentProcess == null)
                {
                    return "No process running.";
                }

                return String.Join(Environment.NewLine, _selectedBookmark.VagrantInstance.CurrentProcess.OutputData);
            }
        }

        /// <summary>
        /// This constructor is for design purposes only.
        /// </summary>
        public ProcessesViewModel()
        {
            _applicationData = new ApplicationData();
        }

        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class.
        /// </summary>
        public ProcessesViewModel(ApplicationData applicationData)
        {
            _applicationData = applicationData;

            Init();
        }

        private void Init()
        {
            //_applicationData.Bookmarks.CollectionChanged += (sender, args) => RaisePropertyChanged("Bookmarks");
            if (_applicationData.Bookmarks != null)
            {
                _selectedBookmark = _applicationData.Bookmarks.FirstOrDefault();
            }
        }

        private RelayCommand _closeCommand;

        public RelayCommand CloseCommand
        {
            get
            {
                if (IsInDesignMode)
                {
                    return _defaultRelayCommand;
                }

                return _closeCommand;
            }
            set
            {
                _closeCommand = value;
                RaisePropertyChanged();
            }
        }

        private readonly RelayCommand _defaultRelayCommand = new RelayCommand(() => { }, () => true);
    }
}