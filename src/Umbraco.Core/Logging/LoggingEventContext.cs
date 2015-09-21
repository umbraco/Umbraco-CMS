using log4net.Core;

namespace Umbraco.Core.Logging
{
    /// <remarks>
    /// Based on https://github.com/cjbhaines/Log4Net.Async
    /// </remarks>
    internal class LoggingEventContext
    {
        public LoggingEventContext(LoggingEvent loggingEvent)
        {
            LoggingEvent = loggingEvent;
        }

        public LoggingEvent LoggingEvent { get; set; }
    }
}