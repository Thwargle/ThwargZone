using System.Windows;

namespace ThwargZone
{
    /// <summary>
    /// Interaction logic for AccountManagement.xaml
    /// </summary>
    public partial class AccountManagement : Window
    {
        public string Username { get; private set; } = "";
        public string Password { get; private set; } = "";

        public AccountManagement(string username = "", string password = "")
        {
            InitializeComponent();
            
            UsernameTextBox.Text = username;
            PasswordBox.Password = password;
            PasswordTextBox.Text = password; // Initialize the text box as well
            
            // Focus on username field
            Loaded += (s, e) => UsernameTextBox.Focus();
            
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
            DialogResult = false;
            Close();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            Username = UsernameTextBox.Text;
            // Get password from whichever field is currently visible
            Password = ShowPasswordCheckBox.IsChecked == true ? PasswordTextBox.Text : PasswordBox.Password;
            DialogResult = true;
            Close();
        }

        private void Support_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement support functionality
            MessageBox.Show("Support functionality not yet implemented.", "Support", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Show password as plain text
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordTextBox.Visibility = Visibility.Visible;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Hide password with bubbles
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordTextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
        }
    }
}
