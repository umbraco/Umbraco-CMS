using System;

namespace Umbraco.Core.Logging
{

    public interface ILogger : ILogger<object> { }

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
    public interface ILogger<T>
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
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void LogCritical(Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a fatal message.
        /// </summary>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void LogCritical(string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void LogError(Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void LogError(string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void LogWarning(string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void LogWarning(Exception exception, string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a info message.
        /// </summary>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void LogInformation(string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a debug message.
        /// </summary>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void LogDebug(string messageTemplate, params object[] propertyValues);

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        void LogTrace(string messageTemplate, params object[] propertyValues);
    }
}
