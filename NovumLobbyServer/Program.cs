
using Common.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NovumLobbyServer.Entities;
using NovumLobbyServer.Services;
using NovumLobbyServer.Services.Interface;

namespace NovumLobbyServer
{
    class Program
    { 
        private static void Main(string[] args)
        {
            var serviceCollection = InitializeContainer();
            var provider = serviceCollection.BuildServiceProvider();

            var server = provider.GetRequiredService<Server>();
            
            server.Start();
        }

        private static IServiceCollection InitializeContainer()
        {
           
            return new ServiceCollection().AddLogging(cfg => cfg
                    .AddConsole())
                .AddSingleton<IClientProviderService, ClientProviderService>()
                .AddSingleton<IClientConnectionService, ClientConnectionService>()
                .AddTransient<PacketAsync>()
                .AddTransient<SubPacket>()
                .AddTransient<GamePacket>()
                .AddSingleton<Server>();
            
        }
    }
}