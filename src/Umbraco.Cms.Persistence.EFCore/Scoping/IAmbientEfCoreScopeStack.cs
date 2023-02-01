namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal interface IAmbientEfCoreScopeStack : IEFCoreScopeAccessor
{
    public IEfCoreScope? AmbientScope { get; }

    IEfCoreScope Pop();

    void Push(IEfCoreScope scope);
}
