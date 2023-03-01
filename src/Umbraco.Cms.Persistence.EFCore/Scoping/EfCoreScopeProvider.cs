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

    public IEfCoreScope CreateDetachedScope() => new EfCoreDetachableScope(_umbracoEfCoreDatabaseFactory, _efCoreScopeAccessor, this, null);

    public void AttachScope(IEfCoreScope other)
    {
        // IScopeProvider.AttachScope works with an IEFCoreScope
        // but here we can only deal with our own Scope class
        if (other is not EfCoreDetachableScope otherScope)
        {
            throw new ArgumentException("Not a Scope instance.");
        }

        if (otherScope.Detachable == false)
        {
            throw new ArgumentException("Not a detachable scope.");
        }

        if (otherScope.Attached)
        {
            throw new InvalidOperationException("Already attached.");
        }

        otherScope.Attached = true;
        otherScope.OriginalScope = (EfCoreScope)_ambientEfCoreScopeStack.AmbientScope!;
        otherScope.OriginalContext = AmbientScopeContext;

        PushAmbientScopeContext(otherScope.ScopeContext);
        _ambientEfCoreScopeStack.Push(otherScope);
    }

    public IEfCoreScope DetachScope()
    {
        if (_ambientEfCoreScopeStack.AmbientScope is not EfCoreDetachableScope ambientScope)
        {
            throw new InvalidOperationException("Ambient scope is not detachable");
        }

        if (ambientScope == null)
        {
            throw new InvalidOperationException("There is no ambient scope.");
        }

        if (ambientScope.Detachable == false)
        {
            throw new InvalidOperationException("Ambient scope is not detachable.");
        }

        PopAmbientScope();
        PopAmbientScopeContext();

        var originalScope = (EfCoreScope)_ambientEfCoreScopeStack.AmbientScope!;
        if (originalScope != ambientScope.OriginalScope)
        {
            throw new InvalidOperationException($"The detatched scope ({ambientScope.InstanceId}) does not match the original ({originalScope.InstanceId})");
        }

        IScopeContext? originalScopeContext = AmbientScopeContext;
        if (originalScopeContext != ambientScope.OriginalContext)
        {
            throw new InvalidOperationException($"The detatched scope context does not match the original");
        }

        ambientScope.OriginalScope = null;
        ambientScope.OriginalContext = null;
        ambientScope.Attached = false;
        return ambientScope;
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

    //TODO: Move to ILockingMechanism
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
