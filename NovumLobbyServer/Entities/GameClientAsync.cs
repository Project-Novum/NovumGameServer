using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Common;
using Common.Entities;
using Common.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace NovumLobbyServer.Entities;

public class GameClientAsync
{
    private readonly ILogger<GameClientAsync> _logger;
    private readonly TimeSpan DefaultPingTimeout;
    private readonly IServiceProvider _provider;

    private CancellationTokenSource _tokenSource;
    private System.Timers.Timer _pingTimer;

    private uint _clientId;


    private NetworkStream _networkStream;
    private TcpClient _tcpClient;
    private IPEndPoint _ipEndPoint;

    private Blowfish _blowfish;

    #region Events

    /// <summary>
    /// Raised when the client is disconnecting
    /// </summary>
    public event EventHandler OnGameClientDisconnected;

    #endregion

    #region Properties

    public int ClientId => (int)_clientId;

    #endregion

    public GameClientAsync(ILogger<GameClientAsync> logger, IServiceProvider provider)
    {
        _logger = logger;
        _provider = provider;
        double pingTime = 5000;
        DefaultPingTimeout = TimeSpan.FromMilliseconds(pingTime);
    }

    /// <summary>
    /// Initializes the client with the appropriate seed data
    /// </summary>
    /// <param name="clientIndex">The local client index</param>
    /// <param name="tcpClient">The connected <see cref="TcpClient"/></param>
    public void InitializeClient(uint clientId, TcpClient tcpClient)
    {
        _logger.LogInformation("Client #{@clientId} is being initialized", clientId);
        _tcpClient = tcpClient;
        _clientId = clientId;
        _networkStream = _tcpClient.GetStream();
        _networkStream.ReadTimeout = 100;
        _networkStream.WriteTimeout = 100;
        _ipEndPoint = (IPEndPoint)_tcpClient.Client.RemoteEndPoint;
        _tokenSource = new CancellationTokenSource();
        _logger.LogTrace("Client #{@clientId} is connecting from {@_ipEndPoint}", clientId, _ipEndPoint);
        Task.Run(async () => await InternalConnectionLoop(_tokenSource.Token));

        _pingTimer = new System.Timers.Timer(DefaultPingTimeout.TotalMilliseconds)
        {
            AutoReset = true,
            Enabled = true
        };
        _pingTimer.Elapsed += PingTimer_Elapsed;
    }

