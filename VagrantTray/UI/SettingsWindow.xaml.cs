using System.ComponentModel;
using System.Windows;

namespace MikeWaltonWeb.VagrantTray.UI
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow
	{
		public SettingsWindow()
		{
			InitializeComponent();
			
			// Insert code required on object creation below this point.
		}

	    protected override void OnClosing(CancelEventArgs e)
	    {
	        e.Cancel = true;
            Visibility = Visibility.Hidden;
	    }
	}
}