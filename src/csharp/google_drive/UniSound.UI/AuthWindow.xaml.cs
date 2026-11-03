using System;
using System.Windows;
using GoogleDriveClient;

namespace UniSound.UI
{
    public partial class AuthWindow : Window
    {
        public GoogleDriveService? DriveService { get; private set; }

        public AuthWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string userId = txtUserId.Text.Trim();
                string token = txtToken.Password.Trim();

                if (string.IsNullOrEmpty(token))
                {
                    token = "demo_unisound_oauth_token_2026";
                }

                CredentialStore store = cmbStoreType.SelectedIndex == 0 
                    ? new MemoryCredentialStore() 
                    : new LocalFileCredentialStore();

                DriveService = new GoogleDriveService(store);
                UserCredential cred = new UserCredential("unisound_app", token, "refresh_token_demo", 3600);
                
                DriveService.Authenticate(userId, cred);

                lblStatus.Text = "Автентифікація успішна!";
                lblStatus.Foreground = System.Windows.Media.Brushes.LightGreen;

                MessageBox.Show($"Успішне підключення до Google Drive!\nТип сховища: {store.StoreName}", "UniSound Auth", MessageBoxButton.OK, MessageBoxImage.Information);
                
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Помилка авторизації";
                lblStatus.Foreground = System.Windows.Media.Brushes.Red;
                MessageBox.Show($"Помилка авторизації: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}