namespace Umbraco.Core.Configuration
{
    public interface ICoreDebug
    {
        /// <summary>
        /// When set to true, Scope logs the stack trace for any scope that gets disposed without being completed.
        /// this helps troubleshooting rogue scopes that we forget to complete
        /// </summary>
        bool LogUncompletedScopes { get; }

        /// <summary>
        /// When set to true, the Logger creates a mini dump of w3wp in ~/App_Data/MiniDump whenever it logs
        /// an error due to a ThreadAbortException that is due to a timeout.
        /// </summary>
        bool DumpOnTimeoutThreadAbort { get; }
    }
}
