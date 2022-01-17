
using Common.Entities;
using Database;
using Microsoft.EntityFrameworkCore;
using NovumLobbyServer;
using NovumLobbyServer.Services;
using NovumLobbyServer.Services.Interface;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;

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
        services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(
            hostingContext.Configuration.GetSection(nameof(RedisConfiguration)).Get<RedisConfiguration>());

        services.AddSingleton<IClientProviderService, ClientProviderService>()
            .AddSingleton<IClientConnectionService, ClientConnectionService>()
            .AddDbContextPool<DBContext>(x => 
                x.UseNpgsql(hostingContext.Configuration.GetConnectionString("DefaultConnection")))
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