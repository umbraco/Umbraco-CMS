namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public class EfCoreScopeProvider : IEfCoreScopeProvider
{
    private readonly IAmbientEfCoreScopeStack _ambientEfCoreScopeStack;
    private readonly IUmbracoEfCoreDatabaseFactory _umbracoEfCoreDatabaseFactory;

    internal EfCoreScopeProvider(IAmbientEfCoreScopeStack ambientEfCoreScopeStack, IUmbracoEfCoreDatabaseFactory umbracoEfCoreDatabaseFactory)
    {
        _ambientEfCoreScopeStack = ambientEfCoreScopeStack;
        _umbracoEfCoreDatabaseFactory = umbracoEfCoreDatabaseFactory;
    }

    public IEfCoreScope CreateScope()
    {
        var efCoreScope = new EfCoreScope(_umbracoEfCoreDatabaseFactory);
        _ambientEfCoreScopeStack.Push(efCoreScope);
        return efCoreScope;
    }
}
