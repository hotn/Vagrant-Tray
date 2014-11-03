using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using MikeWaltonWeb.VagrantTray.Business;
using Parse;

namespace MikeWaltonWeb.VagrantTray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

//only catch and log global exceptions in production builds
#if (DEBUG != true)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            ParseClient.Initialize("wKFfXkN9I4iCNDuezdhxNspXMGWAPBrOoSPB9Xfv", "fBfReXHDQ7AQ9CLogJdZT2wgIeplfe4jrvJpCNdx");
#endif

            VagrantManager.CreateInstance(this);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            Task.Run(() => UploadException(unhandledExceptionEventArgs.ExceptionObject)).Wait();

            try
            {
                Current.Shutdown();
            }
            catch
            {
            }
            try
            {
                Environment.Exit(0);
            }
            catch
            {
            }
        }

        private static async Task UploadException(object exceptionObject)
        {
            var parseObject = new ParseObject("UnhandledException");

            parseObject["appversion"] =
                FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;
            parseObject["exception"] = exceptionObject.ToString();

            await parseObject.SaveAsync();
        }
    }
}
