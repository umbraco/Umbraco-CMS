using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ILogger = Umbraco.Core.Logging.ILogger;

namespace Umbraco.Composing
{
    public static class Current
    {
        private static Microsoft.Extensions.Logging.ILogger<object> _logger = new NullLogger<object>();
        private static ILoggerFactory _loggerFactory;
        private static Configs _configs;
        private static IIOHelper _ioHelper;
        private static IHostingEnvironment _hostingEnvironment;
        private static IBackOfficeInfo _backOfficeInfo;
        private static IProfiler _profiler;

        public static Microsoft.Extensions.Logging.ILogger<object> Logger => EnsureInitialized(_logger);
        public static Configs Configs => EnsureInitialized(_configs);
        public static IIOHelper IOHelper => EnsureInitialized(_ioHelper);
        public static IHostingEnvironment HostingEnvironment => EnsureInitialized(_hostingEnvironment);
        public static IBackOfficeInfo BackOfficeInfo => EnsureInitialized(_backOfficeInfo);
        public static IProfiler Profiler => EnsureInitialized(_profiler);
        public static ILoggerFactory LoggerFactory => EnsureInitialized(_loggerFactory);

        public static bool IsInitialized { get; internal set; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T EnsureInitialized<T>(T returnValue)
            where T : class
        {
            if (returnValue is null && !IsInitialized)
                throw new InvalidOperationException("Current cannot be used before initialize");
            return returnValue;
        }

        public static void Initialize(
            Microsoft.Extensions.Logging.ILogger<object> logger,
            Configs configs,
            IIOHelper ioHelper,
            IHostingEnvironment hostingEnvironment,
            IBackOfficeInfo backOfficeInfo,
            IProfiler profiler)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("Current cannot be initialized more than once");
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configs = configs ?? throw new ArgumentNullException(nameof(configs));
            _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _backOfficeInfo = backOfficeInfo ?? throw new ArgumentNullException(nameof(backOfficeInfo));
            _profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));

            IsInitialized = true;
        }
    }
}
