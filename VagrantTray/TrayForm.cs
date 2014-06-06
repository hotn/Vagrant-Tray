using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using VagrantTray.Properties;

namespace VagrantTray
{
    public partial class TrayForm : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        private readonly VagrantManager _manager = new VagrantManager();

        public TrayForm()
        {
            CreateTrayMenu();

            Closed += (sender, args) => trayMenu.Dispose();
        }

        private void CreateTrayMenu()
        {
            // Create a simple tray menu with only one item.
            trayMenu = new ContextMenuStrip();

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            trayIcon = new NotifyIcon();
            trayIcon.Text = "Vagrant Tray";
            //trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            trayIcon.Icon = Icon.FromHandle(Resources.Vagrant.GetHicon());

            // Add menu to tray icon and show it.
            trayIcon.ContextMenuStrip = trayMenu;
            trayIcon.Visible = true;

            RebuildList();
        }

        private void AddDefaultMenuItems()
        {
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Refresh List", null, (s, e) => RebuildList());
            trayMenu.Items.Add("Exit", null, OnExit);
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false; // Hide form window.
            ShowInTaskbar = false; // Remove from taskbar.

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void RebuildList()
        {
            trayMenu.Items.Clear();

            var instances = _manager.GetInstances();
            foreach (var vagrantInstance in instances)
            {
                Bitmap status = null;
                switch (vagrantInstance.State)
                {
                    case "running":
                        status = Icon.FromHandle(Resources.Green.Handle).ToBitmap();
                        break;
                    case "saved":
                        status = Icon.FromHandle(Resources.Yellow.Handle).ToBitmap();
                        break;
                    case "poweroff":
                        status = Icon.FromHandle(Resources.Red.Handle).ToBitmap();
                        break;
                }

                var submenu = new ToolStripMenuItem(vagrantInstance.Id, status);
                submenu.DropDownItems.Add(new ToolStripMenuItem("Name: " + vagrantInstance.Name) { Enabled = false });
                submenu.DropDownItems.Add(new ToolStripMenuItem("Provider: " + vagrantInstance.Provider) { Enabled = false });
                submenu.DropDownItems.Add(new ToolStripMenuItem("State: " + vagrantInstance.State) { Enabled = false });
                submenu.DropDownItems.Add(new ToolStripMenuItem("Directory: " + vagrantInstance.Directory) { Enabled = false });
                submenu.DropDownItems.Add(new ToolStripSeparator());
                foreach (var vagrantMenuItem in GenerateMenuItemsForVagrantInstance(vagrantInstance))
                {
                    submenu.DropDownItems.Add(vagrantMenuItem);
                }

                trayMenu.Items.Add(submenu);
            }

            AddDefaultMenuItems();
        }

        private IEnumerable<ToolStripItem> GenerateMenuItemsForVagrantInstance(VagrantInstance instance)
        {
            return
                Enum.GetNames(typeof (VagrantCommand))
                    .Select(
                        name =>
                        {
                            Bitmap icon = null;
                            var resource = Resources.ResourceManager.GetObject(name);
                            if (resource != null)
                            {
                                icon = Icon.FromHandle(((Icon) resource).Handle).ToBitmap();
                            }
                            return new ToolStripMenuItem(name, icon,
                                (s, e) =>
                                {
                                    _manager.GetActionForVagrantInstanceCommand(instance,
                                        (VagrantCommand) Enum.Parse(typeof (VagrantCommand), name)).Invoke();
                                    RebuildList();
                                });
                        });
        }
    }
}
