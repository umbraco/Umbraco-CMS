using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Umbraco.Cms.Core;

public static class StaticApplicationLogging
{
    private static ILoggerFactory? loggerFactory;

    public static ILogger<object> Logger => CreateLogger<object>();

    public static void Initialize(ILoggerFactory loggerFactory) => StaticApplicationLogging.loggerFactory = loggerFactory;

    public static ILogger<T> CreateLogger<T>() =>
        loggerFactory?.CreateLogger<T>() ?? NullLoggerFactory.Instance.CreateLogger<T>();

    public static ILogger CreateLogger(Type type) => loggerFactory?.CreateLogger(type) ?? NullLogger.Instance;
}
