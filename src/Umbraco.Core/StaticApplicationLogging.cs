using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Umbraco.Cms.Core;

/// <summary>
///     Provides static access to application logging functionality.
/// </summary>
/// <remarks>
///     This class provides a way to access logging before dependency injection is fully configured.
///     It must be initialized with an <see cref="ILoggerFactory" /> before use.
/// </remarks>
public static class StaticApplicationLogging
{
    private static ILoggerFactory? loggerFactory;

    /// <summary>
    ///     Gets a default logger instance for general logging purposes.
    /// </summary>
    public static ILogger<object> Logger => CreateLogger<object>();

    /// <summary>
    ///     Initializes the static logging with the specified logger factory.
    /// </summary>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory" /> to use for creating loggers.</param>
    public static void Initialize(ILoggerFactory loggerFactory) => StaticApplicationLogging.loggerFactory = loggerFactory;

    /// <summary>
    ///     Creates a logger for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create a logger for.</typeparam>
    /// <returns>An <see cref="ILogger{T}" /> instance, or a null logger if not initialized.</returns>
    public static ILogger<T> CreateLogger<T>() =>
        loggerFactory?.CreateLogger<T>() ?? NullLoggerFactory.Instance.CreateLogger<T>();

    /// <summary>
    ///     Creates a logger for the specified type.
    /// </summary>
    /// <param name="type">The type to create a logger for.</param>
    /// <returns>An <see cref="ILogger" /> instance, or a null logger if not initialized.</returns>
    public static ILogger CreateLogger(Type type) => loggerFactory?.CreateLogger(type) ?? NullLogger.Instance;
}
