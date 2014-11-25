using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using MikeWaltonWeb.VagrantTray.Model;
using MikeWaltonWeb.VagrantTray.Properties;
using Timer = System.Timers.Timer;

namespace MikeWaltonWeb.VagrantTray.UI.Tray
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class VagrantInstanceSubMenu : ToolStripMenuItem
    {
        private Bitmap[] _loadingBitmaps;
        private Timer _timer;
        private int _currentLoadingBitmapIndex;
        private readonly Bookmark _bookmark;
        private readonly Dictionary<string, Action> _commandActions;

        private VagrantToolStripMenuItem _nameMenuItem;
        private VagrantToolStripMenuItem _stateMenuItem;
        private VagrantToolStripMenuItem _directoryMenuItem;
        private List<VagrantToolStripMenuItem> _vagrantCommandMenuItems; 

        public VagrantInstanceSubMenu(Bookmark bookmark, Dictionary<string, Action> commandActions)
            : base(bookmark.Name)
        {
            _bookmark = bookmark;
            _commandActions = commandActions;

            Init();
        }

        private void Init()
        {
            BuildSubMenuItems();

            Icon = MenuItemIcon.Loading;
            
            _bookmark.VagrantInstance.StateChanged += OnVagrantInstanceStateChanged;
        }

        private void BuildSubMenuItems()
        {
            //Add status menu items.
            _nameMenuItem = new VagrantToolStripMenuItem("Name: " + _bookmark.Name) {IsActionItem = false};
            _stateMenuItem = new VagrantToolStripMenuItem("State: " + _bookmark.VagrantInstance.CurrentState)
            {
                IsActionItem = false
            };
            _directoryMenuItem = new VagrantToolStripMenuItem("Directory: " + _bookmark.VagrantInstance.Directory)
            {
                IsActionItem = false
            };
            DropDownItems.Add(_nameMenuItem);
            DropDownItems.Add(_stateMenuItem);
            DropDownItems.Add(_directoryMenuItem);
            DropDownItems.Add(new ToolStripSeparator());

            //add command prompt option
            DropDownItems.Add(new ToolStripMenuItem("Open shell application", null, (sender, args) =>
            {
                var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    FileName = Properties.Settings.Default.ShellApplication,
                    WorkingDirectory = _bookmark.VagrantInstance.Directory
                };
                process.StartInfo = startInfo;
                process.Start();
                Thread.Sleep(50);

                var icon = Resources.VagrantIcon;
                SendMessage(process.MainWindowHandle, WM_SETICON, ICON_BIG, icon.Handle);
                SendMessage(process.MainWindowHandle, WM_SETICON, ICON_SMALL, icon.Handle);
            }));

            //add open in explorer option
            DropDownItems.Add(new ToolStripMenuItem("Open in file explorer", null, (sender, args) =>
            {
                var process = new Process();
                var startInfo = new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = _bookmark.VagrantInstance.Directory
                };
                process.StartInfo = startInfo;
                process.Start();
            }));

            DropDownItems.Add(new ToolStripSeparator());

            //Add vagrant action menu items.
            _vagrantCommandMenuItems = new List<VagrantToolStripMenuItem>();
            foreach (var commandAction in _commandActions)
            {
                var action = commandAction.Value;
                Bitmap icon = null;
                var resource = Resources.ResourceManager.GetObject(commandAction.Key);
                if (resource != null)
                {
                    icon = System.Drawing.Icon.FromHandle(((Icon)resource).Handle).ToBitmap();
                }

                var menuItem = new VagrantToolStripMenuItem(commandAction.Key, icon,
                    (s, e) => action.Invoke()) {IsActionItem = true};

                DropDownItems.Add(menuItem);

                _vagrantCommandMenuItems.Add(menuItem);
            }
        }

        public void OnVagrantInstanceStateChanged(object sender, EventArgs eventArgs)
        {
            var vagrantInstance = (VagrantInstance) sender;
            //TODO: this might not be the best implementation of enums
            Icon =
                (MenuItemIcon)Enum.Parse(typeof(MenuItemIcon), vagrantInstance.CurrentState.ToString());

            _nameMenuItem.Text = "Name: " + _bookmark.Name;
            _stateMenuItem.Text = "State: " + _bookmark.VagrantInstance.CurrentStateString;
            _directoryMenuItem.Text = "Directory: " + _bookmark.VagrantInstance.Directory;

            //Enable menu item click if vagrant is no longer processing.
            if (IsActionItem && vagrantInstance.CurrentState != VagrantInstance.State.Loading)
            {
                Enabled = true;
            }
        }

        /// <summary>
        /// Gets or sets whether the menu item is associated to an action and can be clicked.
        /// </summary>
        public bool IsActionItem { get; set; }

        public MenuItemIcon Icon
        {
            set
            {
                if (_timer != null)
                {
                    _timer.Dispose();
                }

                var vagrantCommandsEnabled = true;

                switch (value)
                {
                    case MenuItemIcon.Running:
                        Image = System.Drawing.Icon.FromHandle(Resources.Green.Handle).ToBitmap();
                        break;
                    case MenuItemIcon.Saved:
                        Image = System.Drawing.Icon.FromHandle(Resources.Yellow.Handle).ToBitmap();
                        break;
                    case MenuItemIcon.Poweroff:
                        Image = System.Drawing.Icon.FromHandle(Resources.Red.Handle).ToBitmap();
                        break;
                    case MenuItemIcon.NotCreated:
                        Image = System.Drawing.Icon.FromHandle(Resources.Red_Warning.Handle).ToBitmap();
                        break;
                    case MenuItemIcon.Loading:
                        if (_loadingBitmaps == null)
                        {
                            InitLoadingIcon();
                        }
                        Image = _loadingBitmaps[0];

                        _timer = new Timer(100);
                        _timer.Elapsed += TimerOnTick;
                        _timer.Start();

                        vagrantCommandsEnabled = false;

                        break;
                }

                //toggle whether commands can be run
                foreach (var vagrantToolStripMenuItem in _vagrantCommandMenuItems)
                {
                    vagrantToolStripMenuItem.Enabled = vagrantCommandsEnabled;
                }
            }
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (_currentLoadingBitmapIndex < _loadingBitmaps.Length - 1)
            {
                try
                {
                    Image = _loadingBitmaps[_currentLoadingBitmapIndex];
                }
                catch (Exception)
                {
                    //For some reason, setting the icon occasionally causes an exception.
                    //As long as we handle it, we're good since it's not a precision animation in the first place.
                }
                _currentLoadingBitmapIndex++;
            }
            else
            {
                _currentLoadingBitmapIndex = 0;
            }
        }

        private void InitLoadingIcon()
        {
            var bmp = new Bitmap(Resources.LoadingStrip);
            // the color from the left bottom pixel will be made transparent
            bmp.MakeTransparent();

            _loadingBitmaps = new Bitmap[bmp.Width / 32];
            for (var i = 0; i < _loadingBitmaps.Length; i++)
            {
                var rect = new Rectangle(i * 32, 0, 32, 32);
                var bmp2 = bmp.Clone(rect, bmp.PixelFormat);
                _loadingBitmaps[i] = System.Drawing.Icon.FromHandle(bmp2.GetHicon()).ToBitmap();
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_loadingBitmaps != null)
            {
                foreach (var loadingBitmap in _loadingBitmaps)
                {
                    loadingBitmap.Dispose();
                }

                _loadingBitmaps = null;
            }

            if (_timer != null)
            {
                _timer.Dispose();
            }
        }

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hwnd, int message, int wParam, IntPtr lParam);

        private const int WM_SETICON = 0x80;
        private const int ICON_SMALL = 0;
        private const int ICON_BIG = 1;
    }

    public enum MenuItemIcon
    {
        Running,
        Saved,
        Poweroff,
        Loading,
        NotCreated
    }
}
