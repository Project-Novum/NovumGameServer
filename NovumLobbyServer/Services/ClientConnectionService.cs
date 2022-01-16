using System.Net;
using System.Net.Sockets;
using Common.Enumerations;

using NovumLobbyServer.Services.Interface;

namespace NovumLobbyServer.Services;

public class ClientConnectionService : IClientConnectionService
{
    private readonly ILogger<ClientConnectionService> _logger;
    private readonly IClientProviderService _clientProviderService;


    private TcpListener _listener;
    private CancellationToken _cancellationToken;

    public string ServiceName => "Client Connection Service";

    public ServiceStatusEnum ServiceStatus { get; private set; }


    public ClientConnectionService(ILogger<ClientConnectionService> logger,
        IClientProviderService clientProviderService)
    {
        _logger = logger;
        _clientProviderService = clientProviderService;
    }

    public async Task BeginListening(string ipAddress, ushort port, CancellationToken cancellationToken)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip)) throw new ArgumentException(nameof(ipAddress));
        await BeginListening(ip!, port, cancellationToken);
    }

    public async Task BeginListening(IPAddress ipAddress, ushort port, CancellationToken cancellationToken)
    {
        if (_listener != null)
        {
            throw new NotSupportedException($"Multiple calls to {nameof(BeginListening)} is not supported");
        }

        _logger.LogInformation("Opening TCP Listener on {@ipAddress}:{@port}", ipAddress.ToString(), port);
        _cancellationToken = cancellationToken;
        _listener = new TcpListener(ipAddress, port);

        await InternalConnectionLoop();
    }

    public void EndListening()
    {
        _logger.LogInformation($"The {nameof(ClientConnectionService)} has had a shutdown requested");

        ServiceStatus = ServiceStatusEnum.Inactive;
        _listener.Stop();

        _logger.LogInformation($"{nameof(ClientConnectionService)} has been shutdown and all clients **saved**!");
    }

    private async Task InternalConnectionLoop()
    {
        _logger.LogInformation("Game lobby loop has started up");
        _listener.Start();
        uint clientIds = 1;
        ServiceStatus = ServiceStatusEnum.Active;
        while (!_cancellationToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogTrace("Invoking AcceptTcpClientAsync()");
                TcpClient incomingConnection = await _listener.AcceptTcpClientAsync(_cancellationToken);

                _logger.LogTrace(
                    $"AcceptTcpClientAsync() has returned with a client, migrating to {nameof(IClientProviderService)}");
                _clientProviderService.AddClient(incomingConnection, clientIds++);

                _logger.LogTrace("Client Provider Service now has the client");
            }
            catch (ObjectDisposedException ode)
            {
                _logger.LogError(ode,
                    "The service was told to shutdown, or errored, after an incoming connection attempt was made. It is probably safe to ignore this Error as the listener is already shutting down");
            }
            catch (InvalidOperationException ioe)
            {
                // Either tcpListener.Start wasn't called (a bug!)
                // or the CancellationToken was cancelled before
                // we started accepting (giving an InvalidOperationException),
                // or the CancellationToken was cancelled after
                // we started accepting (giving an ObjectDisposedException).
                //
                // In the latter two cases we should surface the cancellation
                // exception, or otherwise rethrow the original exception.
                _logger.LogError(ioe,
                    $"The {nameof(ClientConnectionService)} was told to shutdown, or errored, before an incoming connection attempt was made. More context is necessary to see if this Error can be safely ignored");

                _logger.LogError(ioe,
                    _cancellationToken.IsCancellationRequested
                        ? $"The {nameof(ClientConnectionService)} was told to shutdown."
                        : $"The {nameof(ClientConnectionService)} was not told to shutdown. Please present this log to someone to investigate what went wrong while executing the code");
            }
            catch (OperationCanceledException oce)
            {
                _logger.LogWarning(oce,
                    $"The {nameof(ClientConnectionService)} was told to explicitly shutdown and no further action is necessary");
            }
        }

        EndListening();
    }
}