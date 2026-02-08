using NAC.Client.Core.Interfaces;
using NAC.Client.Core.Models;

namespace NAC.Client.Core.Services;

public sealed class MockNacService : INacService
{
    private const int MaxAttempts = 3;
    private int failedAttempts = 0;

    public bool IsConnected { get; private set; }
    public DeviceInfo? CurrentDeviceInfo { get; private set; }
    private Timer? heartbeatTimer;
    private string? sessionToken;

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken ct = default)
    {
        await Task.Delay(1000, ct);

        // 🔒 BLOCK CHECK FIRST
        if (failedAttempts >= MaxAttempts)
        {
            return new LoginResponse
            {
                Success = false,
                IsBlocked = true,
                RemainingAttempts = 0,
                Message = "This device has been blocked due to multiple failed attempts."
            };
        }

        // ✅ CORRECT CREDENTIALS
        if (request.Username == "admin" && request.Password == "admin")
        {
            failedAttempts = 0; // 🔁 reset on success

            return new LoginResponse
            {
                Success = true,
                Message = "Phase 1 passed. Proceed to device validation."
            };
        }

        // ❌ WRONG CREDENTIALS
        failedAttempts++;

        return new LoginResponse
        {
            Success = false,
            RemainingAttempts = Math.Max(0, MaxAttempts - failedAttempts),
            Message = "Invalid username or password"
        };
    }


    public async Task<Phase2ValidationResponse> ValidatePhase2Async(
        Phase2ValidationRequest request,
        CancellationToken ct = default)
    {
        await Task.Delay(1000, ct);

        bool versionOk = request.ClientVersion == "1.0.0";
        bool deviceOk = request.DeviceInfo != null;

        if (!versionOk || !deviceOk)
        {
            IsConnected = false;
            return new Phase2ValidationResponse
            {
                Success = false,
                Message = "Device does not meet security requirements."
            };
        }

        CurrentDeviceInfo = request.DeviceInfo;
        sessionToken = Guid.NewGuid().ToString(); // ✅ REQUIRED
        IsConnected = true;

        return new Phase2ValidationResponse
        {
            Success = true,
            Message = "Phase 2 passed. Network access granted."
        };
    }



    public Task StartHeartbeatAsync(CancellationToken cancellationToken)
    {
        heartbeatTimer = new Timer(async _ =>
        {
            await SendHeartbeatAsync();
        },
        null,
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(2));

        return Task.CompletedTask;
    }

    private async Task SendHeartbeatAsync()
    {
        if (!IsConnected || sessionToken == null)
            return;

        await Task.Delay(300); // simulate network

        // MOCK SERVER DECISION
        bool stillValid = true; // later this comes from server

        if (!stillValid)
        {
            IsConnected = false;
            StopHeartbeatAsync();
        }
    }

    public Task StopHeartbeatAsync()
    {
        heartbeatTimer?.Dispose();
        heartbeatTimer = null;
        sessionToken = null;
        return Task.CompletedTask;
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await StopHeartbeatAsync();
        IsConnected = false;
        CurrentDeviceInfo = null;
    }
}