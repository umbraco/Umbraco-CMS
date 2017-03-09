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
            LogUncompletedScopes = string.Equals("true", appSettings["Umbraco.CoreDebug.LogUncompletedScopes"], StringComparison.OrdinalIgnoreCase);
        }

        // when true, Scope logs the stack trace for any scope that gets disposed without being completed.
        // this helps troubleshooting rogue scopes that we forget to complete
        public bool LogUncompletedScopes { get; private set; }
    }
}
