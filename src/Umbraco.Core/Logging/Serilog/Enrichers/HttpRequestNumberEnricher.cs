using System;
using System.Threading;
using Serilog.Core;
using Serilog.Events;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;

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
        private readonly Func<IFactory> _factoryFunc;
        private static int _lastRequestNumber;
        private static readonly string _requestNumberItemName = typeof(HttpRequestNumberEnricher).Name + "+RequestNumber";

        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        private const string _httpRequestNumberPropertyName = "HttpRequestNumber";


        public HttpRequestNumberEnricher(Func<IFactory> factoryFunc)
        {
            _factoryFunc = factoryFunc;
        }

        /// <summary>
        /// Enrich the log event with the number assigned to the currently-executing HTTP request, if any.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException(nameof(logEvent));

            var factory = _factoryFunc();
            if (factory is null) return;

            var requestCache = factory.GetInstance<IRequestCache>();
            var requestNumber = requestCache.Get(_requestNumberItemName,
                    () => Interlocked.Increment(ref _lastRequestNumber));

            var requestNumberProperty = new LogEventProperty(_httpRequestNumberPropertyName, new ScalarValue(requestNumber));
            logEvent.AddPropertyIfAbsent(requestNumberProperty);
        }
    }
}
