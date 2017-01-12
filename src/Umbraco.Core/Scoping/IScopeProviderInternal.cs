namespace Umbraco.Core.Scoping
{
    internal interface IScopeProviderInternal : IScopeProvider
    {
        IScope AmbientScope { get; set; }
        IScope CreateNoScope();
    }
}