    private async Task InternalConnectionLoop(CancellationToken token)
    {
        const int tickRate = 30; //Convert.ToInt32(simpleConfiguration.Get("tick", "30"));

        await using (token.Register(() =>
                     {
                         _logger.LogTrace("Client #{@_clientId} has been closed safely", _clientId);
                         _tcpClient.Close();
                         OnGameClientDisconnected?.Invoke(this, EventArgs.Empty);
                     }))
        {
            while (!token.IsCancellationRequested)
            {
                var packet = _provider.GetRequiredService<PacketAsync>();
                
                try
                {
                    var readResult = await packet.ReadPacketAsync(_networkStream);

                    if (!readResult)
                    {
                        _logger.LogTrace(
                            "Client #{@_clientId} has no data at this time; suspending for {@tickRate} milliseconds",
                            _clientId, tickRate);
                        await Task.Delay(TimeSpan.FromMilliseconds(tickRate), token);
                    }
                    else
                    {
                        await HandleIncomingBasePacketAsync(packet);
                    }
                }
                catch (ObjectDisposedException ode)
                {
                    _logger.LogError(ode,
                        $"The {nameof(GameClientAsync)} was told to shutdown or threw some sort of error; cleaning up the Client");

                    _tokenSource.Cancel();
                }
                catch (ArgumentException argException)
                {
                    _logger.LogError(argException,
                        $"The {nameof(GameClientAsync)} has thrown an error that more than likely involves communicating back to the Client");
                    
                    _tokenSource.Cancel();
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
                        $"The {nameof(GameClientAsync)} was told to shutdown, or errored, before an incoming packet was read. More context is necessary to see if this Error can be safely ignored");
                    
                    _logger.LogError(ioe,
                        token.IsCancellationRequested
                            ? $"The {nameof(GameClientAsync)} was told to shutdown via the cancellation token. This error can more than likely be discarded."
                            : $"The {nameof(GameClientAsync)} was not told to shutdown. Please present this log to someone to investigate what went wrong while executing the code");
                    
                    _tokenSource.Cancel();
                }
                catch (Exception hipException)
                {
                    _logger.LogError(hipException,
                        $"Client #{_clientId} has thrown an error parsing a particular packet. Dumping out the contents for later inspection");
                    
                    _tokenSource.Cancel();
                    /*_logger.LogData(packet.Data, packet.Code, (int)clientIndex, "", packet.ChecksumInPacket,
                        packet.ChecksumOfPacket);*/
                }
            }
        }
    }

    private async Task HandleIncomingBasePacketAsync(PacketAsync packetAsync)
    {

        if (packetAsync.ConnectionType == PacketConnectionType.INITIAL_HANDSHAKE)
        {
            _blowfish = new Blowfish(GenerateBlowFishKey(packetAsync));
            PacketAsync response = _provider.GetRequiredService<PacketAsync>();
            response.WritePacket(HardCodedPacket.g_secureConnectionAcknowledgment);
            response.EncryptPacket(_blowfish);

            await SendPacket(response);
            return;
        }
        else
        {
            packetAsync.DecryptPacket(_blowfish);

            _logger.LogInformation(packetAsync.ToString());
        }
    }

    private async void PingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (_tokenSource.IsCancellationRequested)
        {
            _pingTimer.Dispose();
            return;
        }

        _logger.LogDebug("Client #{@_clientId} is ready to PING", _clientId);
        //await SendDataPacket(OpCodes.OPCODE_DATA_PING, new byte[0]);
        _logger.LogDebug("Client #{@_clientId} has finished pinging", _clientId);
    }

    private async Task SendPacket(PacketAsync packetAsync)
    {
        using MemoryStream responseStream = new MemoryStream();
        
        responseStream.Write(packetAsync.Header,0,0x10);
        responseStream.Write(packetAsync.Data,0,packetAsync.PacketSizeWithoutHeader);

        await _networkStream.WriteAsync(responseStream.ToArray(), 0, (int)responseStream.Length);

    }

    private byte[] GenerateBlowFishKey(PacketAsync packetAsync)
    {
        using MemoryStream memoryStream = new MemoryStream(packetAsync.Data);
        using BinaryReader binaryReader = new BinaryReader(memoryStream);
        binaryReader.BaseStream.Seek(0x34, SeekOrigin.Begin);
        byte[] buff = new byte[0x40];
        string ticketPhrase =  Encoding.ASCII.GetString(binaryReader.ReadBytes(0x40)).Trim(new[] { '\0' });
        uint clientNumber = binaryReader.ReadUInt32();
        
        byte[] key;
        using (MemoryStream memStream = new MemoryStream(0x2C))
        {
            using (BinaryWriter binWriter = new BinaryWriter(memStream))
            {
                binWriter.Write((Byte)0x78);
                binWriter.Write((Byte)0x56);
                binWriter.Write((Byte)0x34);
                binWriter.Write((Byte)0x12);
                binWriter.Write((UInt32)clientNumber);
                binWriter.Write((Byte)0xE8);
                binWriter.Write((Byte)0x03);
                binWriter.Write((Byte)0x00);
                binWriter.Write((Byte)0x00);
                binWriter.Write(Encoding.ASCII.GetBytes(ticketPhrase), 0, Encoding.ASCII.GetByteCount(ticketPhrase) >= 0x20 ? 0x20 : Encoding.ASCII.GetByteCount(ticketPhrase));                    
            }
            byte[] nonMD5edKey = memStream.GetBuffer();

            using (MD5 md5Hash = MD5.Create())
            {
                key = md5Hash.ComputeHash(nonMD5edKey);
            }
        }
        return key;

    }
}