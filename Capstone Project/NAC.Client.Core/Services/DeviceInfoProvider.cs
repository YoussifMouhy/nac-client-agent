using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using NAC.Client.Core.Models;

namespace NAC.Client.Core.Services;

public static class DeviceInfoProvider
{
    public static DeviceInfo Collect(int vpnPort)
    {
        return new DeviceInfo
        {
            DeviceName = Environment.MachineName,
            OperatingSystem = GetFriendlyOsName(),
            IpAddress = GetLocalIp(),
            MacAddress = GetMacAddress(),
            VpnPort = vpnPort,
            ConnectedAt = DateTime.Now
        };
    }

    private static string GetFriendlyOsName()
    {
        // Converts "Microsoft Windows NT 10.0.26200.0" to "Windows 11 (26200)"
        var os = Environment.OSVersion;
        string version = os.Version.Build >= 22000 ? "11" : "10";
        return $"Windows {version} ({os.Version.Build})";
    }

    private static string GetLocalIp()
    {
        return Dns.GetHostAddresses(Dns.GetHostName())
            .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            ?.ToString() ?? "Unknown";
    }

    private static string GetMacAddress()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
            .Select(nic => nic.GetPhysicalAddress().ToString())
            .FirstOrDefault() ?? "Unknown";
    }
}