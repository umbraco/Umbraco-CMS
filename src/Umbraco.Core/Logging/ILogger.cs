using System;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Defines the logging service.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        /// <param name="exception">An exception.</param>
        void Error(Type reporting, string message, Exception exception = null);

        // note: should we have more overloads for Error too?

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="format">A message.</param>
        void Warn(Type reporting, string format);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageBuilder">A message builder.</param>
        void Warn(Type reporting, Func<string> messageBuilder);

        /// <summary>
        /// Logs a formatted warning message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        void Warn(Type reporting, string format, params object[] args);

        /// <summary>
        /// Logs a formatted warning message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of functions returning objects to format.</param>
        void Warn(Type reporting, string format, params Func<object>[] args);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="message">A message.</param>
        void Warn(Type reporting, Exception exception, string message);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageBuilder">A message builder.</param>
        void Warn(Type reporting, Exception exception, Func<string> messageBuilder);

        /// <summary>
        /// Logs a formatted warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        void Warn(Type reporting, Exception exception, string format, params object[] args);

        /// <summary>
        /// Logs a formatted warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of functions returning objects to format.</param>
        void Warn(Type reporting, Exception exception, string format, params Func<object>[] args);

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Info(Type reporting, string message);

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageBuilder">A message builder.</param>
        void Info(Type reporting, Func<string> messageBuilder);

        /// <summary>
        /// Logs a formatted information message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        void Info(Type reporting, string format, params object[] args);

        /// <summary>
        /// Logs a formatted information message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of functions returning objects to format.</param>
        void Info(Type reporting, string format, params Func<object>[] args);

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Debug(Type reporting, string message);

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageBuilder">A message builder.</param>
        void Debug(Type reporting, Func<string> messageBuilder);

        /// <summary>
        /// Logs a formatted debugging message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of objects to format.</param>
        void Debug(Type reporting, string format, params object[] args);

        /// <summary>
        /// Logs a formatted debugging message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An array of functions returning objects to format.</param>
        void Debug(Type reporting, string format, params Func<object>[] args);
    }
}
