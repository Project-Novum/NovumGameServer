using System.Net;
using NovumLobbyServer.Services.Interface;

namespace NovumLobbyServer;

public class Server : BackgroundService
{
    private readonly ILogger<Server> _logger;
    
    private readonly IPAddress _ipAddress;
    private readonly ushort _port;
    private readonly IClientConnectionService _clientConnectionService;

    public Server(ILogger<Server> logger, IClientConnectionService clientConnectionService)
    {
        _logger = logger;
        
        IPAddress.TryParse("0.0.0.0", out _ipAddress);
        ushort.TryParse("54994", out _port);
        
        _clientConnectionService = clientConnectionService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _clientConnectionService.BeginListening(_ipAddress, _port, stoppingToken);
    }
}