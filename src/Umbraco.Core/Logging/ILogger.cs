using System;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Defines the logging service.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a fatal message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="message">A message.</param>
        void Fatal(Type reporting, Exception exception, string message);

        /// <summary>
        /// Logs a fatal message NOTE: This will log an empty message string
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        void Fatal(Type reporting, Exception exception);

        /// <summary>
        /// Logs a fatal message WITHOUT EX
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Fatal(Type reporting, string message);

        /// <summary>
        /// Logs a fatal message - using a structured log message
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">The message template that includes property values</param>
        /// <param name="propertyValues">Property values to log & update in message template</param>
        void Fatal(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a fatal message WITHOUT EX - using a structured log message
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">The message template that includes property values</param>
        /// <param name="propertyValues">Property values to log & update in message template</param>
        void Fatal(Type reporting, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="message">A message.</param>
        void Error(Type reporting, Exception exception, string message);

        /// <summary>
        /// Logs an error message NOTE: This will log an empty message string
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        void Error(Type reporting, Exception exception);

        /// <summary>
        /// Logs an error message WITHOUT EX
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Error(Type reporting, string message);

        /// <summary>
        /// Logs an error message - using a structured log message
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">The message template that includes property values</param>
        /// <param name="propertyValues">Property values to log & update in message template</param>
        void Error(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs an error message WITHOUT EX - using a structured log message
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">The message template that includes property values</param>
        /// <param name="propertyValues">Property values to log & update in message template</param>
        void Error(Type reporting, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Warn(Type reporting, string message);

        /// <summary>
        /// Logs a warning message - using a structured log message
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">The message template that includes property values</param>
        /// <param name="propertyValues">Property values to log & update in message template</param>
        void Warn(Type reporting, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="message">A message.</param>
        void Warn(Type reporting, Exception exception, string message);

        /// <summary>
        /// Logs a warning message with an exception - using a structured log message
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">The message template that includes property values</param>
        /// <param name="propertyValues">Property values to log & update in message template</param>
        void Warn(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Info(Type reporting, string message);

        /// <summary>
        /// Logs a info message - using a structured log message
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">The message template that includes property values</param>
        /// <param name="propertyValues">Property values to log & update in message template</param>
        void Info(Type reporting, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Debug(Type reporting, string message);
        
        /// <summary>
        /// Logs a debug message - using a structured log message
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">The message template that includes property values</param>
        /// <param name="propertyValues">Property values to log & update in message template</param>
        void Debug(Type reporting, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Verbose(Type reporting, string message);

        /// <summary>
        /// Logs a verbose message - using a structured log message
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">The message template that includes property values</param>
        /// <param name="propertyValues">Property values to log & update in message template</param>
        void Verbose(Type reporting, string messageTemplate, params object[] propertyValues);
        
    }
}
