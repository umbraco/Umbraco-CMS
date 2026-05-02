using Microsoft.AspNetCore.Http.Connections;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Management.Extensions;

/// <summary>
/// Extension methods for converting <see cref="SignalRTransportType"/>
/// to <see cref="HttpTransportType"/>.
/// </summary>
internal static class SignalRTransportTypeExtensions
{
    /// <summary>
    /// Converts a <see cref="SignalRTransportType"/> flags value to the equivalent
    /// <see cref="HttpTransportType"/> flags value by mapping each flag individually.
    /// </summary>
    /// <param name="transportType">The Umbraco transport type flags to convert.</param>
    /// <returns>The equivalent <see cref="HttpTransportType"/> flags value.</returns>
    internal static HttpTransportType ToHttpTransportType(this SignalRTransportType transportType)
    {
        var result = HttpTransportType.None;

        if (transportType.HasFlag(SignalRTransportType.WebSockets))
        {
            result |= HttpTransportType.WebSockets;
        }

        if (transportType.HasFlag(SignalRTransportType.ServerSentEvents))
        {
            result |= HttpTransportType.ServerSentEvents;
        }

        if (transportType.HasFlag(SignalRTransportType.LongPolling))
        {
            result |= HttpTransportType.LongPolling;
        }

        return result;
    }
}
