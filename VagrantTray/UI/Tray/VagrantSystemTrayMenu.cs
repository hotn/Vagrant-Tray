using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MikeWaltonWeb.VagrantTray.Model;
using MikeWaltonWeb.VagrantTray.Properties;
using Timer = System.Timers.Timer;

namespace MikeWaltonWeb.VagrantTray.UI.Tray
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class VagrantSystemTrayMenu : ContextMenuStrip
    {
        public event EventHandler ExitClicked;
        public event EventHandler SettingsClicked;
        public event EventHandler TrayIconClicked;

        private NotifyIcon _icon;
        private Bitmap[] _loadingBitmaps;
        private Timer _timer;
        private int _currentLoadingBitmapIndex;

        public VagrantSystemTrayMenu()
        {
            Init();
        }

        private void Init()
        {
            InitLoadingIcon();

            _icon = new NotifyIcon
            {
                Text = "Vagrant Tray",
                Icon = Icon.FromHandle(_loadingBitmaps[0].GetHicon()),
                ContextMenuStrip = this,
                Visible = true
            };

            AddDefaultMenuItems();

            _icon.Click += (sender, args) =>
            {
                if (((MouseEventArgs)args).Button == MouseButtons.Left && TrayIconClicked != null)
                {
                    TrayIconClicked(sender, args);
                }
            };
        }

        private void AddDefaultMenuItems()
        {
            Items.Add(new ToolStripSeparator());
            Items.Add("Settings...", null, (sender, args) =>
            {
                if (SettingsClicked != null)
                {
                    SettingsClicked(this, EventArgs.Empty);
                }
            });
            Items.Add("Exit", null, (sender, args) =>
            {
                if (ExitClicked != null)
                {
                    ExitClicked(this, EventArgs.Empty);
                }
            });
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            _icon.Icon = Icon.FromHandle(_loadingBitmaps[_currentLoadingBitmapIndex].GetHicon());

            if (_currentLoadingBitmapIndex < _loadingBitmaps.Length - 1)
            {
                _currentLoadingBitmapIndex++;
            }
            else
            {
                _currentLoadingBitmapIndex = 0;
            }
        }

        private void InitLoadingIcon()
        {
            var bmp = new Bitmap(Resources.VagrantWorking);
            // the color from the left bottom pixel will be made transparent
            bmp.MakeTransparent();

            _loadingBitmaps = new Bitmap[bmp.Width / 32];
            for (var i = 0; i < _loadingBitmaps.Length; i++)
            {
                var rect = new Rectangle(i * 32, 0, 32, 32);
                var bmp2 = bmp.Clone(rect, bmp.PixelFormat);

                _loadingBitmaps[i] = Icon.FromHandle(bmp2.GetHicon()).ToBitmap();
            }
        }

        public void AddBookmarkSubmenu(Bookmark bookmark, Dictionary<string, Action> commandActions)
        {
            var submenu = new VagrantInstanceSubMenu(bookmark, commandActions);
            Items.Insert(0, submenu);
        }

        public void Reset()
        {
            Items.Clear();
            AddDefaultMenuItems();
        }

        /// <summary>
        /// Display a tooltip on the system tray menu.
        /// </summary>
        /// <param name="message">Message to show.</param>
        /// <param name="duration">Duration in milliseconds to show message.</param>
        public void ShowMessageBalloon(string message, int duration)
        {
            ShowMessageBalloon(message, null, duration);
        }

        public void ShowMessageBalloon(string message)
        {
            ShowMessageBalloon(message, 1000);
        }

        public void ShowMessageBalloon(string message, Action clickAction)
        {
            ShowMessageBalloon(message, clickAction, 1000);
        }

        public void ShowMessageBalloon(string message, Action clickAction, int duration)
        {
            _icon.BalloonTipText = message;
            _icon.ShowBalloonTip(duration);

            if (clickAction != null)
            {
                EventHandler clickHandler = null;
                clickHandler = (sender, args) =>
                {
                    _icon.BalloonTipClicked -= clickHandler;
                    clickAction();
                };
                _icon.BalloonTipClicked += clickHandler;

                EventHandler removeHandler = null;
                removeHandler = (sender, args) =>
                {
                    _icon.BalloonTipClosed -= removeHandler;
                    _icon.BalloonTipClicked -= clickHandler;
                };
                _icon.BalloonTipClosed += removeHandler;
            }
        }

        public void StartWorkingAnimation()
        {
            if (_timer == null)
            {
                _timer = new Timer(200);
                _timer.Elapsed += TimerOnTick;
            }
            _timer.Start();
        }

        public void StopWorkingAnimation()
        {
            if (_timer == null)
            {
                return;
            }

            _timer.Stop();
            _currentLoadingBitmapIndex = 0;
            try
            {
                _icon.Icon = Icon.FromHandle(_loadingBitmaps[_currentLoadingBitmapIndex].GetHicon());
            }
            catch (Exception)
            {
                //For some reason, setting the icon occasionally causes an exception.
                //As long as we handle it, we're good since it's not a precision animation in the first place.
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _icon.Visible = false;
            _icon.Dispose();
            _icon = null;

            foreach (var item in Items)
            {
                var disposable = item as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
            Items.Clear();
        }
    }
}
