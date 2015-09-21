using System;
using log4net.Appender;
using log4net.Core;
using log4net.Util;

namespace Umbraco.Core.Logging
{
    /// <remarks>
    /// Based on https://github.com/cjbhaines/Log4Net.Async
    /// </remarks>
    public abstract class AsyncForwardingAppenderBase : ForwardingAppender
    {
        #region Private Members

        private const FixFlags DefaultFixFlags = FixFlags.Partial;
        private FixFlags _fixFlags = DefaultFixFlags;
        private LoggingEventHelper _loggingEventHelper;

        #endregion Private Members

        #region Properties

        public FixFlags Fix
        {
            get { return _fixFlags; }
            set { SetFixFlags(value); }
        }

        /// <summary>
        /// The logger name that will be used for logging internal errors.
        /// </summary>
        protected abstract string InternalLoggerName { get; }

        public abstract int? BufferSize { get; set; }

        #endregion Properties

        public override void ActivateOptions()
        {
            base.ActivateOptions();
            _loggingEventHelper = new LoggingEventHelper(InternalLoggerName, DefaultFixFlags);
            InitializeAppenders();
        }

        #region Appender Management

        public override void AddAppender(IAppender newAppender)
        {
            base.AddAppender(newAppender);
            SetAppenderFixFlags(newAppender);
        }

        private void SetFixFlags(FixFlags newFixFlags)
        {
            if (newFixFlags != _fixFlags)
            {
                _loggingEventHelper.Fix = newFixFlags;
                _fixFlags = newFixFlags;
                InitializeAppenders();
            }
        }

        private void InitializeAppenders()
        {
            foreach (var appender in Appenders)
            {
                SetAppenderFixFlags(appender);
            }
        }

        private void SetAppenderFixFlags(IAppender appender)
        {
            var bufferingAppender = appender as BufferingAppenderSkeleton;
            if (bufferingAppender != null)
            {
                bufferingAppender.Fix = Fix;
            }
        }

        #endregion Appender Management

        #region Forwarding

        protected void ForwardInternalError(string message, Exception exception, Type thisType)
        {
            LogLog.Error(thisType, message, exception);
            var loggingEvent = _loggingEventHelper.CreateLoggingEvent(Level.Error, message, exception);
            ForwardLoggingEvent(loggingEvent, thisType);
        }

        protected void ForwardLoggingEvent(LoggingEvent loggingEvent, Type thisType)
        {
            try
            {
                base.Append(loggingEvent);
            }
            catch (Exception exception)
            {
                LogLog.Error(thisType, "Unable to forward logging event", exception);
            }
        }

        #endregion Forwarding
    }
}