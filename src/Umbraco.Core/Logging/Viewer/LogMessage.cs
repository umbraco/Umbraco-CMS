using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Events;
using System;
using System.Collections.Generic;

namespace Umbraco.Core.Logging.Viewer
{
    public class LogMessage
    {
        /// <summary>
        /// The time at which the logevent occurred.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The level of the event.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LogEventLevel Level { get; set; }
        
        /// <summary>
        /// The message template describing the logevent.
        /// </summary>
        public string MessageTemplateText { get; set; }

        /// <summary>
        /// The message template filled with the logevent properties.
        /// </summary>
        public string RenderedMessage { get; set; }

        /// <summary>
        /// Properties associated with the logevent, including those presented in Serilog.Events.LogEvent.MessageTemplate.
        /// </summary>
        public IReadOnlyDictionary<string, LogEventPropertyValue> Properties { get; set; }
        
        /// <summary>
        /// An exception associated with the logevent, or null.
        /// </summary>
        public string Exception { get; set; }
    }
}
