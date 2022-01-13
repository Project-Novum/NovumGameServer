using System.Net;
using System.Net.Sockets;
using Common.Enumerations;
using Microsoft.Extensions.Logging;

using NovumLobbyServer.Services.Interface;

namespace NovumLobbyServer.Services;

public class ClientConnectionService : IClientConnectionService
{
    private readonly ILogger<ClientConnectionService> _logger;
    private readonly IClientProviderService clientProviderService;


    private TcpListener _listener;
    private CancellationTokenSource _tokenSource;
    private IPAddress _lastIpAddress;
    private ushort _lastPort;

    public string ServiceName => "Client Connection Service";

    public ServiceStatusEnum ServiceStatus { get; private set; }


    public ClientConnectionService(ILogger<ClientConnectionService> logger,
        IClientProviderService clientProviderService)
    {
        _logger = logger;
        this.clientProviderService = clientProviderService;
    }

    public void BeginListening(string ipAddress, ushort port)
    {
        if (!IPAddress.TryParse(ipAddress, out var ip)) throw new ArgumentException(nameof(ipAddress));
        this.BeginListening(ip, port);
    }

    public void BeginListening(IPAddress ipAddress, ushort port)
    {
        if (_listener != null)
        {
            throw new NotSupportedException($"Multiple calls to {nameof(BeginListening)} is not supported");
        }

        _logger.LogInformation("Opening TCP Listener on {@ipAddress}:{@port}", ipAddress.ToString(), port);
        _tokenSource = new CancellationTokenSource();
        _listener = new TcpListener(ipAddress, port);
        var t= Task.Run(async () => await InternalConnectionLoop(_tokenSource.Token));
        _lastIpAddress = ipAddress;
        _lastPort = port;
        t.Wait();
    }

    public void EndListening()
    {
        _logger.LogInformation($"The {nameof(ClientConnectionService)} has had a shutdown requested");
        _tokenSource.Cancel();
        _logger.LogInformation($"A cancellation request has been submitted");
    }

    public void RestartService()
    {
        // Don't restart if we're already running
        // or if the last known IP address is null
        if (this.ServiceStatus == ServiceStatusEnum.Active)
        {
            throw new NotSupportedException(
                "As ironic as it seems, you cannot currently restart the service while it is running");
        }

        if (_lastIpAddress is null)
        {
            throw new InvalidOperationException(
                "The service has not been run before and has no last known IP address to re-bind to");
        }

        _logger.LogInformation($"The {nameof(ClientConnectionService)} is being restarted...");
        BeginListening(_lastIpAddress, _lastPort);
        _logger.LogInformation($"The {nameof(ClientConnectionService)} has been restarted");
    }

    private async Task TEST()
    {
        _logger.LogInformation("TEST IS CALLED");
    }

    private async Task InternalConnectionLoop(CancellationToken token)
    {
        _logger.LogInformation("Loop Started");
        _listener.Start();
        uint clientIds = 1;
        using (token.Register(() =>
               {
                   this.ServiceStatus = ServiceStatusEnum.Inactive;
                   _listener.Stop();
                   _listener = null;
               }))
        {
            this.ServiceStatus = ServiceStatusEnum.Active;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _logger.LogTrace("Invoking AcceptTcpClientAsync()");
                    TcpClient incomingConnection = await _listener.AcceptTcpClientAsync();
                    _logger.LogTrace(
                        $"AcceptTcpClientAsync() has returned with a client, migrating to {nameof(IClientProviderService)}");
                    clientProviderService.AddClient(incomingConnection, clientIds++);
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
                    if (token.IsCancellationRequested)
                    {
                        _logger.LogError(ioe, $"The {nameof(ClientConnectionService)} was told to shutdown.");
                    }
                    else
                    {
                        _logger.LogError(ioe,
                            $"The {nameof(ClientConnectionService)} was not told to shutdown. Please present this log to someone to investigate what went wrong while executing the code");
                    }
                }
                catch (OperationCanceledException oce)
                {
                    _logger.LogError(oce,
                        $"The {nameof(ClientConnectionService)} was told to explicitly shutdown and no further action is necessary");
                }
            }

            this.ServiceStatus = ServiceStatusEnum.Inactive;
        }
    }
}