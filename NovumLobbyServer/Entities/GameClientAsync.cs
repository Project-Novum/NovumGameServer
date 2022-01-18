using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Common;
using Common.Entities;
using Common.Enumerations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NovumLobbyServer.Packets.Receive;
using NovumLobbyServer.Packets.Send;
using StackExchange.Redis.Extensions.Core.Abstractions;


namespace NovumLobbyServer.Entities;

public class GameClientAsync
{
    private readonly ILogger<GameClientAsync> _logger;
    private readonly TimeSpan _defaultPingTimeout;
    private readonly IServiceProvider _provider;
    private readonly IRedisDatabase _redis;
    private System.Timers.Timer _pingTimer;

    private uint _clientId;
    private string _clientSessionId;
    private int _clientUserId;

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

    public GameClientAsync(ILogger<GameClientAsync> logger, IServiceProvider provider, IRedisDatabase redis)
    {
        _logger = logger;
        _provider = provider;
        _redis = redis;
        double pingTime = 5000;
        _defaultPingTimeout = TimeSpan.FromMilliseconds(pingTime);
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
        _logger.LogTrace("Client #{@clientId} is connecting from {@_ipEndPoint}", clientId, _ipEndPoint);
        Task.Run(async () => await InternalConnectionLoop());

        _pingTimer = new System.Timers.Timer(_defaultPingTimeout.TotalMilliseconds)
        {
            AutoReset = true,
            Enabled = true
        };
        _pingTimer.Elapsed += PingTimer_Elapsed;
    }

    private TcpState GetState()
    {
        var foo = IPGlobalProperties.GetIPGlobalProperties()
            .GetActiveTcpConnections()
            .SingleOrDefault(x => x.LocalEndPoint.Equals(_tcpClient.Client.LocalEndPoint));
        return foo != null ? foo.State : TcpState.Unknown;
    }

