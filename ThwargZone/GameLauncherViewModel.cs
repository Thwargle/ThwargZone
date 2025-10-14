using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Input;
using ThwargZone.Win32;

namespace ThwargZone
{
    public class GameLauncherViewModel : INotifyPropertyChanged
    {
        // Commands
        public ICommand PlayCommand { get; }
        public ICommand ZoneCommand { get; }
        public ICommand GameLocationCommand { get; }
        public ICommand UserAccountCommand { get; }
        public ICommand ServerSelectionCommand { get; }
        public ICommand ManageCustomServersCommand { get; }

        public GameLauncherViewModel()
        {
            PlayCommand = new RelayCommand(LaunchGame, CanLaunchGame);
            ZoneCommand = new RelayCommand(ZoneClicked);
            GameLocationCommand = new RelayCommand(GameLocationClicked);
            UserAccountCommand = new RelayCommand(UserAccountClicked);
            ServerSelectionCommand = new RelayCommand(OpenServerSelection);
            ManageCustomServersCommand = new RelayCommand(ManageCustomServersClicked);
        }

        public bool InjectDecal
        {
            get => Properties.Settings.Default.InjectDecal;
            set
            {
                if (Properties.Settings.Default.InjectDecal != value)
                {
                    Properties.Settings.Default.InjectDecal = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(InjectDecal));
                    OnPropertyChanged(nameof(ServerDescription));
                }
            }
        }

