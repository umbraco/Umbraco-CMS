using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Umbraco.Composing
{
    public static class Current
    {
        private static ILogger<object> _logger = new NullLogger<object>();
        private static IIOHelper _ioHelper;
        private static IHostingEnvironment _hostingEnvironment;
        private static IBackOfficeInfo _backOfficeInfo;
        private static IProfiler _profiler;
        private static SecuritySettings _securitySettings;
        private static GlobalSettings _globalSettings;

        public static ILogger<object> Logger => EnsureInitialized(_logger);
        public static IIOHelper IOHelper => EnsureInitialized(_ioHelper);
        public static IHostingEnvironment HostingEnvironment => EnsureInitialized(_hostingEnvironment);
        public static IBackOfficeInfo BackOfficeInfo => EnsureInitialized(_backOfficeInfo);
        public static IProfiler Profiler => EnsureInitialized(_profiler);
        public static SecuritySettings SecuritySettings => EnsureInitialized(_securitySettings);
        public static GlobalSettings GlobalSettings => EnsureInitialized(_globalSettings);

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
            ILogger<object> logger,
            SecuritySettings securitySettings,
            GlobalSettings globalSettings,
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
            _ioHelper = ioHelper ?? throw new ArgumentNullException(nameof(ioHelper));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
            _backOfficeInfo = backOfficeInfo ?? throw new ArgumentNullException(nameof(backOfficeInfo));
            _profiler = profiler ?? throw new ArgumentNullException(nameof(profiler));
            _securitySettings = securitySettings ?? throw new ArgumentNullException(nameof(securitySettings));
            _globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));

            IsInitialized = true;
        }

    }
}
