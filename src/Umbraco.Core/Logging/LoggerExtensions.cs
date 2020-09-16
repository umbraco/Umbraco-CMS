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
        /// Logs a warning message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        public static void LogWarning<T>(this ILogger logger, string message)
            => logger.LogWarning(message);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void LogWarning<T>(this ILogger logger, string messageTemplate, params object[] propertyValues)
            => logger.LogWarning(messageTemplate, propertyValues);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="message">A message.</param>
        public static void LogWarning<T>(this ILogger logger, Exception exception, string message)
            => logger.LogWarning(exception, message);

        /// <summary>
        /// Logs a warning message with an exception.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void LogWarning<T>(this ILogger logger, Exception exception, string messageTemplate, params object[] propertyValues)
            => logger.LogWarning(exception, messageTemplate, propertyValues);

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        public static void Debug<T>(this ILogger logger, string message)
            => logger.LogDebug(message);

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <typeparam name="T">The reporting type</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void Debug<T>(this ILogger logger, string messageTemplate, params object[] propertyValues)
            => logger.LogDebug(messageTemplate, propertyValues);

        /// <summary>
        /// Logs a verbose message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="messageTemplate">A message template.</param>
        /// <param name="propertyValues">Property values.</param>
        public static void Verbose<T>(this ILogger logger, string messageTemplate, params object[] propertyValues)
            => logger.LogTrace(messageTemplate, propertyValues);
    }
}
