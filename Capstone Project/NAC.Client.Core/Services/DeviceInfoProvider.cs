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
            OperatingSystem = Environment.OSVersion.ToString(),
            IpAddress = GetLocalIp(),
            MacAddress = GetMacAddress(),
            VpnPort = vpnPort,
            ConnectedAt = DateTime.Now
        };
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