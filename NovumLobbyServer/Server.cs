using System.Net;
using Microsoft.Extensions.Logging;
using NovumLobbyServer.Services.Interface;

namespace NovumLobbyServer;

public class Server
{
    private readonly ILogger<Server> _logger;
    
    private readonly IPAddress _ipAddress;
    private readonly ushort _port;
    private readonly IClientConnectionService clientConnectionService;

    public Server(ILogger<Server> logger, IClientConnectionService clientConnectionService)
    {
        _logger = logger;
        
        IPAddress.TryParse("0.0.0.0", out _ipAddress);
        ushort.TryParse("54994", out _port);
        
        this.clientConnectionService = clientConnectionService;
    }

    public void Start()
    {
        
        clientConnectionService.BeginListening(_ipAddress,_port);
        
    }
}