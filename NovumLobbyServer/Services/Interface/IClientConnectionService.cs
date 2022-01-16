﻿using System.Net;

namespace NovumLobbyServer.Services.Interface;

/// <summary>
/// Defines a service that's responsible for managing the initial listener that accepts connections
/// </summary>
/// <remarks>
/// The implementation of this service should not block at any instance
/// </remarks>
public interface IClientConnectionService : IBaseService
{
    /// <summary>
    /// Activates <see cref="IClientConnectionService"/> and allows for connections
    /// </summary>
    /// <param name="ipAddress">The IP address the service should bind to</param>
    /// <param name="port">The port number to listen on</param>
    /// <remarks>
    /// <see cref="IClientConnectionService"/> can accept a single <see cref="BeginListening(string, ushort)"/> at a time. Subsequent calls to this method will basically not work
    /// </remarks>
    Task BeginListening(string ipAddress, ushort port, CancellationToken cancellationToken);

    /// <summary>
    /// Activates <see cref="IClientConnectionService"/> and allows for connections
    /// </summary>
    /// <param name="ipAddress">The IP address the service should bind to</param>
    /// <param name="port">The port number to listen on</param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// <see cref="IClientConnectionService"/> can accept a single <see cref="BeginListening(string, ushort)"/> at a time. Subsequent calls to this method will basically not work
    /// </remarks>
    Task BeginListening(IPAddress ipAddress, ushort port, CancellationToken cancellationToken);

    /// <summary>
    /// Disables <see cref="IClientConnectionService"/> and no longer accepts more connections
    /// </summary>
    /// <remarks>
    /// As a programmer note, this is more like disabling a 'login' server rather than shutting down existing connections
    /// </remarks>
    void EndListening();
}