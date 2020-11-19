using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Umbraco.Core
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
            try
            {
                return _loggerFactory?.CreateLogger<T>() ?? NullLoggerFactory.Instance.CreateLogger<T>();
            }
            catch (ObjectDisposedException)
            {
                // TODO: Weird, investigate, ScopedRepositoryTests.FullDataSetRepositoryCachePolicy etc
                Debugger.Break();
                return NullLoggerFactory.Instance.CreateLogger<T>();
            }
        }
    }
}
