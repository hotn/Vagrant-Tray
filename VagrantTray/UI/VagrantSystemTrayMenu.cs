using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using MikeWaltonWeb.VagrantTray.Business.Commands;
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

        public void AddInstanceSubmenu(VagrantInstance instance, Dictionary<VagrantCommand, Action> commandActions)
        {
            Bitmap status = null;
            switch (instance.State)
            {
                case "running":
                    status = Icon.FromHandle(Properties.Resources.Green.Handle).ToBitmap();
                    break;
                case "saved":
                    status = Icon.FromHandle(Properties.Resources.Yellow.Handle).ToBitmap();
                    break;
                case "poweroff":
                    status = Icon.FromHandle(Properties.Resources.Red.Handle).ToBitmap();
                    break;
            }

            var submenu = new ToolStripMenuItem(instance.Id, status);
            submenu.DropDownItems.Add(new ToolStripMenuItem("Name: " + instance.Name) { Enabled = false });
            submenu.DropDownItems.Add(new ToolStripMenuItem("Provider: " + instance.Provider) { Enabled = false });
            submenu.DropDownItems.Add(new ToolStripMenuItem("State: " + instance.State) { Enabled = false });
            submenu.DropDownItems.Add(new ToolStripMenuItem("Directory: " + instance.Directory) { Enabled = false });
            submenu.DropDownItems.Add(new ToolStripSeparator());
            foreach (var commandAction in commandActions)
            {
                //submenu.DropDownItems.Add(vagrantMenuItem);

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
        }
    }
}
