using System;
using System.Threading;
using log4net.Appender;
using log4net.Core;

namespace Umbraco.Tests.Logging
{
    internal class DebugAppender : MemoryAppender
    {
        public TimeSpan AppendDelay { get; set; }
        public int LoggedEventCount { get { return m_eventsList.Count; } }

        public bool Cancel { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (Cancel) return;

            if (AppendDelay > TimeSpan.Zero)
            {
                Thread.Sleep(AppendDelay);
            }
            base.Append(loggingEvent);
        }
    }
}