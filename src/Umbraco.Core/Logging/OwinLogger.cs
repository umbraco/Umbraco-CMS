using System;
using System.Diagnostics;

namespace Umbraco.Core.Logging
{
    internal class OwinLogger : Microsoft.Owin.Logging.ILogger
    {
        private readonly ILogger _logger;
        private readonly Lazy<Type> _type;

        public OwinLogger(ILogger logger, Lazy<Type> type)
        {
            _logger = logger;
            _type = type;
        }

        /// <summary>
        /// Aggregates most logging patterns to a single method.  This must be compatible with the Func representation in the OWIN environment.
        ///             To check IsEnabled call WriteCore with only TraceEventType and check the return value, no event will be written.
        /// </summary>
        /// <param name="eventType"/><param name="eventId"/><param name="state"/><param name="exception"/><param name="formatter"/>
        /// <returns/>
        public bool WriteCore(TraceEventType eventType, int eventId, object state, Exception exception, Func<object, Exception, string> formatter)
        {
            if (state == null) state = "";
            switch (eventType)
            {
                case TraceEventType.Critical:
                    _logger.Fatal<TraceEventType, int, object>(_type.Value, exception, "[{EventType}] Event Id: {EventId}, State: {State}", eventType, eventId, state);
                    return true;
                case TraceEventType.Error:
                    _logger.Error<TraceEventType,int,object>(_type.Value, exception, "[{EventType}] Event Id: {EventId}, State: {State}", eventType, eventId, state);
                    return true;
                case TraceEventType.Warning:
                    _logger.Warn(_type.Value, "[{EventType}] Event Id: {EventId}, State: {State}", eventType, eventId, state);
                    return true;
                case TraceEventType.Information:
                    _logger.Info<TraceEventType, int, object>(_type.Value, "[{EventType}] Event Id: {EventId}, State: {State}", eventType, eventId, state);
                    return true;
                case TraceEventType.Verbose:
                    _logger.Debug<TraceEventType, int, object>(_type.Value, "[{EventType}] Event Id: {EventId}, State: {State}", eventType, eventId, state);
                    return true;
                case TraceEventType.Start:
                    _logger.Debug<TraceEventType, int, object>(_type.Value, "[{EventType}] Event Id: {EventId}, State: {State}", eventType, eventId, state);
                    return true;
                case TraceEventType.Stop:
                    _logger.Debug<TraceEventType, int, object>(_type.Value, "[{EventType}] Event Id: {EventId}, State: {State}", eventType, eventId, state);
                    return true;
                case TraceEventType.Suspend:
                    _logger.Debug<TraceEventType, int, object>(_type.Value, "[{EventType}] Event Id: {EventId}, State: {State}", eventType, eventId, state);
                    return true;
                case TraceEventType.Resume:
                    _logger.Debug<TraceEventType, int, object>(_type.Value, "[{EventType}] Event Id: {EventId}, State: {State}", eventType, eventId, state);
                    return true;
                case TraceEventType.Transfer:
                    _logger.Debug<TraceEventType, int, object>(_type.Value, "[{EventType}] Event Id: {EventId}, State: {State}", eventType, eventId, state);
                    return true;
                default:
                    throw new ArgumentOutOfRangeException("eventType");
            }
        }
    }
}
