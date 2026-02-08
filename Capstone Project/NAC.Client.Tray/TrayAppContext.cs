using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NAC.Client.Core.Interfaces;
using NAC.Client.Core.Models;
using NAC.Client.Core.Services;
using NAC.Client.UI;

namespace Tray
{
    public sealed class TrayAppContext : ApplicationContext
    {
        private readonly INacService nacService;
        private readonly NotifyIcon trayIcon;

        private readonly ToolStripMenuItem statusLabel;
        private readonly ToolStripMenuItem loginToggleItem;
        
        private readonly Icon iconDisconnected;
        private readonly Icon iconConnected;
        private ToolStripMenuItem infoItem;

        public TrayAppContext()
        {
            nacService = new MockNacService();

            iconDisconnected = LoadIcon("disconnected.ico");
            iconConnected = LoadIcon("connected.ico");

            statusLabel = new ToolStripMenuItem("Status: Disconnected")
            {
                Enabled = false,
                ForeColor = Color.FromArgb(150, 150, 150),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };

            loginToggleItem = new ToolStripMenuItem("Login")
            {
                Font = new Font("Segoe UI", 9)
            };
            loginToggleItem.Click += OnLoginToggleClicked;
            infoItem = new ToolStripMenuItem("Connection Info")
            {
                Font = new Font("Segoe UI", 9),
                Enabled = false
            };

            infoItem.Click += (_, _) =>
            {
                if (nacService is MockNacService svc && svc.CurrentDeviceInfo != null)
                {
                    var d = svc.CurrentDeviceInfo;

                    MessageBox.Show(
                        $"Device Name: {d.DeviceName}\n" +
                        $"OS: {d.OperatingSystem}\n" +
                        $"IP Address: {d.IpAddress}\n" +
                        $"MAC Address: {d.MacAddress}\n" +
                        $"VPN Port: {d.VpnPort}\n" +
                        $"Connected At: {d.ConnectedAt}",
                        "Connection Information",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            };

            var menu = TrayMenuBuilder.Build(
                statusLabel,
                loginToggleItem,
                OnExitClicked
            );

            menu.Items.Insert(3, infoItem);

            trayIcon = new NotifyIcon
            {
                Text = "NAC Agent",
                Icon = iconDisconnected,
                ContextMenuStrip = menu,
                Visible = true
            };
        }

        private async void OnLoginToggleClicked(object? sender, EventArgs e)
        {
            var loginWindow = new LoginWindow(nacService);
            bool? loginResult = loginWindow.ShowDialog();

            if (loginResult != true)
            {
                RefreshTrayUi();
                return;
            }

            var phase2Request = new Phase2ValidationRequest
            {
                ClientVersion = "1.0.0",
                DeviceInfo = DeviceInfoProvider.Collect(8080)
            };

            var phase2Result = await nacService.ValidatePhase2Async(
                phase2Request,
                CancellationToken.None
            );

            if (!phase2Result.Success)
            {
                MessageBox.Show(
                    phase2Result.Message ?? "Phase 2 validation failed.",
                    "Phase 2 Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                await nacService.LogoutAsync();
                RefreshTrayUi();
                return;
            }

            MessageBox.Show(
                phase2Result.Message ?? "Phase 2 validation succeeded.",
                "Phase 2 Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            await nacService.StartHeartbeatAsync(CancellationToken.None);

            trayIcon.ShowBalloonTip(
                3000,
                "Access Granted",
                "You are now connected to the secure network.",
                ToolTipIcon.Info
            );

            RefreshTrayUi();
        }

        private void OnExitClicked(object? sender, EventArgs e)
        {
            trayIcon.Visible = false;
            trayIcon.Dispose();
            Application.Exit();
        }

        private void RefreshTrayUi()
        {
            if (nacService.IsConnected)
            {
                trayIcon.Icon = iconConnected;
                UpdateStatus("Status: Connected", Color.FromArgb(46, 204, 113));
                loginToggleItem.Text = "Disconnect";
                infoItem.Enabled = true;
            }
            else
            {
                trayIcon.Icon = iconDisconnected;
                UpdateStatus("Status: Disconnected", Color.FromArgb(150, 150, 150));
                loginToggleItem.Text = "Login";
                infoItem.Enabled = false;
            }
        }

        private void UpdateStatus(string text, Color color)
        {
            statusLabel.Text = text;
            statusLabel.ForeColor = color;
        }

        private static Icon LoadIcon(string fileName)
        {
            var asm = Assembly.GetExecutingAssembly();
            var resources = asm.GetManifestResourceNames();

            var resourcePath = resources
                .FirstOrDefault(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

            if (resourcePath == null)
            {
                MessageBox.Show(
                    $"Icon '{fileName}' not found.\n\nAvailable resources:\n{string.Join("\n", resources)}",
                    "Icon Load Error"
                );
                return SystemIcons.Shield;
            }

            try
            {
                using var stream = asm.GetManifestResourceStream(resourcePath);
                return stream != null ? new Icon(stream) : SystemIcons.Shield;
            }
            catch
            {
                return SystemIcons.Shield;
            }
        }
    }
}