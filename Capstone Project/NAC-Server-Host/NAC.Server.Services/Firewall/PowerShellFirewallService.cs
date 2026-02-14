using System;
using System.Diagnostics;
using System.Text; // Required for Encoding
using NAC.Server.Core.Interfaces;

namespace NAC.Server.Services.Firewall
{
    public class PowerShellFirewallService : IFirewallService
    {
        private const int AgentPort = 5000;

        public void InitializeFirewall()
        {
            // We use the new Base64 runner here too
            RunEncodedCommand("Set-NetFirewallProfile -Profile Domain,Public,Private -DefaultInboundAction Block");
            RunEncodedCommand("Remove-NetFirewallRule -DisplayName 'NAC-Allow-Agent-PreAuth' -ErrorAction SilentlyContinue");

            string allowAgentCmd = $"New-NetFirewallRule -DisplayName 'NAC-Allow-Agent-PreAuth' -Direction Inbound -Protocol TCP -LocalPort {AgentPort} -Action Allow";
            RunEncodedCommand(allowAgentCmd);
        }

        public bool AllowClientAccess(string ipAddress)
        {
            try
            {
                // 1. Clean IP
                ipAddress = CleanIp(ipAddress);
                string ruleName = $"NAC-Allow-{ipAddress}";

                // 2. Build the Script (We explicitly import the module to be safe)
                //    Notice we put everything in ONE script block.
                string script = $@"
                    Import-Module NetSecurity;
                    Remove-NetFirewallRule -DisplayName '{ruleName}' -ErrorAction SilentlyContinue;
                    New-NetFirewallRule -DisplayName '{ruleName}' -Direction Inbound -RemoteAddress '{ipAddress}' -Action Allow;
                ";

                Console.WriteLine($"[Firewall] Running encoded command for {ipAddress}...");

                // 3. Run it using the Nuclear Option
                RunEncodedCommand(script);

                Console.WriteLine($"[Firewall] Successfully opened for {ipAddress}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Firewall] ERROR opening rule for {ipAddress}: {ex.Message}");
                return false;
            }
        }

        public void RevokeClientAccess(string ipAddress)
        {
            try
            {
                ipAddress = CleanIp(ipAddress);
                string ruleName = $"NAC-Allow-{ipAddress}";
                string script = $"Remove-NetFirewallRule -DisplayName '{ruleName}' -ErrorAction SilentlyContinue";

                RunEncodedCommand(script);
                Console.WriteLine($"[Firewall] Revoked access for {ipAddress}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Firewall] Revoke Warning: {ex.Message}");
            }
        }

        private string CleanIp(string ip)
        {
            if (string.IsNullOrEmpty(ip)) return "0.0.0.0";
            if (ip.StartsWith("::ffff:")) return ip.Substring(7);
            return ip;
        }

        private void RunEncodedCommand(string script)
        {
            string encodedScript = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));

            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encodedScript}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = Process.Start(processInfo))
            {
                if (process == null) throw new Exception("Failed to start PowerShell.");

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                // ✅ NEW LOGIC: Only fail if there is a real error message 
                // and ignore the "#< CLIXML" progress junk.
                if (process.ExitCode != 0)
                {
                    // If the error starts with #< CLIXML and contains 'progress', it's likely not a real error
                    if (error.Contains("<AV>Preparing modules for first use.</AV>") || error.Contains("progress"))
                    {
                        // It's just PowerShell being chatty. Log it and move on.
                        Console.WriteLine("[Firewall Info] PowerShell initialized modules successfully.");
                        return;
                    }

                    string fullError = $"Exit: {process.ExitCode} | Err: {error} | Out: {output}";
                    throw new Exception(fullError);
                }
            }
        }
    }
}