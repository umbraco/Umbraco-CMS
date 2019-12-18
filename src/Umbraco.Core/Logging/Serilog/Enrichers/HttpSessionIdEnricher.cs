using Serilog.Core;
using Serilog.Events;
using System;
using System.Web;

namespace Umbraco.Core.Logging.Serilog.Enrichers
{
    /// <summary>
    /// Enrich log events with the HttpSessionId property.
    /// Original source - https://github.com/serilog-web/classic/blob/master/src/SerilogWeb.Classic/Classic/Enrichers/HttpSessionIdEnricher.cs
    /// Nupkg: 'Serilog.Web.Classic' contains handlers & extra bits we do not want
    /// </summary>
    internal class HttpSessionIdEnricher : ILogEventEnricher
    {
        /// <summary>
        /// The property name added to enriched log events.
        /// </summary>
        public const string HttpSessionIdPropertyName = "HttpSessionId";

        /// <summary>
        /// Enrich the log event with the current ASP.NET session id, if sessions are enabled.</summary>
        /// <param name="logEvent">The log event to enrich.</param>
        /// <param name="propertyFactory">Factory for creating new properties to add to the event.</param>
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null) throw new ArgumentNullException("logEvent");

            if (HttpContext.Current == null)
                return;

            if (HttpContext.Current.Session == null)
                return;

            var sessionId = HttpContext.Current.Session.SessionID;
            var sessionIdProperty = new LogEventProperty(HttpSessionIdPropertyName, new ScalarValue(sessionId));
            logEvent.AddPropertyIfAbsent(sessionIdProperty);
        }
    }
}
