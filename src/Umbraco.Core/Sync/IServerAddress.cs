namespace Umbraco.Cms.Core.Sync;

/// <summary>
///     Provides the address of a server.
/// </summary>
public interface IServerAddress
{
    /// <summary>
    ///     Gets the server address.
    /// </summary>
    string? ServerAddress { get; }

    // TODO: Should probably add things like port, protocol, server name, app id
}
