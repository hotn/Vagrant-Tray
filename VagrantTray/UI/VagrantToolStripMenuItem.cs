using System;
using System.Drawing;
using System.Windows.Forms;
using MikeWaltonWeb.VagrantTray.Model;
using MikeWaltonWeb.VagrantTray.Properties;

namespace MikeWaltonWeb.VagrantTray.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class VagrantToolStripMenuItem : ToolStripMenuItem
    {
        private static Bitmap[] _loadingBitmaps;
        private Timer _timer;
        private int _currentLoadingBitmapIndex;

        public VagrantToolStripMenuItem(string text) : base(text)
        {

        }

        public VagrantToolStripMenuItem(string text, Image image, EventHandler onClick) : base(text, image, onClick)
        {
            
        }

        public VagrantToolStripMenuItem(string text, MenuItemIcon icon) : base(text)
        {
            Icon = icon;
        }

        public VagrantToolStripMenuItem(Bookmark bookmark)
            : this(
                bookmark.Name,
                (MenuItemIcon) Enum.Parse(typeof (MenuItemIcon), bookmark.VagrantInstance.CurrentState.ToString()))
        {
            bookmark.VagrantInstance.StateChanged += OnVagrantInstanceStateChanged;
        }

        public void OnVagrantInstanceStateChanged(object sender, EventArgs eventArgs)
        {
            var vagrantInstance = (VagrantInstance) sender;
            //TODO: this might not be the best implementation of enums
            Icon =
                (MenuItemIcon)Enum.Parse(typeof(MenuItemIcon), vagrantInstance.CurrentState.ToString());

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
    }

    public enum MenuItemIcon
    {
        Running,
        Saved,
        Poweroff,
        Loading
    }
}
