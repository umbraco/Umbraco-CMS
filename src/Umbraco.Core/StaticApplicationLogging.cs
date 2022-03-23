using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Umbraco.Cms.Core
{
    public static class StaticApplicationLogging
    {
        private static ILoggerFactory _loggerFactory;

        public static void Initialize(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public static ILogger<object> Logger => CreateLogger<object>();

        public static ILogger<T> CreateLogger<T>()
        {
            return _loggerFactory?.CreateLogger<T>() ?? NullLoggerFactory.Instance.CreateLogger<T>();
        }
    }
}
