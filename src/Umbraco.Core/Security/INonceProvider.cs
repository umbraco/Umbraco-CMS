namespace Umbraco.Core.Security
{
    public interface INonceProvider
    {
        string ScriptNonce { get; }
        string StyleNonce { get; }
    }
}