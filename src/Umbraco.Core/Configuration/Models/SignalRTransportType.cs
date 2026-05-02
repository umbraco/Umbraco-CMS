namespace Umbraco.Cms.Core.Configuration.Models;

/// <summary>
/// Specifies the transport protocols available for SignalR connections.
/// </summary>
/// <remarks>
/// This enum mirrors <c>Microsoft.AspNetCore.Http.Connections.HttpTransportType</c> so that
/// <see cref="SignalRSettings"/> can live in Umbraco.Core, which does not reference the
/// ASP.NET Core shared framework. The integer values are kept identical to allow safe
/// flag-by-flag conversion via
/// <c>SignalRTransportTypeExtensions.ToHttpTransportType</c> in the web layer.
/// </remarks>
[Flags]
public enum SignalRTransportType
{
    /// <summary>
    /// No transport.
    /// </summary>
    None = 0,

    /// <summary>
    /// The WebSockets transport.
    /// </summary>
    WebSockets = 1,

    /// <summary>
    /// The Server-Sent Events transport.
    /// </summary>
    ServerSentEvents = 2,

    /// <summary>
    /// The HTTP Long Polling transport.
    /// </summary>
    LongPolling = 4,
}
