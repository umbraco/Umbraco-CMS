namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public class EfCoreScopeAccessor : IEFCoreScopeAccessor
{
    private readonly IAmbientEfCoreScopeStack _ambientEfCoreScopeStack;

    internal EfCoreScopeAccessor(IAmbientEfCoreScopeStack ambientEfCoreScopeStack)
    {
        _ambientEfCoreScopeStack = ambientEfCoreScopeStack;
    }

    public IEfCoreScope? AmbientScope => _ambientEfCoreScopeStack.AmbientScope;
}
