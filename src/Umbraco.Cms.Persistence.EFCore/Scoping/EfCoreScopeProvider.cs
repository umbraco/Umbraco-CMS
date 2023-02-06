using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public class EfCoreScopeProvider : IEfCoreScopeProvider
{
    private readonly IAmbientEfCoreScopeStack _ambientEfCoreScopeStack;
    private readonly IUmbracoEfCoreDatabaseFactory _umbracoEfCoreDatabaseFactory;
    private readonly IEFCoreScopeAccessor _efCoreScopeAccessor;

    // Needed for DI as IAmbientEfCoreScopeStack is internal
    public EfCoreScopeProvider()
        : this(
            StaticServiceProvider.Instance.GetRequiredService<IAmbientEfCoreScopeStack>(),
            StaticServiceProvider.Instance.GetRequiredService<IUmbracoEfCoreDatabaseFactory>(),
            StaticServiceProvider.Instance.GetRequiredService<IEFCoreScopeAccessor>())
    {
    }

    internal EfCoreScopeProvider(IAmbientEfCoreScopeStack ambientEfCoreScopeStack, IUmbracoEfCoreDatabaseFactory umbracoEfCoreDatabaseFactory, IEFCoreScopeAccessor efCoreScopeAccessor)
    {
        _ambientEfCoreScopeStack = ambientEfCoreScopeStack;
        _umbracoEfCoreDatabaseFactory = umbracoEfCoreDatabaseFactory;
        _efCoreScopeAccessor = efCoreScopeAccessor;
    }

    public IEfCoreScope CreateScope()
    {
        if (_ambientEfCoreScopeStack.AmbientScope is null)
        {
            var ambientScope = new EfCoreScope(_umbracoEfCoreDatabaseFactory, _efCoreScopeAccessor, this);
            _ambientEfCoreScopeStack.Push(ambientScope);
            return ambientScope;
        }

        var efCoreScope = new EfCoreScope(_umbracoEfCoreDatabaseFactory, _efCoreScopeAccessor, this, _ambientEfCoreScopeStack.AmbientScope);
        _ambientEfCoreScopeStack.Push(efCoreScope);
        return efCoreScope;
    }

    public void PopAmbientScope() => _ambientEfCoreScopeStack.Pop();
}
