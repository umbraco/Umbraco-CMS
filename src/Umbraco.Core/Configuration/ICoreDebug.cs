namespace Umbraco.Core.Configuration
{
    public interface ICoreDebug
    {
        bool DumpOnTimeoutThreadAbort { get; }
        bool LogUncompletedScopes { get; }
    }
}
