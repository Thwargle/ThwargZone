using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace ThwargZone
{
    public partial class RemoveCustomServerWindow : Window
    {
        private List<ServerInfo> customServers;

        public RemoveCustomServerWindow()
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

        private void RemoveServer_Click(object sender, RoutedEventArgs e)
        {
            if (CustomServersListBox.SelectedItem is ServerInfo selectedServer)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove the server '{selectedServer.Name}'?",
                    "Confirm Removal",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        RemoveServerFromCustomXml(selectedServer.Name);
                        
                        // Refresh the list
                        LoadCustomServers();
                        
                        MessageBox.Show($"Server '{selectedServer.Name}' has been removed.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to remove server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a server to remove.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
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
    }
}
