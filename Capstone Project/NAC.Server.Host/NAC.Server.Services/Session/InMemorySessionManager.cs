using NAC.Server.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace NAC.Server.Services.Session
{
    public class InMemorySessionManager : ISessionManager
    {
        // 1. Existing Session Store
        private readonly ConcurrentDictionary<string, SessionInfo> _sessions = new();

        // 2. NEW: Failed Attempts Store (IP Address -> Count)
        private readonly ConcurrentDictionary<string, int> _failedAttempts = new();

        // 3. Configuration constant
        private const int MAX_FAILURES_BEFORE_BLOCK = 3;

        public void RegisterSession(string username, string ipAddress, string deviceId)
        {
            var session = new SessionInfo
            {
                Username = username,
                IpAddress = ipAddress,
                DeviceId = deviceId,
                LastHeartbeat = DateTime.UtcNow
            };

            _sessions.AddOrUpdate(deviceId, session, (key, oldValue) => session);
            Console.WriteLine($"[Session] Registered: {username} ({ipAddress})");
        }

        public void UpdateHeartbeat(string deviceId)
        {
            if (_sessions.TryGetValue(deviceId, out var session))
            {
                session.LastHeartbeat = DateTime.UtcNow;
            }
        }

        // ✅ THIS IS THE CORRECT IMPLEMENTATION
        public IEnumerable<string> GetExpiredSessions(TimeSpan threshold)
        {
            var expiredIps = new List<string>();
            var now = DateTime.UtcNow;

            foreach (var session in _sessions.Values)
            {
                if (now - session.LastHeartbeat > threshold)
                {
                    expiredIps.Add(session.IpAddress);
                    // Remove from session store
                    RemoveSession(session.DeviceId);
                }
            }
            return expiredIps;
        }

        public void RemoveSession(string deviceId)
        {
            _sessions.TryRemove(deviceId, out _);
        }

        // ---------------------------------------------------------
        // NEW METHODS FOR BLOCKING LOGIC
        // ---------------------------------------------------------

        public bool IsClientBlocked(string ipAddress)
        {
            if (_failedAttempts.TryGetValue(ipAddress, out int failures))
            {
                return failures >= MAX_FAILURES_BEFORE_BLOCK;
            }
            return false;
        }

        public int RegisterLoginFailure(string ipAddress)
        {
            // Atomically add 1 to the failure count
            return _failedAttempts.AddOrUpdate(ipAddress, 1, (key, old) => old + 1);
        }

        public void ResetLoginFailures(string ipAddress)
        {
            // If they log in successfully, wipe their record
            _failedAttempts.TryRemove(ipAddress, out _);
        }

        // ---------------------------------------------------------
        // INTERNAL HELPER CLASS
        // ---------------------------------------------------------
        private class SessionInfo
        {
            public string Username { get; set; }
            public string IpAddress { get; set; }
            public string DeviceId { get; set; }
            public DateTime LastHeartbeat { get; set; }
        }
    }
}