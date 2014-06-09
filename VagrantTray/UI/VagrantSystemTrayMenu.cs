﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MikeWaltonWeb.VagrantTray.Business;
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
            Items.Add("Settings...", null, ExitClicked);
            Items.Add("Exit", null, SettingsClicked);
        }

        private IEnumerable<ToolStripItem> GenerateMenuItemsForVagrantInstance(VagrantInstance instance)
        {
            return
                Enum.GetNames(typeof(VagrantCommand))
                    .Select(
                        name =>
                        {
                            Bitmap icon = null;
                            var resource = Properties.Resources.ResourceManager.GetObject(name);
                            if (resource != null)
                            {
                                icon = Icon.FromHandle(((Icon)resource).Handle).ToBitmap();
                            }
                            return new ToolStripMenuItem(name, icon,
                                (s, e) =>
                                {
                                    /*_manager.GetActionForVagrantInstanceCommand(instance,
                                        (VagrantCommand)Enum.Parse(typeof(VagrantCommand), name)).Invoke();
                                    RebuildList();*/
                                });
                        });
        }

        public void AddInstanceSubmenu(VagrantInstance instance)
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
            foreach (var vagrantMenuItem in GenerateMenuItemsForVagrantInstance(instance))
            {
                submenu.DropDownItems.Add(vagrantMenuItem);
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
