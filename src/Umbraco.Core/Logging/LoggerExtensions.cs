using System;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Provides extension methods for the <see cref="ILogger"/> interface.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Determines if logging is enabled at a specified level, for a reporting type.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="level">The level.</param>
        public static bool IsEnabled<T>(this ILogger logger, LogLevel level)
            => logger.IsEnabled(typeof(T), level);

        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        /// <param name="exception">An exception.</param>
        public static void Error<T>(this ILogger logger, Exception exception, string message)
            => logger.Error(typeof(T), exception, message);

        /// <summary>
        /// Logs an error message with an exception.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void Error<T>(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
            => logger.Error(typeof(T), exception, messageTemplate, propertyValues);

        /// <summary>
        /// Logs an error exception.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">An exception.</param>
        public static void Error<T>(this ILogger logger, Exception exception)
            => logger.Error(typeof(T), exception);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        public static void Error<T>(this ILogger logger, string message)
            => logger.Error(typeof(T), message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void Error<T>(this ILogger logger, string messageTemplate, params object[] propertyValues)
            => logger.Error(typeof(T), messageTemplate, propertyValues);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        public static void Warn<T>(this ILogger logger, string message)
            => logger.Warn(typeof(T), message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void Warn<T>(this ILogger logger, string messageTemplate, params object[] propertyValues)
            => logger.Warn(typeof(T), messageTemplate, propertyValues);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="message">A message.</param>
        public static void Warn<T>(this ILogger logger, Exception exception, string message)
            => logger.Warn(typeof(T), exception, message);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void Warn<T>(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
            => logger.Warn(typeof(T), exception, messageTemplate, propertyValues);

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        public static void Info<T>(this ILogger logger, string message)
            => logger.Info(typeof(T), message);

        /// <summary>
        /// Logs a information message.
        /// </summary>
        /// <typeparam name="T">The reporting type</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void Info<T>(this ILogger logger, string messageTemplate, params object[] propertyValues)
            => logger.Info(typeof(T), messageTemplate, propertyValues);

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        public static void Debug<T>(this ILogger logger, string message)
            => logger.Debug(typeof(T), message);

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <typeparam name="T">The reporting type</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void Debug<T>(this ILogger logger, string messageTemplate, params object[] propertyValues)
            => logger.Debug(typeof(T), messageTemplate, propertyValues);

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        public static void Verbose<T>(this ILogger logger, string message)
            => logger.Verbose(typeof(T), message);

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void Verbose<T>(this ILogger logger, string messageTemplate, params object[] propertyValues)
            => logger.Verbose(typeof(T), messageTemplate, propertyValues);

        /// <summary>
        /// Logs a fatal message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="message">A message.</param>
        public static void Fatal<T>(this ILogger logger, Exception exception, string message)
            => logger.Fatal(typeof(T), exception, message);

        /// <summary>
        /// Logs a fatal message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void Fatal<T>(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
            => logger.Fatal(typeof(T), exception, messageTemplate, propertyValues);
    }
}
