using System;
using System.Threading;
using System.Web;
using Serilog.Core;
using Serilog.Events;

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
        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        public const string HttpRequestNumberPropertyName = "HttpRequestNumber";

        static int _lastRequestNumber;
        static readonly string RequestNumberItemName = typeof(HttpRequestNumberEnricher).Name + "+RequestNumber";

        /// <summary>
        /// Enrich the log event with the number assigned to the currently-executing HTTP request, if any.
        /// </summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");

            if (HttpContext.Current == null)
                return;

            int requestNumber;
            var requestNumberItem = HttpContext.Current.Items[RequestNumberItemName];
            if (requestNumberItem == null)
                HttpContext.Current.Items[RequestNumberItemName] = requestNumber = Interlocked.Increment(ref _lastRequestNumber);
            else
                requestNumber = (int)requestNumberItem;

            var requestNumberProperty = new LogEventProperty(HttpRequestNumberPropertyName, new ScalarValue(requestNumber));
            logEvent.AddPropertyIfAbsent(requestNumberProperty);
        }
    }
}
