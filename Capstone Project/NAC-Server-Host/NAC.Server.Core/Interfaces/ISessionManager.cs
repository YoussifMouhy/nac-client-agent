using NAC.Server.Core.Models;
using System.Collections.Generic;
using System;

namespace NAC.Server.Core.Interfaces
{
    public interface ISessionManager
    {
        void RegisterSession(string username, string ipAddress, string deviceName);
        void UpdateHeartbeat(string deviceName);
        IEnumerable<string> GetExpiredSessions(TimeSpan timeout);
        void RemoveSession(string ipAddress);

        // 👇 THESE MUST BE HERE FOR THE CONTROLLER TO WORK 👇
        bool IsClientBlocked(string ipAddress);
        int RegisterLoginFailure(string ipAddress);
        void ResetLoginFailures(string ipAddress);
    }
}