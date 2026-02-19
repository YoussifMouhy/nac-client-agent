using NAC.Server.Core.Interfaces;
using NAC.Server.Services.Firewall;
using NAC.Server.Services.Session;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 1. CONFIGURE KESTREL (LISTEN PORT 5000)
        builder.WebHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.ListenAnyIP(5000, listenOptions =>
            {
                listenOptions.UseHttps();
            });
        });

        // 2. REGISTER SERVICES
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        // -- A. Firewall Service
        builder.Services.AddSingleton<IFirewallService, PowerShellFirewallService>();

        // -- B. Session Manager
        builder.Services.AddSingleton<ISessionManager, InMemorySessionManager>();

        // -- C. Background Cleanup
        builder.Services.AddHostedService<SessionCleanupService>();

        var app = builder.Build();

        // 3. CONFIGURE PIPELINE
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.MapControllers();

        // 4. INITIALIZE FIREWALL ON STARTUP
        using (var scope = app.Services.CreateScope())
        {
            var firewall = scope.ServiceProvider.GetRequiredService<IFirewallService>();
            
            // ✅ Initialization: Lock the server down immediately
            firewall.InitializeFirewall(); 
            Console.WriteLine("[Startup] Server Started. Firewall Locked.");
        }

        // 5. ✅ GRACEFUL SHUTDOWN (The fix for Ctrl+C)
        // We register a callback that runs when the server is stopping.
        var lifetime = app.Lifetime;
        lifetime.ApplicationStopping.Register(() =>
        {
            // We need a manual scope here because the app's main scope is closing
            using (var scope = app.Services.CreateScope())
            {
                var firewall = scope.ServiceProvider.GetRequiredService<IFirewallService>();
                Console.WriteLine("\n[Shutdown] Cleaning up Firewall rules...");
                
                // Re-running InitializeFirewall resets the profile to Block 
                // and removes the login-port exceptions.
                firewall.InitializeFirewall(); 
                Console.WriteLine("[Shutdown] Firewall Reset. Goodbye.");
            }
        });

        //Task.Run(async () => {
        //    using var udpClient = new System.Net.Sockets.UdpClient();
        //    var endpoint = new System.Net.IPEndPoint(System.Net.IPAddress.Broadcast, 8888);
        //    byte[] data = System.Text.Encoding.UTF8.GetBytes("NAC_SERVER_HERE");

        //    while (true)
        //    {
        //        await udpClient.SendAsync(data, data.Length, endpoint);
        //        await Task.Delay(5000); // Shout every 5 seconds
        //    }
        //});

        app.Run();
    }
}