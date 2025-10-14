using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ThwargZone
{
    public partial class ManageCustomServerWindow : Window
    {
        private List<ServerInfo> customServers;

        public ManageCustomServerWindow()
        {
            InitializeComponent();
            LoadCustomServers();
            
            ThwargZone.AppSettings.WpfWindowPlacementSetting.Persist(this);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.Source == sender)
            {
                DragMove();
            }
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoadCustomServers()
        {
            try
            {
                var customServersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customServers.xml");
                
                if (!File.Exists(customServersPath))
                {
                    ShowNoServersMessage();
                    return;
                }

                var doc = XDocument.Load(customServersPath);
                customServers = new List<ServerInfo>();

                foreach (var serverElement in doc.Root?.Elements("ServerItem") ?? Enumerable.Empty<XElement>())
                {
                    var server = new ServerInfo
                    {
                        Name = serverElement.Element("name")?.Value ?? "",
                        Host = serverElement.Element("server_host")?.Value ?? "",
                        Port = int.TryParse(serverElement.Element("server_port")?.Value, out var port) ? port : 0,
                        Type = serverElement.Element("type")?.Value ?? "",
                        Description = serverElement.Element("description")?.Value ?? ""
                    };

                    if (!string.IsNullOrEmpty(server.Name))
                    {
                        customServers.Add(server);
                    }
                }

                if (customServers.Count == 0)
                {
                    ShowNoServersMessage();
                }
                else
                {
                    CustomServersListBox.ItemsSource = customServers;
                    CustomServersListBox.Visibility = Visibility.Visible;
                    NoServersText.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load custom servers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowNoServersMessage();
            }
        }

        private void ShowNoServersMessage()
        {
            CustomServersListBox.Visibility = Visibility.Collapsed;
            NoServersText.Visibility = Visibility.Visible;
        }

        private void AddServer_Click(object sender, RoutedEventArgs e)
        {
            var addServerWindow = new AddCustomServerWindow();
            if (addServerWindow.ShowDialog() == true)
            {
                LoadCustomServers(); // Refresh the list
            }
        }

        private void EditServer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ServerInfo server)
            {
                // Store the original server name before editing
                var originalServerName = server.Name;
                var currentServerName = Properties.Settings.Default.ServerName ?? "";
                
                System.Diagnostics.Debug.WriteLine($"=== EDITING SERVER ===");
                System.Diagnostics.Debug.WriteLine($"Original server name: '{originalServerName}'");
                System.Diagnostics.Debug.WriteLine($"Current selected server name: '{currentServerName}'");
                
                var editServerWindow = new AddCustomServerWindow(server);
                if (editServerWindow.ShowDialog() == true)
                {
                    LoadCustomServers(); // Refresh the list
                    
                    // Check if the edited server is the currently selected server
                    // We need to check against the original name since that's what was selected
                    // Also check if the ServerPath matches the original server (in case ServerName is empty)
                    var currentServerPath = Properties.Settings.Default.ServerPath ?? "";
                    var originalServerPath = $"{server.Host}:{server.Port}";
                    
                    bool isCurrentlySelected = string.Equals(currentServerName, originalServerName, StringComparison.OrdinalIgnoreCase) ||
                                             string.Equals(currentServerPath, originalServerPath, StringComparison.OrdinalIgnoreCase);
                    
                    if (isCurrentlySelected)
                    {
                        
                        // Find the updated server in the refreshed list
                        var updatedServer = customServers.FirstOrDefault(s => 
                            string.Equals(s.Name, editServerWindow.ServerNameTextBox.Text, StringComparison.OrdinalIgnoreCase));
                        
                        if (updatedServer != null)
                        {
                            // Update the settings with the new server information
                            UpdateGameLauncherSettings(updatedServer);
                        }
                        else
                        {
                        }
                    }
                    else
                    {
                    }
                }
            }
        }

        private void RemoveServer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ServerInfo server)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove the server '{server.Name}'?",
                    "Confirm Removal",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        RemoveServerFromCustomXml(server.Name);
                        
                        // Refresh the list
                        LoadCustomServers();
                        
                        MessageBox.Show($"Server '{server.Name}' has been removed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to remove server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void RemoveServerFromCustomXml(string serverName)
        {
            var customServersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customServers.xml");
            
            if (!File.Exists(customServersPath))
            {
                throw new InvalidOperationException("Custom servers file not found.");
            }

            var doc = XDocument.Load(customServersPath);
            
            var serverToRemove = doc.Root?.Elements("ServerItem")
                .FirstOrDefault(s => s.Element("name")?.Value.Equals(serverName, StringComparison.OrdinalIgnoreCase) == true);
            
            if (serverToRemove == null)
            {
                throw new InvalidOperationException($"Server '{serverName}' not found in custom servers.");
            }

            serverToRemove.Remove();
            doc.Save(customServersPath);
        }

        private void UpdateGameLauncherSettings(ServerInfo server)
        {
            try
            {
                // Construct the server path in the same format as ServerSelection.xaml.cs
                string cleanHost = server.Host;
                if (cleanHost.Contains(":"))
                {
                    // Extract just the host part before the colon
                    cleanHost = cleanHost.Split(':')[0];
                }
                
                var serverPath = $"{cleanHost}:{server.Port}";
                
                // Update the settings
                Properties.Settings.Default.ServerPath = serverPath;
                Properties.Settings.Default.ServerName = server.Name;
                Properties.Settings.Default.ServerDescription = server.Description ?? "";
                Properties.Settings.Default.ServerType = string.IsNullOrEmpty(server.Type) ? "PvE" : server.Type;
                Properties.Settings.Default.Save();
                
                
                // Small delay to ensure file system has time to write changes
                System.Threading.Thread.Sleep(100);
                
                // Close any open GameLauncher windows and navigate to ServerSelection
                CloseGameLauncherWindowsAndNavigateToServerSelection();
                
                // Also refresh any open ServerSelection windows to update their cached server lists
                RefreshServerSelectionWindows();
            }
            catch (Exception ex)
            {
            }
        }

        private void CloseGameLauncherWindowsAndNavigateToServerSelection()
        {
            try
            {
                
                // Find and close any open GameLauncher windows
                int gameLauncherCount = 0;
                var gameLauncherWindows = new List<GameLauncher>();
                
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is GameLauncher gameLauncher)
                    {
                        gameLauncherWindows.Add(gameLauncher);
                        gameLauncherCount++;
                    }
                }
                
                // Close all GameLauncher windows
                foreach (var gameLauncher in gameLauncherWindows)
                {
                    gameLauncher.Close();
                }
                
                
                // Check if ServerSelection window is already open
                bool serverSelectionExists = false;
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is ServerSelection)
                    {
                        serverSelectionExists = true;
                        window.Activate();
                        window.Focus();
                        break;
                    }
                }
                
                // If no ServerSelection window exists, create a new one
                if (!serverSelectionExists)
                {
                    var serverSelection = new ServerSelection();
                    serverSelection.Show();
                    serverSelection.Activate();
                    serverSelection.Focus();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private void RefreshGameLauncherWindows()
        {
            try
            {
                
                // Find and refresh any open GameLauncher windows
                int gameLauncherCount = 0;
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is GameLauncher gameLauncher)
                    {
                        gameLauncherCount++;
                        
                        // Trigger a refresh of the server description
                        if (gameLauncher.DataContext is GameLauncherViewModel viewModel)
                        {
                            viewModel.RefreshServerDescription();
                        }
                        else
                        {
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
            }
        }

        private void RefreshServerSelectionWindows()
        {
            try
            {
                
                // Find and refresh any open ServerSelection windows
                int serverSelectionCount = 0;
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is ServerSelection serverSelection)
                    {
                        serverSelectionCount++;
                        
                        // Trigger a refresh of the server list
                        if (serverSelection.DataContext is ServerSelectionViewModel viewModel)
                        {
                            
                            // Store the currently selected server name before reloading
                            string selectedServerName = viewModel.SelectedServer?.Name;
                            
                            // Reload the server data to get the updated custom servers
                            _ = viewModel.LoadServerDataAsync().ContinueWith(task =>
                            {
                                if (task.IsCompletedSuccessfully && !string.IsNullOrEmpty(selectedServerName))
                                {
                                    // Find the updated server in the new collection and update SelectedServer
                                    var updatedServer = viewModel.Servers.FirstOrDefault(s => 
                                        string.Equals(s.Name, selectedServerName, StringComparison.OrdinalIgnoreCase));
                                    
                                    if (updatedServer != null)
                                    {
                                        
                                        // Update the SelectedServer on the UI thread
                                        Application.Current.Dispatcher.Invoke(() =>
                                        {
                                            viewModel.SelectedServer = updatedServer;
                                        });
                                    }
                                    else
                                    {
                                    }
                                }
                            });
                            
                        }
                        else
                        {
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
            }
        }
    }
}
