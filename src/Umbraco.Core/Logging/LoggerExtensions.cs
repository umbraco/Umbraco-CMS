using System;
using System.Linq;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Provides extension methods for the <see cref="ILogger"/> interface.
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        /// <param name="exception">An exception.</param>
        public static void Error<T>(this ILogger logger, string message, Exception exception = null)
        {
            logger.Error(typeof(T), message, exception);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        public static void Warn<T>(this ILogger logger, string message)
        {
            logger.Warn(typeof(T), message);
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="messageBuilder">A message builder.</param>
        public static void Warn<T>(this ILogger logger, Func<string> messageBuilder)
        {
            logger.Warn(typeof(T), messageBuilder);
        }

        /// <summary>
        /// Logs a formatted warning message with an exception.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="messageBuilder">A message builder.</param>
        public static void Warn<T>(this ILogger logger, Exception exception, Func<string> messageBuilder)
        {
            logger.Warn(typeof(T), exception, messageBuilder);
        }

        /// <summary>
        /// Logs a formatted warning message with an exception.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="exception">An exception.</param>
        /// <param name="message">A message.</param>
        public static void Warn<T>(this ILogger logger, Exception exception, string message)
        {
            logger.Warn(typeof(T), exception, message);
        }

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        public static void Info<T>(this ILogger logger, string message)
        {
            logger.Info(typeof(T), message);
        }

        /// <summary>
        /// Logs an information message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="messageBuilder">A message builder.</param>
        public static void Info<T>(this ILogger logger, Func<string> messageBuilder)
        {
            logger.Info(typeof(T), messageBuilder);
        }

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="message">A message.</param>
        public static void Debug<T>(this ILogger logger, string message)
        {
            logger.Debug(typeof(T), message);
        }

        /// <summary>
        /// Logs a debugging message.
        /// </summary>
        /// <typeparam name="T">The reporting type.</typeparam>
        /// <param name="logger">The logger.</param>
        /// <param name="messageBuilder">A message builder.</param>
        public static void Debug<T>(this ILogger logger, Func<string> messageBuilder)
        {
            logger.Debug(typeof(T), messageBuilder);
        }
    }
}
