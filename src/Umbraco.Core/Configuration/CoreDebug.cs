using System;

namespace Umbraco.Core.Configuration
{
    internal class CoreDebug : ICoreDebug
    {
        public CoreDebug()
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            LogUncompletedScopes = string.Equals("true", appSettings[Constants.AppSettings.Debug.LogUncompletedScopes], StringComparison.OrdinalIgnoreCase);
            DumpOnTimeoutThreadAbort = string.Equals("true", appSettings[Constants.AppSettings.Debug.DumpOnTimeoutThreadAbort], StringComparison.OrdinalIgnoreCase);
        }

        // when true, Scope logs the stack trace for any scope that gets disposed without being completed.
        // this helps troubleshooting rogue scopes that we forget to complete
        public bool LogUncompletedScopes { get; }

        // when true, the Logger creates a mini dump of w3wp in ~/App_Data/MiniDump whenever it logs
        // an error due to a ThreadAbortException that is due to a timeout.
        public bool DumpOnTimeoutThreadAbort { get; }
    }
}
