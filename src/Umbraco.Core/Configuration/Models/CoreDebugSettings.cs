namespace Umbraco.Core.Configuration.Models
{
    public class CoreDebugSettings
    {
        public bool LogUncompletedScopes { get; set; } = false;

        public bool DumpOnTimeoutThreadAbort { get; set; } = false;
    }
}
