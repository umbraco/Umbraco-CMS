using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Extensions;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScope : CoreScope, IEfCoreScope
{
    private readonly IUmbracoEfCoreDatabaseFactory _efCoreDatabaseFactory;
    private readonly IEFCoreScopeAccessor _efCoreScopeAccessor;
    private readonly EfCoreScopeProvider _efCoreScopeProvider;
    private IUmbracoEfCoreDatabase? _umbracoEfCoreDatabase;
    private IScope? _innerScope;
    private bool _disposed;

    public EfCoreScope(
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        FileSystems scopedFileSystem,
        IEfCoreScopeProvider efCoreScopeProvider,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        IScopeProvider scopeProvider,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
        : base(distributedLockingMechanismFactory, loggerFactory, scopedFileSystem, eventAggregator, repositoryCacheMode, scopeFileSystems)
    {
        _efCoreDatabaseFactory = efCoreDatabaseFactory;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EfCoreScopeProvider)efCoreScopeProvider;
        ScopeContext = scopeContext;
        _innerScope = scopeProvider.CreateScope();
    }

    public EfCoreScope(
        EfCoreScope parentScope,
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        FileSystems scopedFileSystem,
        IEfCoreScopeProvider efCoreScopeProvider,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
        : base(parentScope, distributedLockingMechanismFactory, loggerFactory, scopedFileSystem, eventAggregator, repositoryCacheMode, scopeFileSystems)
    {
        _efCoreDatabaseFactory = efCoreDatabaseFactory;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EfCoreScopeProvider)efCoreScopeProvider;
        ScopeContext = scopeContext;
        ParentScope = parentScope;
    }

    public EfCoreScope? ParentScope { get; }

    public IScopeContext? ScopeContext { get; set; }

    public async Task<T> ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task<T>> method)
    {
        if (_disposed)
        {
            throw new InvalidOperationException(
                "The scope has been disposed, therefore the database is not available.");
        }

        if (_umbracoEfCoreDatabase is null)
        {
            InitializeDatabase();
        }

        return await method(_umbracoEfCoreDatabase!.UmbracoEFContext);
    }

    public async Task ExecuteWithContextAsync<T>(Func<UmbracoEFContext, Task> method) =>
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
                $"The {nameof(EfCoreScope)} {InstanceId} being disposed is not the Ambient {nameof(EfCoreScope)} {_efCoreScopeAccessor.AmbientScope?.InstanceId.ToString() ?? "NULL"}. This typically indicates that a child {nameof(EfCoreScope)} was not disposed, or flowed to a child thread that was not awaited, or concurrent threads are accessing the same {nameof(EfCoreScope)} (Ambient context) which is not supported. If using Task.Run (or similar) as a fire and forget tasks or to run threads in parallel you must suppress execution context flow with ExecutionContext.SuppressFlow() and ExecutionContext.RestoreFlow().";
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
        if (_umbracoEfCoreDatabase is null)
        {
            _umbracoEfCoreDatabase = FindDatabase();
        }

        // Check if we are already in a transaction before starting one
        if (_umbracoEfCoreDatabase.UmbracoEFContext.Database.CurrentTransaction is null)
        {
            DbTransaction? transaction = _innerScope?.Database.Transaction;
            Locks.EnsureLocks(InstanceId);
            _umbracoEfCoreDatabase.UmbracoEFContext.Database.SetDbConnection(transaction?.Connection);

            if (transaction is null)
            {
                _umbracoEfCoreDatabase.UmbracoEFContext.Database.BeginTransaction();
            }
            else
            {
                _umbracoEfCoreDatabase.UmbracoEFContext.Database.UseTransaction(transaction);
            }
        }
    }

    private IUmbracoEfCoreDatabase FindDatabase()
    {
        if (ParentScope is not null)
        {
            return ParentScope.FindDatabase();
        }

        return _umbracoEfCoreDatabase ??= _efCoreDatabaseFactory.Create();
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
            bool databaseException = false;
            try
            {
                if (_umbracoEfCoreDatabase is not null && _innerScope?.Database.Transaction is null)
                {
                    // Transaction connection can be null here if we get chosen as the deadlock victim.
                    if (_umbracoEfCoreDatabase.UmbracoEFContext.Database.CurrentTransaction?.GetDbTransaction()
                            .Connection is not null)
                    {
                        if (completed)
                        {
                            _umbracoEfCoreDatabase.UmbracoEFContext.Database.CommitTransaction();
                        }
                        else
                        {
                            _umbracoEfCoreDatabase.UmbracoEFContext.Database.RollbackTransaction();
                        }
                    }
                }
            }
            catch
            {
                databaseException = true;
            }
            finally
            {
                _umbracoEfCoreDatabase?.Dispose();
                _umbracoEfCoreDatabase = null;
                _efCoreDatabaseFactory.Dispose();
            }
        }
    }
}
