using System;
using System.Configuration;

namespace Umbraco.Core.Configuration
{
    public class CoreDebugSettings : ICoreDebugSettings
    {
        public CoreDebugSettings()
        {
            var appSettings = ConfigurationManager.AppSettings;
            LogUncompletedScopes = string.Equals("true", appSettings[Constants.AppSettings.Debug.LogUncompletedScopes], StringComparison.OrdinalIgnoreCase);
            DumpOnTimeoutThreadAbort = string.Equals("true", appSettings[Constants.AppSettings.Debug.DumpOnTimeoutThreadAbort], StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc />
        public bool LogUncompletedScopes { get; }

        /// <inheritdoc />
        public bool DumpOnTimeoutThreadAbort { get; }
    }
}
