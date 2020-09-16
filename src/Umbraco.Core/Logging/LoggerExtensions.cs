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
    }
}
