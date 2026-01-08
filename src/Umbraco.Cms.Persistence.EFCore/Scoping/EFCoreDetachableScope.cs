using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal sealed class EFCoreDetachableScope<TDbContext> : EFCoreScope<TDbContext> where TDbContext : DbContext
{
    private readonly IEFCoreScopeAccessor<TDbContext> _efCoreScopeAccessor;
    private readonly EFCoreScopeProvider<TDbContext> _efCoreScopeProvider;

    public EFCoreDetachableScope(
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        IEFCoreScopeAccessor<TDbContext> efCoreScopeAccessor,
        FileSystems fileSystems,
        IEFCoreScopeProvider<TDbContext> efCoreScopeProvider,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        IDbContextFactory<TDbContext> dbContextFactory,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
        : base(
            distributedLockingMechanismFactory,
            loggerFactory,
            efCoreScopeAccessor,
            fileSystems,
            efCoreScopeProvider,
            scopeContext,
            eventAggregator,
            dbContextFactory,
            repositoryCacheMode,
            scopeFileSystems)
    {
        if (scopeContext is not null)
        {
            throw new ArgumentException("Cannot set context on detachable scope.", nameof(scopeContext));
        }

        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EFCoreScopeProvider<TDbContext>)efCoreScopeProvider;

        Detachable = true;

        ScopeContext = new ScopeContext();
    }

    public EFCoreDetachableScope(
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        IEFCoreScopeAccessor<TDbContext> efCoreScopeAccessor,
        FileSystems fileSystems,
        IEFCoreScopeProvider<TDbContext> efCoreScopeProvider,
        EFCoreScope<TDbContext> parentScope,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        IDbContextFactory<TDbContext> dbContextFactory)
        : base(
            parentScope,
            distributedLockingMechanismFactory,
            loggerFactory,
            efCoreScopeAccessor,
            fileSystems,
            efCoreScopeProvider,
            scopeContext,
            eventAggregator,
            dbContextFactory) =>
        throw new NotImplementedException();

    public EFCoreScope<TDbContext>? OriginalScope { get; set; }

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
