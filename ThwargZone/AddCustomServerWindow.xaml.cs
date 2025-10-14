using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace ThwargZone
{
    public partial class AddCustomServerWindow : Window
    {
        private ServerInfo? editingServer;

        public AddCustomServerWindow()
        {
            InitializeComponent();
            
            // Set default values
            ServerPortTextBox.Text = "9000";
            ServerTypeComboBox.SelectedIndex = 0; // PvE
            
            ThwargZone.AppSettings.WpfWindowPlacementSetting.Persist(this);
        }

        public AddCustomServerWindow(ServerInfo server) : this()
        {
            editingServer = server;
            
            // Populate fields with existing server data
            ServerNameTextBox.Text = server.Name;
            ServerHostTextBox.Text = server.Host;
            ServerPortTextBox.Text = server.Port.ToString();
            DescriptionTextBox.Text = server.Description;
            
            // Set the server type
            for (int i = 0; i < ServerTypeComboBox.Items.Count; i++)
            {
                if (ServerTypeComboBox.Items[i] is System.Windows.Controls.ComboBoxItem item && 
                    item.Content.ToString() == server.Type)
                {
                    ServerTypeComboBox.SelectedIndex = i;
                    break;
                }
            }
            
            // Update window title and button text
            Title = "Edit Custom Server";
            AddServerButton.Content = "Save";
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
            DialogResult = false;
            Close();
        }

        private void AddServer_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(ServerNameTextBox.Text))
            {
                MessageBox.Show("Please enter a server name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ServerHostTextBox.Text))
            {
                MessageBox.Show("Please enter a server host.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ServerPortTextBox.Text) || !int.TryParse(ServerPortTextBox.Text, out int port))
            {
                MessageBox.Show("Please enter a valid server port.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (ServerTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a server type.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (editingServer != null)
                {
                    // Update existing server
                    UpdateServerInCustomXml(
                        editingServer.Name,
                        ServerNameTextBox.Text.Trim(),
                        ServerHostTextBox.Text.Trim(),
                        port,
                        ((System.Windows.Controls.ComboBoxItem)ServerTypeComboBox.SelectedItem).Content.ToString(),
                        DescriptionTextBox.Text.Trim()
                    );
                    MessageBox.Show("Custom server updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Add new server
                    AddServerToCustomXml(
                        ServerNameTextBox.Text.Trim(),
                        ServerHostTextBox.Text.Trim(),
                        port,
                        ((System.Windows.Controls.ComboBoxItem)ServerTypeComboBox.SelectedItem).Content.ToString(),
                        DescriptionTextBox.Text.Trim()
                    );
                    MessageBox.Show("Custom server added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to {(editingServer != null ? "update" : "add")} custom server: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddServerToCustomXml(string name, string host, int port, string type, string description)
        {
            var customServersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customServers.xml");
            
            XDocument doc;
            if (File.Exists(customServersPath))
            {
                doc = XDocument.Load(customServersPath);
            }
            else
            {
                doc = new XDocument(new XElement("Servers"));
            }

            // Check if server with same name already exists
            var existingServer = doc.Root?.Elements("ServerItem")
                .FirstOrDefault(s => s.Element("name")?.Value.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
            
            if (existingServer != null)
            {
                throw new InvalidOperationException($"A server with the name '{name}' already exists.");
            }

            // Add new server
            var newServer = new XElement("ServerItem",
                new XElement("name", name),
                new XElement("server_host", host),
                new XElement("server_port", port.ToString()),
                new XElement("type", type),
                new XElement("description", description)
            );

            doc.Root?.Add(newServer);
            doc.Save(customServersPath);
        }

        private void UpdateServerInCustomXml(string oldName, string newName, string host, int port, string type, string description)
        {
            var customServersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "customServers.xml");
            
            if (!File.Exists(customServersPath))
            {
                throw new InvalidOperationException("Custom servers file not found.");
            }

            var doc = XDocument.Load(customServersPath);
            
            // Find the server to update
            var serverToUpdate = doc.Root?.Elements("ServerItem")
                .FirstOrDefault(s => s.Element("name")?.Value.Equals(oldName, StringComparison.OrdinalIgnoreCase) == true);
            
            if (serverToUpdate == null)
            {
                throw new InvalidOperationException($"Server '{oldName}' not found in custom servers.");
            }

            // Check if new name conflicts with existing server (if name changed)
            if (!oldName.Equals(newName, StringComparison.OrdinalIgnoreCase))
            {
                var conflictingServer = doc.Root?.Elements("ServerItem")
                    .FirstOrDefault(s => s.Element("name")?.Value.Equals(newName, StringComparison.OrdinalIgnoreCase) == true);
                
                if (conflictingServer != null)
                {
                    throw new InvalidOperationException($"A server with the name '{newName}' already exists.");
                }
            }

            // Update server data
            serverToUpdate.Element("name")?.SetValue(newName);
            serverToUpdate.Element("server_host")?.SetValue(host);
            serverToUpdate.Element("server_port")?.SetValue(port.ToString());
            serverToUpdate.Element("type")?.SetValue(type);
            serverToUpdate.Element("description")?.SetValue(description);

            doc.Save(customServersPath);
        }
    }
}
