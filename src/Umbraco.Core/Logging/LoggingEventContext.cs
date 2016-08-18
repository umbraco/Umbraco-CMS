using log4net.Core;

namespace Umbraco.Core.Logging
{
    /// <remarks>
    /// Borrowed from https://github.com/cjbhaines/Log4Net.Async - will reference Nuget packages directly in v8
    /// </remarks>
    internal sealed class LoggingEventContext
    {
        public LoggingEventContext(LoggingEvent loggingEvent, object httpContext)
        {
            LoggingEvent = loggingEvent;
            HttpContext = httpContext;
        }

        public LoggingEvent LoggingEvent { get; set; }

        public object HttpContext { get; set; }
    }
}