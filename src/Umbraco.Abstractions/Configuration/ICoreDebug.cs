namespace Umbraco.Core.Configuration
{
    public interface ICoreDebug
    {
        bool LogUncompletedScopes { get; }
        bool DumpOnTimeoutThreadAbort { get; }
    }
}