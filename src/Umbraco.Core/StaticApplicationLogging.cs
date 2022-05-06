using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Umbraco.Cms.Core;

public static class StaticApplicationLogging
{
    private static ILoggerFactory? s_loggerFactory;

    public static ILogger<object> Logger => CreateLogger<object>();

    public static void Initialize(ILoggerFactory loggerFactory) => s_loggerFactory = loggerFactory;

    public static ILogger<T> CreateLogger<T>() =>
        s_loggerFactory?.CreateLogger<T>() ?? NullLoggerFactory.Instance.CreateLogger<T>();

    public static ILogger CreateLogger(Type type) => s_loggerFactory?.CreateLogger(type) ?? NullLogger.Instance;
}
