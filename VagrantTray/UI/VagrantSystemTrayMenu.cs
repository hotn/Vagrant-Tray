using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.UI
{
    [System.ComponentModel.DesignerCategory("Code")]
    public class VagrantSystemTrayMenu : ContextMenuStrip
    {
        public event EventHandler ExitClicked;
        public event EventHandler SettingsClicked;
        public event EventHandler TrayIconClicked;

        private NotifyIcon _icon;

        public VagrantSystemTrayMenu()
        {
            Init();
        }

        private void Init()
        {
            _icon = new NotifyIcon
            {
                Text = "Vagrant Tray",
                Icon = Icon.FromHandle(Properties.Resources.Vagrant.GetHicon()),
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
        public void ShowMessageBalloon(string message, int duration = 1000)
        {
            _icon.BalloonTipText = message;
            _icon.ShowBalloonTip(duration);
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
