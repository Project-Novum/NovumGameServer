
using Common.Entities;
using Database;
using Microsoft.EntityFrameworkCore;
using NovumLobbyServer;
using NovumLobbyServer.Services;
using NovumLobbyServer.Services.Interface;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(x =>
    {
        x.SetBasePath(Directory.GetCurrentDirectory());
        x.AddJsonFile("appsettings.json", optional: true);
        x.AddEnvironmentVariables();
        x.AddCommandLine(args);
    })
    .ConfigureServices((hostingContext, services) =>
    {
        services.AddSingleton<IClientProviderService, ClientProviderService>()
            .AddSingleton<IClientConnectionService, ClientConnectionService>()
            .AddDbContextPool<DBContext>(x => 
                x.UseMySQL(hostingContext.Configuration.GetConnectionString("DefaultConnection")))
            .AddTransient<PacketAsync>()
            .AddTransient<SubPacket>()
            .AddTransient<GamePacket>()
            .AddHostedService<Server>();
    })
    .ConfigureLogging((hostingContext, logging) => {
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
        logging.AddConsole();
    }); ;

 await host.RunConsoleAsync();