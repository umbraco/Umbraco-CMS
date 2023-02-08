using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

public class EfCoreScopeProvider : IEfCoreScopeProvider
{
    private readonly IAmbientEfCoreScopeStack _ambientEfCoreScopeStack;
    private readonly IUmbracoEfCoreDatabaseFactory _umbracoEfCoreDatabaseFactory;
    private readonly IEFCoreScopeAccessor _efCoreScopeAccessor;
    private readonly IAmbientEFCoreScopeContextStack _ambientEfCoreScopeContextStack;

    // Needed for DI as IAmbientEfCoreScopeStack is internal
    public EfCoreScopeProvider()
        : this(
            StaticServiceProvider.Instance.GetRequiredService<IAmbientEfCoreScopeStack>(),
            StaticServiceProvider.Instance.GetRequiredService<IUmbracoEfCoreDatabaseFactory>(),
            StaticServiceProvider.Instance.GetRequiredService<IEFCoreScopeAccessor>(),
            StaticServiceProvider.Instance.GetRequiredService<IAmbientEFCoreScopeContextStack>(),
            StaticServiceProvider.Instance.GetRequiredService<IDistributedLockingMechanismFactory>())
    {
    }

    internal EfCoreScopeProvider(
        IAmbientEfCoreScopeStack ambientEfCoreScopeStack,
        IUmbracoEfCoreDatabaseFactory umbracoEfCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        IAmbientEFCoreScopeContextStack ambientEfCoreScopeContextStack,
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory)
    {
        _ambientEfCoreScopeStack = ambientEfCoreScopeStack;
        _umbracoEfCoreDatabaseFactory = umbracoEfCoreDatabaseFactory;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _ambientEfCoreScopeContextStack = ambientEfCoreScopeContextStack;
        DistributedLockingMechanismFactory = distributedLockingMechanismFactory;
    }

    public IScopeContext? AmbientScopeContext => _ambientEfCoreScopeContextStack.AmbientContext;

    public IEfCoreScope CreateScope()
    {
        if (_ambientEfCoreScopeStack.AmbientScope is null)
        {
            ScopeContext? newContext = _ambientEfCoreScopeContextStack.AmbientContext == null ? new ScopeContext() : null;
            var ambientScope = new EfCoreScope(_umbracoEfCoreDatabaseFactory, _efCoreScopeAccessor, this, newContext);

            if (newContext != null)
            {
                PushAmbientScopeContext(newContext);
            }

            _ambientEfCoreScopeStack.Push(ambientScope);
            return ambientScope;
        }

        var efCoreScope = new EfCoreScope(_umbracoEfCoreDatabaseFactory, _efCoreScopeAccessor, this, (EfCoreScope)_ambientEfCoreScopeStack.AmbientScope, null);

        _ambientEfCoreScopeStack.Push(efCoreScope);
        return efCoreScope;
    }

    public IDistributedLockingMechanismFactory DistributedLockingMechanismFactory { get; }

    public void PopAmbientScope() => _ambientEfCoreScopeStack.Pop();

    public void PushAmbientScopeContext(IScopeContext? scopeContext)
    {
        if (scopeContext is null)
        {
            throw new ArgumentNullException(nameof(scopeContext));
        }
        _ambientEfCoreScopeContextStack.Push(scopeContext);
    }

    public void PopAmbientScopeContext() => _ambientEfCoreScopeContextStack.Pop();
}
