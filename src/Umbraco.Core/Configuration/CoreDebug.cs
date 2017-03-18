using System;

namespace Umbraco.Core.Configuration
{
    internal static class CoreDebugExtensions
    {
        private static CoreDebug _coreDebug;

        public static CoreDebug CoreDebug(this UmbracoConfig config)
        {
            return _coreDebug ?? (_coreDebug = new CoreDebug());
        }
    }

    internal class CoreDebug
    {
        public CoreDebug()
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            DumpOnTimeoutThreadAbort = string.Equals("true", appSettings["Umbraco.CoreDebug.DumpOnTimeoutThreadAbort"], StringComparison.OrdinalIgnoreCase);
        }

        // when true, the Logger creates a minidump of w3wp in ~/App_Data/MiniDump whenever it logs
        // an error due to a ThreadAbortException that is due to a timeout.
        public bool DumpOnTimeoutThreadAbort { get; private set; }
    }
}
