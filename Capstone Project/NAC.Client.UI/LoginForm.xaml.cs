using System;
using System.Threading;
using System.Windows;
using NAC.Client.Core.Interfaces;
using NAC.Client.Core.Models;

namespace NAC.Client.UI
{
    public partial class LoginWindow : Window
    {
        private readonly INacService nacService;

        public LoginWindow(INacService nacService)
        {
            InitializeComponent();
            this.nacService = nacService;
            PositionNearTray();
        }

        private void PositionNearTray()
        {
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 12;
            Top = workArea.Bottom - Height - 12;
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameBox.Text?.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Please enter username and password.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var request = new LoginRequest
            {
                Username = username,
                Password = password
            };

            try
            {
                IsEnabled = false;

                LoginResponse result = await nacService.LoginAsync(
                    request,
                    CancellationToken.None
                );

                // 🔑 THIS IS THE CORRECT PLACE
                if (!result.Success)
                {
                    // 🚫 DEVICE BLOCKED
                    if (result.IsBlocked)
                    {
                        MessageBox.Show(
                            result.Message ?? "This device has been blocked.",
                            "Access Blocked",
                            MessageBoxButton.OK,
                            MessageBoxImage.Stop
                        );

                        // Optional: disable login UI completely
                        ConnectButton.IsEnabled = false;
                        UsernameBox.IsEnabled = false;
                        PasswordBox.IsEnabled = false;

                        return;
                    }

                    // ❌ WRONG PASSWORD, ATTEMPTS LEFT
                    MessageBox.Show(
                        $"{result.Message}\n\nRemaining attempts: {result.RemainingAttempts}",
                        "Login Failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    return;
                }


                // ✅ SUCCESS PATH
                DialogResult = true;   // <—— REQUIRED for tray logic
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Connection Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                IsEnabled = true;
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordPlaceholder.Visibility =
                string.IsNullOrEmpty(PasswordBox.Password)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }


    }
}