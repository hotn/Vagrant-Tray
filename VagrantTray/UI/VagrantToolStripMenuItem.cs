﻿using System;
using System.Drawing;
using System.Windows.Forms;
using MikeWaltonWeb.VagrantTray.Properties;

namespace MikeWaltonWeb.VagrantTray.UI
{
    public class VagrantToolStripMenuItem : ToolStripMenuItem
    {
        private static Bitmap[] _loadingBitmaps;
        private Timer _timer;
        private int _currentLoadingBitmapIndex;

        public VagrantToolStripMenuItem(string text) : base(text)
        {

        }

        public VagrantToolStripMenuItem(string text, MenuItemIcon icon) : base(text)
        {
            Icon = icon;
        }

        public MenuItemIcon Icon
        {
            set
            {
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
                    case MenuItemIcon.Loading:
                        if (_loadingBitmaps == null)
                        {
                            InitLoadingIcon();
                        }
                        Image = _loadingBitmaps[0];

                        _timer = new Timer {Interval = 100};
                        _timer.Tick += TimerOnTick;
                        _timer.Start();
                        break;
                }
            }
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            if (_currentLoadingBitmapIndex < _loadingBitmaps.Length)
            {
                Image = _loadingBitmaps[_currentLoadingBitmapIndex];
                _currentLoadingBitmapIndex++;
            }
            else
            {
                _currentLoadingBitmapIndex = 0;
            }
        }

        private static void InitLoadingIcon()
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
    }

    public enum MenuItemIcon
    {
        Running,
        Saved,
        Poweroff,
        Loading
    }
}
