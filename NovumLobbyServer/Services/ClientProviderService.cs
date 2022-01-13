using System.Collections.ObjectModel;
using System.Net.Sockets;
using Common.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NovumLobbyServer.Entities;
using NovumLobbyServer.Services.Interface;

namespace NovumLobbyServer.Services;

public class ClientProviderService : IClientProviderService
{
    private readonly ILogger<ClientProviderService> _logger;
    private readonly List<GameClientAsync> _clients;
    
    private readonly IServiceProvider _provider;
    
    
    
    public string ServiceName => "Game Client Service";
    public ServiceStatusEnum ServiceStatus { get; private set; }
    
    public ReadOnlyCollection<GameClientAsync> Clients => _clients.AsReadOnly();

    
    
    public ClientProviderService(ILogger<ClientProviderService> logger, IServiceProvider provider)
    {
        _logger = logger;

        _provider = provider;
        _clients = new List<GameClientAsync>();
        ServiceStatus = ServiceStatusEnum.Active;
    }


    public void AddClient(TcpClient client, uint clientId)
    {
        
        GameClientAsync gameClient = _provider.GetRequiredService<GameClientAsync>();
        gameClient.InitializeClient(clientId, client);
        AddClient(gameClient);
    }

    public void AddClient(GameClientAsync gameClient)
    {
        _logger.LogInformation($"Client {gameClient.ClientId} has connected");
        _clients.Add(gameClient);
        _logger.LogInformation($"There are {_clients.Count} connected clients");
        gameClient.OnGameClientDisconnected += Client_OnGameClientDisconnected;
    }

    public void RemoveClient(uint clientId) => this.RemoveClient(_clients.Find(c => c.ClientId == (int)clientId));
    

    public void RemoveClient(GameClientAsync gameClient)
    {
        _logger.LogInformation($"Client {gameClient.ClientId} is disconnecting");
        _clients.Remove(gameClient);
        _logger.LogInformation($"There are {_clients.Count} connected clients");
    }

    public bool TryGetClient(uint index, out GameClientAsync gameClient)
    {
        gameClient = null;
        foreach (var c in _clients)
        {
            if (c.ClientId == index)
            {
                gameClient = c;
                return true;
            }
        }
        return false;
    }
    
    private void Client_OnGameClientDisconnected(object sender, EventArgs e)
    {
        if (!(sender is GameClientAsync client)) return;
        RemoveClient(client);
        
        client.OnGameClientDisconnected -= Client_OnGameClientDisconnected;
    }
}