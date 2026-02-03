using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

/// <summary>
/// Represents a detachable EF Core scope that can be detached and reattached to the ambient scope stack.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
internal sealed class EFCoreDetachableScope<TDbContext> : EFCoreScope<TDbContext> where TDbContext : DbContext
{
    private readonly IEFCoreScopeAccessor<TDbContext> _efCoreScopeAccessor;
    private readonly EFCoreScopeProvider<TDbContext> _efCoreScopeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreDetachableScope{TDbContext}"/> class.
    /// </summary>
    /// <param name="distributedLockingMechanismFactory">The distributed locking mechanism factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="efCoreScopeAccessor">The EF Core scope accessor.</param>
    /// <param name="fileSystems">The file systems.</param>
    /// <param name="efCoreScopeProvider">The EF Core scope provider.</param>
    /// <param name="scopeContext">The scope context (must be null for detachable scopes).</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="dbContextFactory">The DbContext factory.</param>
    /// <param name="repositoryCacheMode">The repository cache mode.</param>
    /// <param name="scopeFileSystems">Whether to scope file systems.</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreDetachableScope{TDbContext}"/> class with a parent scope.
    /// </summary>
    /// <param name="distributedLockingMechanismFactory">The distributed locking mechanism factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="efCoreScopeAccessor">The EF Core scope accessor.</param>
    /// <param name="fileSystems">The file systems.</param>
    /// <param name="efCoreScopeProvider">The EF Core scope provider.</param>
    /// <param name="parentScope">The parent scope.</param>
    /// <param name="scopeContext">The scope context.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="dbContextFactory">The DbContext factory.</param>
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

    /// <summary>
    /// Gets or sets the original scope that was active when this detachable scope was attached.
    /// </summary>
    public EFCoreScope<TDbContext>? OriginalScope { get; set; }

    /// <summary>
    /// Gets or sets the original scope context that was active when this detachable scope was attached.
    /// </summary>
    public IScopeContext? OriginalContext { get; set; }

    /// <summary>
    /// Gets a value indicating whether this scope is detachable.
    /// </summary>
    public bool Detachable { get; }

    /// <summary>
    /// Gets or sets a value indicating whether this scope is currently attached.
    /// </summary>
    public bool Attached { get; set; }

    /// <summary>
    /// Disposes the scope and handles detached scope cleanup.
    /// </summary>
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
