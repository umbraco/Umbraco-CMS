using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScope<TDbContext> : CoreScope, IEfCoreScope<TDbContext>
    where TDbContext : DbContext
{
    private readonly IEFCoreScopeAccessor<TDbContext> _efCoreScopeAccessor;
    private readonly EfCoreScopeProvider<TDbContext> _efCoreScopeProvider;
    private readonly IScope? _innerScope;
    private bool _disposed;
    private TDbContext? _dbContext;
    private IDbContextFactory<TDbContext> _dbContextFactory;

    protected EfCoreScope(
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        IEFCoreScopeAccessor<TDbContext> efCoreScopeAccessor,
        FileSystems scopedFileSystem,
        IEfCoreScopeProvider<TDbContext> efCoreScopeProvider,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        IDbContextFactory<TDbContext> dbContextFactory,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
        : base(distributedLockingMechanismFactory, loggerFactory, scopedFileSystem, eventAggregator, repositoryCacheMode, scopeFileSystems)
    {
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EfCoreScopeProvider<TDbContext>)efCoreScopeProvider;
        ScopeContext = scopeContext;
        _dbContextFactory = dbContextFactory;
    }

    public EfCoreScope(
        IScope parentScope,
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        IEFCoreScopeAccessor<TDbContext> efCoreScopeAccessor,
        FileSystems scopedFileSystem,
        IEfCoreScopeProvider<TDbContext> efCoreScopeProvider,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        IDbContextFactory<TDbContext> dbContextFactory,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
        : base(parentScope, distributedLockingMechanismFactory, loggerFactory, scopedFileSystem, eventAggregator, repositoryCacheMode, scopeFileSystems)
    {
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EfCoreScopeProvider<TDbContext>)efCoreScopeProvider;
        ScopeContext = scopeContext;
        _innerScope = parentScope;
        _dbContextFactory = dbContextFactory;
    }

    public EfCoreScope(
        EfCoreScope<TDbContext> parentScope,
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        IEFCoreScopeAccessor<TDbContext> efCoreScopeAccessor,
        FileSystems scopedFileSystem,
        IEfCoreScopeProvider<TDbContext> efCoreScopeProvider,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        IDbContextFactory<TDbContext> dbContextFactory,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
        : base(parentScope, distributedLockingMechanismFactory, loggerFactory, scopedFileSystem, eventAggregator, repositoryCacheMode, scopeFileSystems)
    {
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EfCoreScopeProvider<TDbContext>)efCoreScopeProvider;
        ScopeContext = scopeContext;
        ParentScope = parentScope;
        _dbContextFactory = dbContextFactory;
    }


    public EfCoreScope<TDbContext>? ParentScope { get; }

    public IScopeContext? ScopeContext { get; set; }

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

    public async Task ExecuteWithContextAsync<T>(Func<TDbContext, Task> method) =>
        await ExecuteWithContextAsync(async db =>
        {
            await method(db);
            return true; // Do nothing
        });

    public void Reset() => Completed = null;

    public override void Dispose()
    {
        if (this != _efCoreScopeAccessor.AmbientScope)
        {
            var failedMessage =
                $"The {nameof(EfCoreScope<TDbContext>)} {InstanceId} being disposed is not the Ambient {nameof(EfCoreScope<TDbContext>)} {_efCoreScopeAccessor.AmbientScope?.InstanceId.ToString() ?? "NULL"}. This typically indicates that a child {nameof(EfCoreScope<TDbContext>)} was not disposed, or flowed to a child thread that was not awaited, or concurrent threads are accessing the same {nameof(EfCoreScope<TDbContext>)} (Ambient context) which is not supported. If using Task.Run (or similar) as a fire and forget tasks or to run threads in parallel you must suppress execution context flow with ExecutionContext.SuppressFlow() and ExecutionContext.RestoreFlow().";
            throw new InvalidOperationException(failedMessage);
        }

        if (ParentScope is null)
        {
            DisposeEfCoreDatabase();
        }

        Locks.ClearLocks(InstanceId);

        if (ParentScope is null)
        {
            Locks.EnsureLocksCleared(InstanceId);
        }

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
