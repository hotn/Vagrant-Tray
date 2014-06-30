using System;
using System.Collections.Generic;
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
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ApplicationData _applicationData;

        private bool _hasChanged;

        public List<BookmarkViewModel> Bookmarks
        {
            get
            {
                return _applicationData.Bookmarks != null
                    ? _applicationData.Bookmarks.Select(b => new BookmarkViewModel(b)).ToList()
                    : new List<BookmarkViewModel>();
            }
        }

        /// <summary>
        /// This constructor is for design purposes only.
        /// </summary>
        public SettingsViewModel()
        {
            _applicationData = new ApplicationData();
        }

        /// <summary>
        /// Initializes a new instance of the SettingsViewModel class.
        /// </summary>
        public SettingsViewModel(ApplicationData applicationData)
        {
            _applicationData = applicationData;

            Init();
        }

        private void Init()
        {
            _applicationData.Bookmarks.CollectionChanged += (sender, args) => RaisePropertyChanged("Bookmarks");
        }

        private RelayCommand _okCommand;

        public RelayCommand OkCommand
        {
            get
            {
                if (IsInDesignMode)
                {
                    return _defaultRelayCommand;
                }

                return _okCommand;
            }
            set
            {
                _okCommand = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand _cancelCommand;

        public RelayCommand CancelCommand
        {
            get
            {
                if (IsInDesignMode)
                {
                    return _defaultRelayCommand;
                }

                return _cancelCommand;
            }
            set
            {
                _cancelCommand = value;
                RaisePropertyChanged();
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

        private RelayCommand _newBookmarkCommand;

        public RelayCommand NewBookmarkCommand
        {
            get
            {
                if (IsInDesignMode)
                {
                    return _defaultRelayCommand;
                }

                return _newBookmarkCommand;
            }
            set
            {
                _newBookmarkCommand = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand<BookmarkViewModel> _editBookmarkCommand;

        public RelayCommand<BookmarkViewModel> EditBookmarkCommand
        {
            get
            {
                if (IsInDesignMode)
                {
                    return new RelayCommand<BookmarkViewModel>(b => { }, b => true);
                }

                return _editBookmarkCommand;
            }
            set
            {
                _editBookmarkCommand = value;
                RaisePropertyChanged();
            }
        }

        private RelayCommand<BookmarkViewModel> _deleteBookmarkCommand;

        public RelayCommand<BookmarkViewModel> DeleteBookmarkCommand
        {
            get
            {
                if (IsInDesignMode)
                {
                    return new RelayCommand<BookmarkViewModel>(b => { }, b => true);
                }

                return _deleteBookmarkCommand;
            }
            set
            {
                _deleteBookmarkCommand = value;
                RaisePropertyChanged();
            }
        }

        private readonly RelayCommand _defaultRelayCommand = new RelayCommand(() => { }, () => true);
    }
}