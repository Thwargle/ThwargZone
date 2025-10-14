using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ThwargZone
{
    /// <summary>
    /// Interaction logic for GameLauncher.xaml
    /// </summary>
    public partial class GameLauncher : Window
    {
        public GameLauncherViewModel ViewModel { get; }

        public GameLauncher()
        {
            InitializeComponent();

            ViewModel = new GameLauncherViewModel();
            DataContext = ViewModel;

            ThwargZone.AppSettings.WpfWindowPlacementSetting.Persist(this);
            
            // Update title when window is loaded
            Loaded += (s, e) => UpdateWindowTitle();
            
            // Update title when window is activated (shown)
            Activated += (s, e) => UpdateWindowTitle();
        }
        
        private void UpdateWindowTitle()
        {
            var serverName = Properties.Settings.Default.ServerName;
            if (!string.IsNullOrEmpty(serverName))
            {
                TitleText.Text = $"ThwargZone for Asheron's Call - {serverName}";
                this.Title = $"ThwargZone for Asheron's Call - {serverName}";
            }
            else
            {
                TitleText.Text = "ThwargZone for Asheron's Call";
                this.Title = "ThwargZone for Asheron's Call";
            }
        }
        
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();
                }
                catch (InvalidOperationException)
                {
                    // Ignore if DragMove cannot be called
                }
            }
        }
        
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

