﻿using System;
using System.IO;
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
    public class BookmarkViewModel : ViewModelBase
    {
        private readonly Bookmark _bookmark;
        private RelayCommand<string> _browseCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _cancelCommand;

        public string BookmarkName
        {
            get
            {
                if (IsInDesignMode)
                {
                    return "My Bookmark";
                }

                return _bookmark != null ?_bookmark.Name : String.Empty;
            }
            set
            {
                if (_bookmark != null)
                {
                    _bookmark.Name = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string VagrantInstanceLocation
        {
            get
            {
                if (IsInDesignMode)
                {
                    return "C:/somefolder/vagrant";
                }

                return _bookmark != null ? _bookmark.VagrantInstance.Directory : String.Empty;
            }
            set
            {
                if (_bookmark != null)
                {
                    _bookmark.VagrantInstance.Directory = value;
                    RaisePropertyChanged();
                }
            }
        }

        public bool IsPresent
        {
            get { return Directory.Exists(VagrantInstanceLocation); }
        }

        public RelayCommand<string> BrowseCommand
        {
            get
            {
                if (IsInDesignMode)
                {
                    return new RelayCommand<string>(s => { }, s => true);
                }

                return _browseCommand;
            }
            set
            {
                _browseCommand = value;
                RaisePropertyChanged();
            }
        }

        

        public RelayCommand SaveCommand
        {
            get
            {
                if (IsInDesignMode)
                {
                    return _defaultCommand;
                }

                return _saveCommand;
            }
            set
            {
                _saveCommand = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                if (IsInDesignMode)
                {
                    return _defaultCommand;
                }

                return _cancelCommand;
            }
            set
            {
                _cancelCommand = value;
                RaisePropertyChanged();
            }
        }

        private readonly RelayCommand _defaultCommand = new RelayCommand(() => { }, () => true);

        /// <summary>
        /// Initializes a new instance of the BookmarkViewModel class.
        /// </summary>
        public BookmarkViewModel()
        {
        }

        public BookmarkViewModel(Bookmark bookmark)
        {
            _bookmark = bookmark;
        }
    }
}