using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.IO;

namespace ThwargZone
{
    /// <summary>
    /// Interaction logic for ServerSelection.xaml
    /// </summary>
    public partial class ServerSelection : Window
    {
        public ServerSelectionViewModel ViewModel { get; }

        public ServerSelection()
        {
            InitializeComponent();
            ViewModel = new ServerSelectionViewModel();
            DataContext = ViewModel;
            
            // Load server data when window loads
            Loaded += async (s, e) => await ViewModel.LoadServerDataAsync();

            ThwargZone.AppSettings.WpfWindowPlacementSetting.Persist(this);
        }

        private void Server_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBlock textBlock && textBlock.DataContext is ServerInfo server)
            {
                
                ViewModel.SelectedServer = server;
                ViewModel.ConnectToServer(this);
            }
        }

        private void GameHelp_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Game Help - This would open the game help documentation.", "Game Help", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MemberServices_Click(object sender, MouseButtonEventArgs e)
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

        private void Subscription_Click(object sender, MouseButtonEventArgs e)
        {
            var manageCustomServerWindow = new ManageCustomServerWindow();
            manageCustomServerWindow.ShowDialog();
        }

        private void Website_Click(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://asheronscall.com",
                UseShellExecute = true
            });
        }

        private void Exit_Click(object sender, MouseButtonEventArgs e)
        {
            // Close all windows and shut down the application
            foreach (Window window in Application.Current.Windows)
            {
                window.Close();
            }
            Application.Current.Shutdown();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the window by clicking on the Grid background (not on child controls)
            if (e.LeftButton == MouseButtonState.Pressed && e.Source == sender)
            {
                try
                {
                    this.DragMove();
                }
                catch (InvalidOperationException)
                {
                    // DragMove can only be called when mouse is pressed, ignore if not
                }
            }
        }
    }

    public class ServerSelectionViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private ServerInfo _selectedServer;

        public ObservableCollection<ServerInfo> Servers { get; }
        public int TotalPopulation => Servers?.Sum(s => s.PlayerCount) ?? 0;

        public ServerInfo SelectedServer
        {
            get => _selectedServer;
            set
            {
                _selectedServer = value;
                OnPropertyChanged(nameof(SelectedServer));
            }
        }

        public ServerSelectionViewModel()
        {
            _httpClient = new HttpClient();
            Servers = new ObservableCollection<ServerInfo>();
        }

        public async Task LoadServerDataAsync()
        {
            try
            {
                var customServers = new List<ServerInfo>();
                var officialServers = new List<ServerInfo>();

                // Load custom servers from local XML
                try
                {
                    customServers = LoadCustomServers();
                }
                catch (Exception ex)
                {
                }

                // Load servers from remote XML
                try
                {
                    var serversXml = await _httpClient.GetStringAsync("https://raw.githubusercontent.com/acresources/serverslist/master/Servers.xml");
                    officialServers = ParseServersXml(serversXml);
                }
                catch (Exception ex)
                {
                }

                // Combine all servers for player count matching
                var allServers = customServers.Concat(officialServers).ToList();

                // Load player counts from JSON
                var playerCountsJson = await _httpClient.GetStringAsync("http://treestats.net/player_counts-latest.json");
                var playerCounts = JsonConvert.DeserializeObject<PlayerCountData[]>(playerCountsJson);

                // Match servers with player counts
                int matchedCount = 0;
                foreach (var server in allServers)
                {
                    var playerCount = playerCounts?.FirstOrDefault(pc => 
                        string.Equals(pc.server, server.Name, StringComparison.OrdinalIgnoreCase));
                    
                    if (playerCount != null)
                    {
                        server.PlayerCount = playerCount.count;
                        matchedCount++;
                    }
                }

                // Sort custom servers by player count (descending), then official servers by player count (descending)
                var sortedCustomServers = customServers.OrderByDescending(s => s.PlayerCount).ToList();
                var sortedOfficialServers = officialServers.OrderByDescending(s => s.PlayerCount).ToList();
                var finalServerList = sortedCustomServers.Concat(sortedOfficialServers).ToList();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Servers.Clear();
                    foreach (var server in finalServerList)
                    {
                        Servers.Add(server);
                    }
                    OnPropertyChanged(nameof(TotalPopulation));
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load server data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private List<ServerInfo> ParseServersXml(string xmlContent)
        {
            var servers = new List<ServerInfo>();
            
            try
            {
                var doc = XDocument.Parse(xmlContent);
                var serverElements = doc.Descendants("ServerItem");

                foreach (var serverElement in serverElements)
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
                        servers.Add(server);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to parse servers XML: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return servers;
        }

        private List<ServerInfo> LoadCustomServers()
        {
            var servers = new List<ServerInfo>();
            var customServersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customServers.xml");
            
            if (!File.Exists(customServersPath))
            {
                return servers; // Return empty list if file doesn't exist
            }
            
            try
            {
                var doc = XDocument.Load(customServersPath);
                var serverElements = doc.Descendants("ServerItem");

                foreach (var serverElement in serverElements)
                {
                    var name = serverElement.Element("name")?.Value ?? "";
                    var host = serverElement.Element("server_host")?.Value ?? "";
                    var portString = serverElement.Element("server_port")?.Value ?? "";
                    var type = serverElement.Element("type")?.Value ?? "";
                    var description = serverElement.Element("description")?.Value ?? "";
                    
                    
                    var server = new ServerInfo
                    {
                        Name = name,
                        Host = host,
                        Port = int.TryParse(portString, out var port) && port > 0 ? port : 9000,
                        Type = type,
                        Description = description
                    };


                    if (!string.IsNullOrEmpty(server.Name))
                    {
                        servers.Add(server);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return servers;
        }

        public void ConnectToServer(Window serverSelectionWindow)
        {
            if (SelectedServer != null)
            {
                // Save server path, name, description, and type to settings
                // Handle case where Host might already contain a port (e.g., "162.154.96.85:9000")
                string cleanHost = SelectedServer.Host;
                if (cleanHost.Contains(":"))
                {
                    // Extract just the host part before the colon
                    cleanHost = cleanHost.Split(':')[0];
                }
                
                var serverPath = $"{cleanHost}:{SelectedServer.Port}";
                Properties.Settings.Default.ServerPath = serverPath;
                Properties.Settings.Default.ServerName = SelectedServer.DisplayName;
                Properties.Settings.Default.ServerDescription = SelectedServer.Description ?? "";
                Properties.Settings.Default.ServerType = string.IsNullOrEmpty(SelectedServer.Type) ? "PvE" : SelectedServer.Type;
                Properties.Settings.Default.Save();

                // Check if GameLauncher already exists
                GameLauncher gameLauncher = null;
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is GameLauncher gl)
                    {
                        gameLauncher = gl;
                        break;
                    }
                }

                // If GameLauncher exists, show it; otherwise create a new one
                if (gameLauncher != null)
                {
                    gameLauncher.Show();
                }
                else
                {
                    gameLauncher = new GameLauncher();
                    gameLauncher.Show();
                    Application.Current.MainWindow = gameLauncher;
                }
                
                // Close the server selection window
                serverSelectionWindow.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
