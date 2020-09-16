using System;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Defines the logging service.
    /// </summary>
    /// <remarks>
    /// <para>Message templates in logging methods follow the Message Templates specification
    /// available at https://messagetemplates.org/ in order to support structured logging.</para>
    /// <para>Implementations must ensure that they support these templates. Note that the
    /// specification includes support for traditional C# numeric placeholders.</para>
    /// <para>For instance, "Processed {Input} in {Time}ms."</para>
    /// </remarks>
    public interface ILogger
    {
        /// <summary>
        /// Determines if logging is enabled at a specified level, for a reporting type.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="level">The level.</param>
        bool IsEnabled(Type reporting, LogLevel level);

        /// <summary>
        /// Logs a fatal message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="message">A message.</param>
        void Fatal(Type reporting, Exception exception, string message);

        /// <summary>
        /// Logs a fatal exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <remarks>The message string is unspecified and is implementation-specific.</remarks>
        void Fatal(Type reporting, Exception exception);

        /// <summary>
        /// Logs a fatal message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Fatal(Type reporting, string message);

        /// <summary>
        /// Logs a fatal message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void Fatal(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues);
        /// <summary>
        /// Logs a fatal message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Fatal<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a fatal message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Fatal<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs a fatal message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Fatal<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0);

        /// <summary>
        /// Logs a fatal message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void Fatal(Type reporting, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="message">A message.</param>
        void Error(Type reporting, Exception exception, string message);

        /// <summary>
        /// Logs an error exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <remarks>The message string is unspecified and is implementation-specific.</remarks>
        void Error(Type reporting, Exception exception);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Error(Type reporting, string message);

        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void Error(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues);
        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Error<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Error<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Error<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void Error(Type reporting, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Warn(Type reporting, string message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void Warn(Type reporting, string messageTemplate, params object[] propertyValues);

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
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void Warn(Type reporting, Exception exception, string messageTemplate, params object[] propertyValues);
        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Warn<T0, T1, T2>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Warn<T0, T1>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Warn<T0>(Type reporting, Exception exception, string messageTemplate, T0 propertyValue0);
        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Warn<T0>(Type reporting, string message, T0 propertyValue0);

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Info(Type reporting, string message);

        /// <summary>
        /// Logs a info message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void Info(Type reporting, string messageTemplate, params object[] propertyValues);
        /// <summary>
        /// Logs a info message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Info<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a info message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Info<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs a info message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Info<T0>(Type reporting, string messageTemplate, T0 propertyValue0);

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Debug(Type reporting, string message);

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void Debug(Type reporting, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Debug<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Debug<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Debug<T0>(Type reporting, string messageTemplate, T0 propertyValue0);

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="message">A message.</param>
        void Verbose(Type reporting, string message);

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void Verbose(Type reporting, string messageTemplate, params object[] propertyValues);
        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        /// <param name="propertyValue2">Property value 2</param>
        void Verbose<T0, T1, T2>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1, T2 propertyValue2);
        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        /// <param name="propertyValue1">Property value 1</param>
        void Verbose<T0, T1>(Type reporting, string messageTemplate, T0 propertyValue0, T1 propertyValue1);
        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="reporting">The reporting type.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValue0">Property value 0</param>
        void Verbose<T0>(Type reporting, string messageTemplate, T0 propertyValue0);
    }
}
