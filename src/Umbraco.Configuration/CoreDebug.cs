using System;
using System.Configuration;

namespace Umbraco.Core.Configuration
{
    public class CoreDebug : ICoreDebug
    {
        public CoreDebug()
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
