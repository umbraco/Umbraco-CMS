using Serilog.Core;
using Serilog.Events;
using Umbraco.Cms.Core.Net;

namespace Umbraco.Cms.Core.Logging.Serilog.Enrichers;

/// <summary>
///     Enrich log events with the HttpSessionId property.
///     Original source -
///     https://github.com/serilog-web/classic/blob/master/src/SerilogWeb.Classic/Classic/Enrichers/HttpSessionIdEnricher.cs
///     Nupkg: 'Serilog.Web.Classic' contains handlers and extra bits we do not want
/// </summary>
public class HttpSessionIdEnricher : ILogEventEnricher
{
    /// <summary>
    ///     The property name added to enriched log events.
    /// </summary>
    public const string HttpSessionIdPropertyName = "HttpSessionId";

    private readonly ISessionIdResolver _sessionIdResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpSessionIdEnricher"/> class, which enriches Serilog log events with the current HTTP session ID.
    /// </summary>
    /// <param name="sessionIdResolver">An implementation used to resolve the current HTTP session ID for log enrichment.</param>
    public HttpSessionIdEnricher(ISessionIdResolver sessionIdResolver) => _sessionIdResolver = sessionIdResolver;

    /// <summary>
    ///     Enrich the log event with the current ASP.NET session id, if sessions are enabled.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent == null)
        {
            throw new ArgumentNullException(nameof(logEvent));
        }

        var sessionId = _sessionIdResolver.SessionId;
        if (sessionId is null)
        {
            return;
        }

        var sessionIdProperty = new LogEventProperty(HttpSessionIdPropertyName, new ScalarValue(sessionId));
        logEvent.AddPropertyIfAbsent(sessionIdProperty);
    }
}
