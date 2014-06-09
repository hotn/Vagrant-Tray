using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MikeWaltonWeb.VagrantTray.Business;
using MikeWaltonWeb.VagrantTray.Business.Commands;
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
            

            _manager = new VagrantManager(_trayMenu);
        }

        private void OnExit(object sender, EventArgs e)
        {
            _trayMenu.Dispose();
            Application.Current.Shutdown();
        }
    }
}