        public bool RememberServerSelection
        {
            get => Properties.Settings.Default.RememberServerSelection;
            set
            {
                if (Properties.Settings.Default.RememberServerSelection != value)
                {
                    Properties.Settings.Default.RememberServerSelection = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(RememberServerSelection));
                }
            }
        }

        public string ClientFileLocation
        {
            get => GetACLocation();
            set
            {
                if (Properties.Settings.Default.ACLocation != value)
                {
                    Properties.Settings.Default.ACLocation = value;
                    Properties.Settings.Default.Save();
                    OnPropertyChanged(nameof(ClientFileLocation));
                    (PlayCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public string ServerDescription
        {
            get 
            {
                var serverName = Properties.Settings.Default.ServerName ?? "";
                var description = Properties.Settings.Default.ServerDescription ?? "";
                var injectDecal = Properties.Settings.Default.InjectDecal;
                
                if (string.IsNullOrEmpty(serverName))
                    return description;
                
                // Add Decal injection status to server name
                var displayName = injectDecal ? $"{serverName} - with Decal Injection" : serverName;
                
                if (string.IsNullOrEmpty(description))
                    return displayName;
                
                return $"{displayName}\n\n{description}";
            }
        }

        public string BackgroundImageSource
        {
            get
            {
                var serverType = Properties.Settings.Default.ServerType ?? "PvE";
                
                // Return appropriate background based on server type
                if (serverType.Equals("PvP", StringComparison.OrdinalIgnoreCase) ||
                    serverType.Equals("PK", StringComparison.OrdinalIgnoreCase))
                {
                    return "/Assets/PvP_Background.bmp";
                }
                
                return "/Assets/PvE_Background.bmp";
            }
        }

        public void RefreshServerDescription()
        {
            // Force reload settings from disk to get the latest values
            Properties.Settings.Default.Reload();
            
            OnPropertyChanged(nameof(ServerDescription));
            OnPropertyChanged(nameof(BackgroundImageSource));
        }

        private string GetACLocation()
        {
            try { return Properties.Settings.Default.ACLocation; }
            catch { return @"C:\Turbine\Asheron's Call\acclient.exe"; }
        }

        private bool CanLaunchGame()
        {
            var path = ClientFileLocation;
            return !string.IsNullOrWhiteSpace(path) && File.Exists(path);
        }

        private void LaunchGame()
        {
            try
            {
                var acClientExeLocation = ClientFileLocation;

                if (String.IsNullOrEmpty(acClientExeLocation) || !File.Exists(acClientExeLocation))
                {
                    MessageBox.Show("AC Client location doesn't exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (String.IsNullOrEmpty(Properties.Settings.Default.ServerPath))
                {
                    MessageBox.Show("No host provided.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (String.IsNullOrEmpty(Properties.Settings.Default.UserAccount) || String.IsNullOrEmpty(Properties.Settings.Default.UserPassword))
                {
                    MessageBox.Show("Invalid account or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Parse server path to get host and port separately
                string serverHost = Properties.Settings.Default.ServerPath;
                string serverPort = "9000"; // Default port
                
                
                if (serverHost.Contains(":"))
                {
                    var parts = serverHost.Split(':');
                    serverHost = parts[0];
                    serverPort = parts[1];
                    
                    // If port is 0 or invalid, use default port 9000
                    if (serverPort == "0" || !int.TryParse(serverPort, out var parsedPort) || parsedPort <= 0)
                    {
                        serverPort = "9000";
                    }
                    else
                    {
                    }
                }
                else
                {
                }

                string arguments = "-h " + serverHost + " -p " + serverPort + 
                                 " -a " + Properties.Settings.Default.UserAccount + 
                                 " -v " + Properties.Settings.Default.UserPassword;

                if (!Properties.Settings.Default.InjectDecal)
                {
                    var startInfo = new ProcessStartInfo();
                    startInfo.FileName = acClientExeLocation;
                    startInfo.Arguments = arguments;
                    startInfo.CreateNoWindow = true;
                    startInfo.WorkingDirectory = Path.GetDirectoryName(acClientExeLocation) ?? throw new InvalidOperationException();

                    Process.Start(startInfo);
                }
                else
                {
                    // Get Decal location from registry
                    string decalInjectLocation;
                    try
                    {
                        decalInjectLocation = Injector.GetDecalLocation();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Decal installation not found in registry.\n\nError: {ex.Message}", "Decal Not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    bool success = Injector.RunSuspendedCommaInjectCommaAndResume(acClientExeLocation, arguments, decalInjectLocation, "DecalStartup");
                    
                    if (!success)
                    {
                        MessageBox.Show("Failed to launch game with Decal injection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to launch game:\n{ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ZoneClicked()
        {
            ChooseLauncherLocation();
            MessageBox.Show("You have clicked the Zone menu item.", "Zone Clicked", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        public void GameLocationClicked()
        {
            ChooseLauncherLocation();
        }
        
        private void UserAccountClicked()
        {
            var currentAccount = Properties.Settings.Default.UserAccount ?? "";
            var currentPassword = Properties.Settings.Default.UserPassword ?? "";
            var dialog = new AccountManagement(currentAccount, currentPassword);
            
            if (dialog.ShowDialog() == true)
            {
                Properties.Settings.Default.UserAccount = dialog.Username;
                Properties.Settings.Default.UserPassword = dialog.Password;
                Properties.Settings.Default.Save();
                MessageBox.Show("User account information has been updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        

        public void ChooseLauncherLocation()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                InitialDirectory = "C:\\Turbine\\Asheron's Call",
                DefaultExt = ".exe",
                Filter = "Executables (exe)|*.exe|All files (*.*)|*.*"
            };
            var result = dlg.ShowDialog();
            if (result == true) { ClientFileLocation = dlg.FileName; }
        }

        public void OpenServerSelection()
        {
            // Hide the game launcher window
            Application.Current.MainWindow?.Hide();
            
            var serverSelectionWindow = new ServerSelection();
            serverSelectionWindow.Closed += (s, e) =>
            {
                // Refresh server description in case a new server was selected
                RefreshServerDescription();
                // Show the game launcher window again when server selection closes
                Application.Current.MainWindow?.Show();
            };
            serverSelectionWindow.Show();
        }

        private void ManageCustomServersClicked()
        {
            var manageCustomServerWindow = new ManageCustomServerWindow();
            manageCustomServerWindow.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

