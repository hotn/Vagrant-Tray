using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MikeWaltonWeb.VagrantTray.Business.VagrantExe;
using MikeWaltonWeb.VagrantTray.Model;

namespace MikeWaltonWeb.VagrantTray.UI
{
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

        public void AddInstanceSubmenu(VagrantInstance instance, Dictionary<VagrantProcess.Command, Action> commandActions)
        {
            var status = MenuItemIcon.Loading;
            switch (instance.State)
            {
                case "running":
                    status = MenuItemIcon.Running;
                    break;
                case "saved":
                    status = MenuItemIcon.Saved;
                    break;
                case "poweroff":
                    status = MenuItemIcon.Poweroff;
                    break;
            }

            var submenu = new VagrantToolStripMenuItem(instance.Id, status);
            submenu.DropDownItems.Add(new VagrantToolStripMenuItem("Name: " + instance.Name) { Enabled = false });
            submenu.DropDownItems.Add(new VagrantToolStripMenuItem("Provider: " + instance.Provider) { Enabled = false });
            submenu.DropDownItems.Add(new VagrantToolStripMenuItem("State: " + instance.State) { Enabled = false });
            submenu.DropDownItems.Add(new VagrantToolStripMenuItem("Directory: " + instance.Directory) { Enabled = false });
            submenu.DropDownItems.Add(new ToolStripSeparator());
            foreach (var commandAction in commandActions)
            {
                Bitmap icon = null;
                var resource = Properties.Resources.ResourceManager.GetObject(commandAction.Key.ToString());
                if (resource != null)
                {
                    icon = Icon.FromHandle(((Icon)resource).Handle).ToBitmap();
                }

                submenu.DropDownItems.Add(new ToolStripMenuItem(commandAction.Key.ToString(), icon,
                    (s, e) =>
                    {
                        commandAction.Value.Invoke();
                    }));
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
