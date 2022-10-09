using Serilog.Core;
using Serilog.Events;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.Logging.Serilog.Enrichers;

/// <summary>
///     Enrich log events with a HttpRequestNumber unique within the current
///     logging session.
///     Original source -
///     https://github.com/serilog-web/classic/blob/master/src/SerilogWeb.Classic/Classic/Enrichers/HttpRequestNumberEnricher.cs
///     Nupkg: 'Serilog.Web.Classic' contains handlers and extra bits we do not want
/// </summary>
public class HttpRequestNumberEnricher : ILogEventEnricher
{
    /// <summary>
    ///     The property name added to enriched log events.
    /// </summary>
    private const string HttpRequestNumberPropertyName = "HttpRequestNumber";
    private static readonly string _requestNumberItemName = typeof(HttpRequestNumberEnricher).Name + "+RequestNumber";

    private static int _lastRequestNumber;
    private readonly IRequestCache _requestCache;

    public HttpRequestNumberEnricher(IRequestCache requestCache) =>
        _requestCache = requestCache ?? throw new ArgumentNullException(nameof(requestCache));

    /// <summary>
    ///     Enrich the log event with the number assigned to the currently-executing HTTP request, if any.
    /// </summary>
    /// <param name="logEvent">The log event to enrich.</param>
    /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent == null)
        {
            throw new ArgumentNullException(nameof(logEvent));
        }

        var requestNumber = _requestCache.Get(
            _requestNumberItemName,
            () => Interlocked.Increment(ref _lastRequestNumber));

        var requestNumberProperty =
            new LogEventProperty(HttpRequestNumberPropertyName, new ScalarValue(requestNumber));
        logEvent.AddPropertyIfAbsent(requestNumberProperty);
    }
}
