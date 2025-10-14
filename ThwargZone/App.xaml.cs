using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ThwargZone
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Check if user wants to remember server selection
            bool rememberSelection = ThwargZone.Properties.Settings.Default.RememberServerSelection;
            bool hasServerConfigured = !string.IsNullOrEmpty(ThwargZone.Properties.Settings.Default.ServerPath);
            
            if (rememberSelection && hasServerConfigured)
            {
                // Skip server selection and go directly to GameLauncher
                var gameLauncher = new GameLauncher();
                gameLauncher.Show();
                MainWindow = gameLauncher;
            }
            else
            {
                // Show server selection window
                var serverSelection = new ServerSelection();
                serverSelection.Show();
                MainWindow = serverSelection;
            }
        }
    }
}
