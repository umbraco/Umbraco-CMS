using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Entities;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScope : CoreScope, IEfCoreScope
{
    private readonly IUmbracoEfCoreDatabaseFactory _efCoreDatabaseFactory;
    private readonly IEFCoreScopeAccessor _efCoreScopeAccessor;
    private readonly EfCoreScopeProvider _efCoreScopeProvider;
    private IUmbracoEfCoreDatabase? _umbracoEfCoreDatabase;
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
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
        : base(distributedLockingMechanismFactory, loggerFactory, scopedFileSystem, eventAggregator, repositoryCacheMode, scopeFileSystems)
    {
        _efCoreDatabaseFactory = efCoreDatabaseFactory;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _efCoreScopeProvider = (EfCoreScopeProvider)efCoreScopeProvider;
        ScopeContext = scopeContext;
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

    public void Reset() => _completed = null;

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

        _efCoreScopeProvider.PopAmbientScope();

        HandleScopeContext();
        base.Dispose();

        _disposed = true;
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
            Locks.EnsureLocks(InstanceId);
            _umbracoEfCoreDatabase.UmbracoEFContext.Database.BeginTransaction();
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
                _efCoreScopeProvider.AmbientScopeContext?.ScopeExit(_completed.HasValue && _completed.Value);
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
        var completed = _completed.HasValue && _completed.Value;
        {
            bool databaseException = false;
            try
            {
                if (_umbracoEfCoreDatabase is not null)
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
