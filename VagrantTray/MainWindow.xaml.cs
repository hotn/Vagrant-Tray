using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace VagrantTray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private NotifyIcon _trayIcon;
        private ContextMenuStrip _trayMenu;

        private VagrantManager _manager;

        public MainWindow()
        {
            InitializeComponent();

            Visibility = Visibility.Hidden;

            CreateTrayMenu();
        }

        private void CreateTrayMenu()
        {
            // Create a simple tray menu with only one item.
            _trayMenu = new ContextMenuStrip();

            // Create a tray icon. In this example we use a
            // standard system icon for simplicity, but you
            // can of course use your own custom icon too.
            _trayIcon = new NotifyIcon();
            _trayIcon.Text = "Vagrant Tray";

            //_trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);
            _trayIcon.Icon = System.Drawing.Icon.FromHandle(Properties.Resources.Vagrant.GetHicon());

            // Add menu to tray icon and show it.
            _trayIcon.ContextMenuStrip = _trayMenu;
            _trayIcon.Visible = true;

            _manager = new VagrantManager(_trayIcon);

            RebuildList();
        }

        private void AddDefaultMenuItems()
        {
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add("Refresh List", null, (s, e) => RebuildList());
            _trayMenu.Items.Add("Exit", null, OnExit);
        }

        private void OnExit(object sender, EventArgs e)
        {
            _trayIcon.Visible = false;
            _trayMenu.Dispose();
            Application.Current.Shutdown();
        }

        private void RebuildList()
        {
            _trayMenu.Items.Clear();

            var instances = _manager.GetInstances();
            foreach (var vagrantInstance in instances)
            {
                Bitmap status = null;
                switch (vagrantInstance.State)
                {
                    case "running":
                        status = System.Drawing.Icon.FromHandle(Properties.Resources.Green.Handle).ToBitmap();
                        break;
                    case "saved":
                        status = System.Drawing.Icon.FromHandle(Properties.Resources.Yellow.Handle).ToBitmap();
                        break;
                    case "poweroff":
                        status = System.Drawing.Icon.FromHandle(Properties.Resources.Red.Handle).ToBitmap();
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

                _trayMenu.Items.Add(submenu);
            }

            AddDefaultMenuItems();
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
                                icon = System.Drawing.Icon.FromHandle(((Icon)resource).Handle).ToBitmap();
                            }
                            return new ToolStripMenuItem(name, icon,
                                (s, e) =>
                                {
                                    _manager.GetActionForVagrantInstanceCommand(instance,
                                        (VagrantCommand)Enum.Parse(typeof(VagrantCommand), name)).Invoke();
                                    RebuildList();
                                });
                        });
        }
    }
}
