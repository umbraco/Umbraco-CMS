using System;

namespace Umbraco.Core.Logging
{
    /// <summary>
    /// Allows for strongly typed log sources
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Adds an error log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public static void Error<T>(this ILogger logger, string message, Exception exception)
        {
            logger.Error(typeof(T), message, exception);
        }

        /// <summary>
        /// Adds a warn log
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logger"></param>
        /// <param name="message"></param>
        /// <param name="formatItems"></param>
        public static void Warn<T>(this ILogger logger, string message, params Func<object>[] formatItems)
        {
            logger.Warn(typeof(T), message, formatItems);
        }
        
        public static void WarnWithException<T>(this ILogger logger, string message, Exception e, params Func<object>[] formatItems)
        {
            logger.WarnWithException(typeof(T), message, e, formatItems);
        }


        /// <summary>
        /// Traces a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logger"></param>
        /// <param name="generateMessageFormat">The generate message format.</param>
        /// <param name="formatItems">The format items.</param>
        /// <remarks></remarks>
        public static void Info<T>(this ILogger logger, string generateMessageFormat, params Func<object>[] formatItems)
        {
            logger.Info(typeof(T), generateMessageFormat, formatItems);
        }

        /// <summary>
        /// Traces a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logger"></param>
        /// <param name="generateMessage">The delegate to generate a message.</param>
        /// <remarks></remarks>
        public static void Info<T>(this ILogger logger, Func<string> generateMessage)
        {
            logger.Info(typeof(T), generateMessage);
        }

        /// <summary>
        /// Debugs a message, only generating the message if tracing is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logger"></param>
        /// <param name="generateMessage">The delegate to generate a message.</param>
        /// <remarks></remarks>
        public static void Debug<T>(this ILogger logger, Func<string> generateMessage)
        {
            logger.Debug(typeof(T), generateMessage);
        }

        /// <summary>
        /// Debugs a message, only generating the message if debug is actually enabled. Use this method to avoid calling any long-running methods such as "ToDebugString" if logging is disabled.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="logger"></param>
        /// <param name="generateMessageFormat">The generate message format.</param>
        /// <param name="formatItems">The format items.</param>
        /// <remarks></remarks>
        public static void Debug<T>(this ILogger logger, string generateMessageFormat, params Func<object>[] formatItems)
        {
            logger.Debug(typeof(T), generateMessageFormat, formatItems);
        }
    }
}