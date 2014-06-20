using System;
using System.Collections.Generic;
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
            var submenu = new VagrantToolStripMenuItem(bookmark);

            //Add status menu items.
            submenu.DropDownItems.Add(new VagrantToolStripMenuItem("Name: " + bookmark.VagrantInstance.Name) { Enabled = false });
            submenu.DropDownItems.Add(new VagrantToolStripMenuItem("Provider: " + bookmark.VagrantInstance.Provider) { Enabled = false });
            submenu.DropDownItems.Add(new VagrantToolStripMenuItem("State: " + bookmark.VagrantInstance.CurrentState) { Enabled = false });
            submenu.DropDownItems.Add(new VagrantToolStripMenuItem("Directory: " + bookmark.VagrantInstance.Directory) { Enabled = false });
            submenu.DropDownItems.Add(new ToolStripSeparator());

            //Add vagrant action menu items.
            foreach (var commandAction in commandActions)
            {
                Bitmap icon = null;
                var resource = Properties.Resources.ResourceManager.GetObject(commandAction.Key);
                if (resource != null)
                {
                    icon = Icon.FromHandle(((Icon)resource).Handle).ToBitmap();
                }

                submenu.DropDownItems.Add(new VagrantToolStripMenuItem(commandAction.Key, icon,
                    (s, e) =>
                    {
                        commandAction.Value.Invoke();
                    }) {IsActionItem = true});
            }

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
