using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;

/// <summary>
/// Represents an EF Core scope that provides database context access and transaction management.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
internal class EFCoreScope<TDbContext> : CoreScope, IEfCoreScope<TDbContext>
    where TDbContext : DbContext
{
    private readonly IEFCoreScopeAccessor<TDbContext> _efCoreScopeAccessor;
    private readonly EFCoreScopeProvider<TDbContext> _efCoreScopeProvider;
    private readonly IScope? _innerScope;
    private bool _disposed;
    private TDbContext? _dbContext;
    private IDbContextFactory<TDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreScope{TDbContext}"/> class.
    /// </summary>
    /// <param name="distributedLockingMechanismFactory">The distributed locking mechanism factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="efCoreScopeAccessor">The EF Core scope accessor.</param>
    /// <param name="scopedFileSystem">The scoped file system.</param>
    /// <param name="iefCoreScopeProvider">The EF Core scope provider.</param>
    /// <param name="scopeContext">The scope context.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="dbContextFactory">The DbContext factory.</param>
    /// <param name="repositoryCacheMode">The repository cache mode.</param>
    /// <param name="scopeFileSystems">Whether to scope file systems.</param>
    protected EFCoreScope(
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        IEFCoreScopeAccessor<TDbContext> efCoreScopeAccessor,
        FileSystems scopedFileSystem,
        IEFCoreScopeProvider<TDbContext> iefCoreScopeProvider,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        IDbContextFactory<TDbContext> dbContextFactory,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
        : base(distributedLockingMechanismFactory, loggerFactory, scopedFileSystem, eventAggregator, repositoryCacheMode, scopeFileSystems)
    {
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EFCoreScopeProvider<TDbContext>)iefCoreScopeProvider;
        ScopeContext = scopeContext;
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreScope{TDbContext}"/> class with an IScope parent.
    /// </summary>
    /// <param name="parentScope">The parent scope.</param>
    /// <param name="distributedLockingMechanismFactory">The distributed locking mechanism factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="efCoreScopeAccessor">The EF Core scope accessor.</param>
    /// <param name="scopedFileSystem">The scoped file system.</param>
    /// <param name="iefCoreScopeProvider">The EF Core scope provider.</param>
    /// <param name="scopeContext">The scope context.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="dbContextFactory">The DbContext factory.</param>
    /// <param name="repositoryCacheMode">The repository cache mode.</param>
    /// <param name="scopeFileSystems">Whether to scope file systems.</param>
    public EFCoreScope(
        IScope parentScope,
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        IEFCoreScopeAccessor<TDbContext> efCoreScopeAccessor,
        FileSystems scopedFileSystem,
        IEFCoreScopeProvider<TDbContext> iefCoreScopeProvider,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        IDbContextFactory<TDbContext> dbContextFactory,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
        : base(parentScope, distributedLockingMechanismFactory, loggerFactory, scopedFileSystem, eventAggregator, repositoryCacheMode, scopeFileSystems)
    {
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EFCoreScopeProvider<TDbContext>)iefCoreScopeProvider;
        ScopeContext = scopeContext;
        _innerScope = parentScope;
        _dbContextFactory = dbContextFactory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreScope{TDbContext}"/> class with an EFCoreScope parent.
    /// </summary>
    /// <param name="parentScope">The parent EF Core scope.</param>
    /// <param name="distributedLockingMechanismFactory">The distributed locking mechanism factory.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="efCoreScopeAccessor">The EF Core scope accessor.</param>
    /// <param name="scopedFileSystem">The scoped file system.</param>
    /// <param name="iefCoreScopeProvider">The EF Core scope provider.</param>
    /// <param name="scopeContext">The scope context.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="dbContextFactory">The DbContext factory.</param>
    /// <param name="repositoryCacheMode">The repository cache mode.</param>
    /// <param name="scopeFileSystems">Whether to scope file systems.</param>
    public EFCoreScope(
        EFCoreScope<TDbContext> parentScope,
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        IEFCoreScopeAccessor<TDbContext> efCoreScopeAccessor,
        FileSystems scopedFileSystem,
        IEFCoreScopeProvider<TDbContext> iefCoreScopeProvider,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        IDbContextFactory<TDbContext> dbContextFactory,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
        : base(parentScope, distributedLockingMechanismFactory, loggerFactory, scopedFileSystem, eventAggregator, repositoryCacheMode, scopeFileSystems)
    {
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EFCoreScopeProvider<TDbContext>)iefCoreScopeProvider;
        ScopeContext = scopeContext;
        ParentScope = parentScope;
        _dbContextFactory = dbContextFactory;
    }


    /// <summary>
    /// Gets the parent EF Core scope, if any.
    /// </summary>
    public EFCoreScope<TDbContext>? ParentScope { get; }

    /// <inheritdoc />
    public IScopeContext? ScopeContext { get; set; }

    /// <inheritdoc />
    public async Task<T> ExecuteWithContextAsync<T>(Func<TDbContext, Task<T>> method)
    {
        if (_disposed)
        {
            throw new InvalidOperationException(
                "The scope has been disposed, therefore the database is not available.");
        }

        if (_dbContext is null)
        {
            InitializeDatabase();
        }

        return await method(_dbContext!);
    }

    /// <inheritdoc />
    public async Task ExecuteWithContextAsync<T>(Func<TDbContext, Task> method) =>
        await ExecuteWithContextAsync(async db =>
        {
            await method(db);
            return true; // Do nothing
        });

    /// <summary>
    /// Resets the scope's completion state.
    /// </summary>
    public void Reset() => Completed = null;

    /// <inheritdoc />
    public override void Dispose()
    {
        if (this != _efCoreScopeAccessor.AmbientScope)
        {
            var failedMessage =
                $"The {nameof(EFCoreScope<TDbContext>)} {InstanceId} being disposed is not the Ambient {nameof(EFCoreScope<TDbContext>)} {_efCoreScopeAccessor.AmbientScope?.InstanceId.ToString() ?? "NULL"}. This typically indicates that a child {nameof(EFCoreScope<TDbContext>)} was not disposed, or flowed to a child thread that was not awaited, or concurrent threads are accessing the same {nameof(EFCoreScope<TDbContext>)} (Ambient context) which is not supported. If using Task.Run (or similar) as a fire and forget tasks or to run threads in parallel you must suppress execution context flow with ExecutionContext.SuppressFlow() and ExecutionContext.RestoreFlow().";
            throw new InvalidOperationException(failedMessage);
        }

        if (ParentScope is null)
        {
            DisposeEfCoreDatabase();
        }

        Locks.ClearLocks(InstanceId);

        // Since we can nest EFCoreScopes in other scopes derived from CoreScope, we should check whether our ParentScope OR the base ParentScope exists.
        // Only if neither do do we take responsibility for ensuring the locks are cleared.
        // Eventually the highest parent will clear the locks.
        // Further, these locks are a reference to the locks of the highest parent anyway (see the constructor of CoreScope).
#pragma warning disable SA1100 // Do not prefix calls with base unless local implementation exists (justification: provides additional clarify here that this is defined on the base class).
        if (ParentScope is null && base.HasParentScope is false)
        {
            Locks.EnsureLocksCleared(InstanceId);
        }
#pragma warning restore SA1100 // Do not prefix calls with base unless local implementation exists

        _efCoreScopeProvider.PopAmbientScope();

        HandleScopeContext();
        base.Dispose();

        _disposed = true;
        if (ParentScope is null)
        {
            if (Completed.HasValue && Completed.Value)
            {
                _innerScope?.Complete();
            }

            _innerScope?.Dispose();
        }
    }

    private void InitializeDatabase()
    {
        if (_dbContext is null)
        {
            _dbContext = FindDbContext();
        }

        // Check if we are already in a transaction before starting one
        if (_dbContext.Database.CurrentTransaction is null)
        {
            DbTransaction? transaction = _innerScope?.Database.Transaction;
            _dbContext.Database.SetDbConnection(transaction?.Connection);
            Locks.EnsureLocks(InstanceId);

            if (transaction is null)
            {
                _dbContext.Database.BeginTransaction();
            }
            else
            {
                _dbContext.Database.UseTransaction(transaction);
            }
        }
    }

    private TDbContext FindDbContext()
    {
        if (ParentScope is not null)
        {
            return ParentScope.FindDbContext();
        }

        return _dbContext ??= _dbContextFactory.CreateDbContext();
    }

    private void HandleScopeContext()
    {
        // if *we* created it, then get rid of it
        if (_efCoreScopeProvider.AmbientScopeContext == ScopeContext)
        {
            try
            {
                _efCoreScopeProvider.AmbientScopeContext?.ScopeExit(Completed.HasValue && Completed.Value);
            }
            finally
            {
                // removes the ambient context (ambient scope already gone)
                _efCoreScopeProvider.PopAmbientScopeContext();
            }
        }
    }

    private void DisposeEfCoreDatabase()
    {
        var completed = Completed.HasValue && Completed.Value;
        {
            try
            {
                if (_dbContext is null || _innerScope is not null)
                {
                    return;
                }

                // Transaction connection can be null here if we get chosen as the deadlock victim.
                if (_dbContext.Database.CurrentTransaction?.GetDbTransaction().Connection is null)
                {
                    return;
                }

                if (completed)
                {
                    _dbContext.Database.CommitTransaction();
                }
                else
                {
                    _dbContext.Database.RollbackTransaction();
                }
            }
            finally
            {
                _dbContext?.Dispose();
                _dbContext = null;
            }
        }
    }
}
