using System;
using System.Threading;
using Serilog.Core;
using Serilog.Events;
using Umbraco.Core.Cache;

namespace Umbraco.Core.Logging.Serilog.Enrichers
{
    /// <summary>
    /// Enrich log events with a HttpRequestNumber unique within the current
    /// logging session.
    /// Original source - https://github.com/serilog-web/classic/blob/master/src/SerilogWeb.Classic/Classic/Enrichers/HttpRequestNumberEnricher.cs
    /// Nupkg: 'Serilog.Web.Classic' contains handlers & extra bits we do not want
    /// </summary>
    internal class HttpRequestNumberEnricher : ILogEventEnricher
    {
        private static int _lastRequestNumber;
        private static readonly string _requestNumberItemName = typeof(HttpRequestNumberEnricher).Name + "+RequestNumber";

        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        private const string _httpRequestNumberPropertyName = "HttpRequestNumber";

        private readonly Lazy<IAppCache> _requestCache;

        public HttpRequestNumberEnricher(Lazy<IAppCache> requestCache)
        {
            _requestCache = requestCache;
        }

        /// <summary>
        /// Enrich the log event with the number assigned to the currently-executing HTTP request, if any.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");

            var requestNumber = _requestCache.Value.Get(_requestNumberItemName,
                    () => Interlocked.Increment(ref _lastRequestNumber));

            var requestNumberProperty = new LogEventProperty(_httpRequestNumberPropertyName, new ScalarValue(requestNumber));
            logEvent.AddPropertyIfAbsent(requestNumberProperty);
        }
    }
}
