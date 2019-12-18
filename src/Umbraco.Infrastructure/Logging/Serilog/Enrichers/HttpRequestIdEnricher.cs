using System;
using Serilog.Core;
using Serilog.Events;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Logging.Serilog.Enrichers
{
    /// <summary>
    /// Enrich log events with a HttpRequestId GUID.
    /// Original source - https://github.com/serilog-web/classic/blob/master/src/SerilogWeb.Classic/Classic/Enrichers/HttpRequestIdEnricher.cs
    /// Nupkg: 'Serilog.Web.Classic' contains handlers & extra bits we do not want
    /// </summary>
    internal class HttpRequestIdEnricher : ILogEventEnricher
    {
        private readonly Func<IRequestCache> _requestCacheGetter;

        public HttpRequestIdEnricher(Func<IRequestCache> requestCacheGetter)
        {
            _requestCacheGetter = requestCacheGetter;
        }

        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        public const string HttpRequestIdPropertyName = "HttpRequestId";

        /// <summary>
        /// Enrich the log event with an id assigned to the currently-executing HTTP request, if any.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var requestCache = _requestCacheGetter();
            if(requestCache is null) return;

            Guid requestId;
            if (!LogHttpRequest.TryGetCurrentHttpRequestId(out requestId, requestCache))
                return;

            var requestIdProperty = new LogEventProperty(HttpRequestIdPropertyName, new ScalarValue(requestId));
            logEvent.AddPropertyIfAbsent(requestIdProperty);
        }
    }
}
