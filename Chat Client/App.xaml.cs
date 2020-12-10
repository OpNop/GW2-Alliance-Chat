using System.Threading;
using System.Windows;

namespace Chat_Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutex = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "TinyChatTACS";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                //app is already running! Exiting the application
                MessageBox.Show("TACS is already running", "Tiny Alliance Chat", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
