namespace Umbraco.Cms.Core.Sync;

/// <summary>
///     The role of a server in an application environment.
/// </summary>
public enum ServerRole : byte
{
    /// <summary>
    ///     The server role is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    ///     The server is the single server of a single-server environment.
    /// </summary>
    Single = 1,

    /// <summary>
    ///     In a multi-servers environment, the server is a Subscriber server.
    /// </summary>
    Subscriber = 2,

    /// <summary>
    ///     In a multi-servers environment, the server is the Scheduling Publisher.
    /// </summary>
    SchedulingPublisher = 3,
}
