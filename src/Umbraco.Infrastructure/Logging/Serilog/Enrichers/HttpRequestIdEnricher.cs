using Serilog.Core;
using Serilog.Events;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Logging.Serilog.Enrichers;

/// <summary>
///     Enrich log events with a HttpRequestId GUID.
///     Original source -
///     https://github.com/serilog-web/classic/blob/master/src/SerilogWeb.Classic/Classic/Enrichers/HttpRequestIdEnricher.cs
///     Nupkg: 'Serilog.Web.Classic' contains handlers and extra bits we do not want
/// </summary>
public class HttpRequestIdEnricher : ILogEventEnricher
{
    /// <summary>
    ///     The property name added to enriched log events.
    /// </summary>
    public const string HttpRequestIdPropertyName = "HttpRequestId";

    private readonly IRequestCache _requestCache;

    public HttpRequestIdEnricher(IRequestCache requestCache) =>
        _requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));

    /// <summary>
    ///     Enrich the log event with an id assigned to the currently-executing HTTP request, if any.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent == null)
        {
            throw new ArgumentNullException(nameof(logEvent));
        }

        if (!LogHttpRequest.TryGetCurrentHttpRequestId(out Guid? requestId, _requestCache))
        {
            return;
        }

        var requestIdProperty = new LogEventProperty(HttpRequestIdPropertyName, new ScalarValue(requestId));
        logEvent.AddPropertyIfAbsent(requestIdProperty);
    }
}