    private async Task InternalConnectionLoop()
    {
        const int tickRate = 30; //Convert.ToInt32(simpleConfiguration.Get("tick", "30"));

        while (GetState() == TcpState.Established)
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
                    await Task.Delay(TimeSpan.FromMilliseconds(tickRate));
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

                break;
            }
            catch (ArgumentException argException)
            {
                _logger.LogError(argException,
                    $"The {nameof(GameClientAsync)} has thrown an error that more than likely involves communicating back to the Client");

                break;
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


                break;
            }
            catch (Exception hipException)
            {
                _logger.LogError(hipException,
                    $"Client #{_clientId} has thrown an error parsing a particular packet. Dumping out the contents for later inspection");
                break;
            }
        }
        
        _logger.LogTrace("Client #{@_clientId} has been closed safely", _clientId);
        _tcpClient.Close();
        _pingTimer.Stop();
        OnGameClientDisconnected?.Invoke(this, EventArgs.Empty);
    }

    private async Task HandleIncomingBasePacketAsync(PacketAsync packetAsync)
    {

        if (packetAsync.ConnectionType == PacketConnectionType.INITIAL_HANDSHAKE)
        {
            _blowfish = new Blowfish(GenerateBlowFishKey(packetAsync));
            PacketAsync response = _provider.GetRequiredService<PacketAsync>();
            response.WritePacket(HardCodedPacket.g_secureConnectionAcknowledgment);
            response.EncryptPacket(_blowfish);
            _logger.LogInformation("Sent the Handshake");
            await SendPacket(response);
            return;
        }

        packetAsync.DecryptPacket(_blowfish);

        _logger.LogInformation(packetAsync.ToString());

        foreach (var packet in packetAsync.SubPacketList)
        {
            if (packet.Type == SubPacketType.GAME_PACKET)
            {
                switch (packet.GamePacketAsync.Opcode)
                {
                    case 0x05:
                        await ProcessSessionAcknowledgement(packet);
                        break;
                }
            }

            /*if (packet.Type == SubPacketType.ZONE_PACKET2)
            {
                
                PongPacket pongPacket = new PongPacket();
                SubPacket subPacket = ActivatorUtilities.CreateInstance<SubPacket>(_provider);
                if (subPacket.Create(0x0008,0,pongPacket))
                {
                    PacketAsync response = ActivatorUtilities.CreateInstance<PacketAsync>(_provider);
                    response.SubPacketList.Add(subPacket);
                    response.BuildPacket(true, false);
                    //response.EncryptPacket(_blowfish);
                    _logger.LogInformation("Sending PONG {0}",pongPacket.ToString());
                    await SendPacket(response);
                }
            }*/

        }
    }

    private async Task ProcessSessionAcknowledgement(SubPacket packet)
    {
        SessionPacket session = new SessionPacket(packet);
        _logger.LogInformation(session.ToString());

        _clientSessionId = session.SessionId;

        var exist = await _redis.GetAsync<int>(session.SessionId);

        _logger.LogInformation("Found user id: {userId}", exist);
        _clientUserId = exist;
        
        AccountListPacket accountListPacket = AccountListPacket.CreateDefault();
        SubPacket subPacket = ActivatorUtilities.CreateInstance<SubPacket>(_provider);
        if (subPacket.Create(accountListPacket))
        {
            PacketAsync response = ActivatorUtilities.CreateInstance<PacketAsync>(_provider);
            response.SubPacketList.Add(subPacket);
            response.BuildPacket(true, false);
            response.EncryptPacket(_blowfish);
            _logger.LogInformation("Sending Account List {0}",accountListPacket.ToString());
           await SendPacket(response);
        }
    }

    private async void PingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        _logger.LogDebug("Client #{@_clientId} is ready to PING", _clientId);
        //await SendDataPacket(OpCodes.OPCODE_DATA_PING, new byte[0]);
        _logger.LogDebug("Client #{@_clientId} has finished pinging", _clientId);
    }

    private async Task SendPacket(PacketAsync packetAsync)
    {
        //_logger.LogInformation("Sending the following Packet {0}",ObjectDumper.Dump(packetAsync));
        await using MemoryStream responseStream = new MemoryStream();
        
        responseStream.Write(packetAsync.Header,0,0x10);
        responseStream.Write(packetAsync.Data,0,packetAsync.PacketSizeWithoutHeader);

        await _networkStream.WriteAsync(responseStream.ToArray(), 0, (int)responseStream.Length);
    }

    private byte[] GenerateBlowFishKey(PacketAsync packetAsync)
    {
        using MemoryStream memoryStream = new MemoryStream(packetAsync.Data);
        using BinaryReader binaryReader = new BinaryReader(memoryStream);
        binaryReader.BaseStream.Seek(0x34, SeekOrigin.Begin);
        //byte[] buff = new byte[0x40];
        string ticketPhrase =  Encoding.ASCII.GetString(binaryReader.ReadBytes(0x40)).Trim(new[] { '\0' });
        uint clientNumber = binaryReader.ReadUInt32();
        
        byte[] key;
        using MemoryStream memStream = new MemoryStream(0x2C);
        using BinaryWriter binWriter = new BinaryWriter(memStream);

        binWriter.Write((byte)0x78);
        binWriter.Write((byte)0x56);
        binWriter.Write((byte)0x34);
        binWriter.Write((byte)0x12);
        binWriter.Write(clientNumber);
        binWriter.Write((byte)0xE8);
        binWriter.Write((byte)0x03);
        binWriter.Write((byte)0x00);
        binWriter.Write((byte)0x00);
        binWriter.Write(Encoding.ASCII.GetBytes(ticketPhrase), 0, Encoding.ASCII.GetByteCount(ticketPhrase) >= 0x20 ? 0x20 : Encoding.ASCII.GetByteCount(ticketPhrase));
        byte[] nonMd5EdKey = memStream.GetBuffer();

        using MD5 md5Hash = MD5.Create();
        key = md5Hash.ComputeHash(nonMd5EdKey);

        return key;

    }
}