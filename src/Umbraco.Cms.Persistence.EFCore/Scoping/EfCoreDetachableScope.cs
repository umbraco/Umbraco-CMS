using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreDetachableScope : EfCoreScope
{
    private readonly IEFCoreScopeAccessor _efCoreScopeAccessor;
    private readonly EfCoreScopeProvider _efCoreScopeProvider;

    public EfCoreDetachableScope(
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        FileSystems fileSystems,
        IEfCoreScopeProvider efCoreScopeProvider,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator)
        : base(
            distributedLockingMechanismFactory,
            efCoreDatabaseFactory,
            efCoreScopeAccessor,
            fileSystems,
            efCoreScopeProvider,
            scopeContext,
            eventAggregator)
    {
        if (scopeContext is not null)
        {
            throw new ArgumentException("Cannot set context on detachable scope.", nameof(scopeContext));
        }

        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EfCoreScopeProvider)efCoreScopeProvider;

        Detachable = true;

        ScopeContext = new ScopeContext();
    }

    public EfCoreDetachableScope(
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        FileSystems fileSystems,
        IEfCoreScopeProvider efCoreScopeProvider,
        EfCoreScope parentScope,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator)
        : base(
            distributedLockingMechanismFactory,
            efCoreDatabaseFactory,
            efCoreScopeAccessor,
            fileSystems,
            efCoreScopeProvider,
            parentScope,
            scopeContext,
            eventAggregator) =>
        throw new NotImplementedException();

    public EfCoreScope? OriginalScope { get; set; }

    public IScopeContext? OriginalContext { get; set; }

    public bool Detachable { get; }

    public bool Attached { get; set; }

    public new void Dispose()
    {
        HandleDetachedScopes();
        base.Dispose();
    }

    private void HandleDetachedScopes()
    {
        if (Detachable)
        {
            // get out of the way, restore original

            // TODO: Difficult to know if this is correct since this is all required
            // by Deploy which I don't fully understand since there is limited tests on this in the CMS
            if (OriginalScope != _efCoreScopeAccessor.AmbientScope)
            {
                _efCoreScopeProvider.PopAmbientScope();
            }

            if (OriginalContext != _efCoreScopeProvider.AmbientScopeContext)
            {
                _efCoreScopeProvider.PopAmbientScopeContext();
            }

            Attached = false;
            OriginalScope = null;
            OriginalContext = null;
        }
    }
}
