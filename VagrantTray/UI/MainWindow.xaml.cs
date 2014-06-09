using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MikeWaltonWeb.VagrantTray.Business;
using MikeWaltonWeb.VagrantTray.Model;
using Application = System.Windows.Application;

namespace MikeWaltonWeb.VagrantTray.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private VagrantSystemTrayMenu _trayMenu;

        private VagrantManager _manager;

        public MainWindow()
        {
            InitializeComponent();

            Visibility = Visibility.Hidden;

            CreateTrayMenu();
        }

        private void CreateTrayMenu()
        {
            _trayMenu = new VagrantSystemTrayMenu();
            _trayMenu.ExitClicked += OnExit;
            _trayMenu.SettingsClicked += (sender, args) => RebuildList();

            _manager = new VagrantManager(_trayMenu);

            RebuildList();
        }

        private void OnExit(object sender, EventArgs e)
        {
            _trayMenu.Dispose();
            Application.Current.Shutdown();
        }

        private void RebuildList()
        {
            _trayMenu.Reset();

            var instances = _manager.GetInstances();
            foreach (var vagrantInstance in instances)
            {
                _trayMenu.AddInstanceSubmenu(vagrantInstance);
            }
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
