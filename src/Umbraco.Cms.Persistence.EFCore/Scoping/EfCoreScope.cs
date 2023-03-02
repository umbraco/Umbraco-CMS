using Microsoft.EntityFrameworkCore.Storage;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Persistence.EFCore.Entities;
using Umbraco.Cms.Persistence.EFCore.Services;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScope : IEfCoreScope
{
    private readonly IUmbracoEfCoreDatabaseFactory _efCoreDatabaseFactory;
    private readonly IEFCoreScopeAccessor _efCoreScopeAccessor;
    private readonly EfCoreScopeProvider _efCoreScopeProvider;
    private readonly IEventAggregator _eventAggregator;
    private readonly RepositoryCacheMode _repositoryCacheMode;
    private readonly bool? _scopeFileSystems;

    private ICompletable? _scopedFileSystem;
    private IsolatedCaches? _isolatedCaches;
    private IUmbracoEfCoreDatabase? _umbracoEfCoreDatabase;
    private IScopedNotificationPublisher? _notificationPublisher;
    private bool? _completed;
    private bool _disposed;

    public EfCoreScope(
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        FileSystems scopedFileSystem,
        IEfCoreScopeProvider efCoreScopeProvider,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
    {
        _efCoreDatabaseFactory = efCoreDatabaseFactory;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _eventAggregator = eventAggregator;
        _repositoryCacheMode = repositoryCacheMode;
        _scopeFileSystems = scopeFileSystems;
        _efCoreScopeProvider = (EfCoreScopeProvider)efCoreScopeProvider;
        InstanceId = Guid.NewGuid();
        Locks = ParentScope is null ? new LockingMechanism(distributedLockingMechanismFactory) : ResolveLockingMechanism();
        ScopeContext = scopeContext;

        if (scopeFileSystems is true)
        {
            _scopedFileSystem = scopedFileSystem.Shadow();
        }
    }

    public EfCoreScope(
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IEFCoreScopeAccessor efCoreScopeAccessor,
        FileSystems scopedFileSystem,
        IEfCoreScopeProvider efCoreScopeProvider,
        EfCoreScope parentScope,
        IScopeContext? scopeContext,
        IEventAggregator eventAggregator,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null)
        : this(
            distributedLockingMechanismFactory,
            efCoreDatabaseFactory,
            efCoreScopeAccessor,
            scopedFileSystem,
            efCoreScopeProvider,
            scopeContext,
            eventAggregator,
            repositoryCacheMode,
            scopeFileSystems)
    {
        // cannot specify a different fs scope!
        // can be 'true' only on outer scope (and false does not make much sense)
        if (scopeFileSystems != null && ParentScope?._scopeFileSystems != _scopeFileSystems)
        {
            throw new ArgumentException($"Value '{scopeFileSystems.Value}' be different from parent value '{ParentScope?._scopeFileSystems}'.", nameof(scopeFileSystems));
        }

        ParentScope = parentScope;
    }


    public Guid InstanceId { get; }

    public EfCoreScope? ParentScope { get; }

    public IScopeContext? ScopeContext { get; set; }

    public ILockingMechanism Locks { get; }

    public int Depth
    {
        get
        {
            if (ParentScope == null)
            {
                return 0;
            }

            return ParentScope.Depth + 1;
        }
    }

    public IScopedNotificationPublisher Notifications
    {
        get
        {
            EnsureNotDisposed();
            if (ParentScope != null)
            {
                return ParentScope.Notifications;
            }

            return _notificationPublisher ??= new ScopedNotificationPublisher(_eventAggregator);
        }
    }

    public RepositoryCacheMode RepositoryCacheMode
    {
        get
        {
            if (_repositoryCacheMode != RepositoryCacheMode.Unspecified)
            {
                return _repositoryCacheMode;
            }

            if (ParentScope != null)
            {
                return ParentScope.RepositoryCacheMode;
            }

            return RepositoryCacheMode.Default;
        }
    }

    public IsolatedCaches IsolatedCaches
    {
        get
        {
            if (ParentScope != null)
            {
                return ParentScope.IsolatedCaches;
            }

            return _isolatedCaches ??= new IsolatedCaches(_ => new DeepCloneAppCache(new ObjectCacheAppCache()));
        }
    }

    public bool ScopedFileSystems
    {
        get
        {
            if (ParentScope != null)
            {
                return ParentScope.ScopedFileSystems;
            }

            return _scopedFileSystem != null;
        }
    }

    private ILockingMechanism ResolveLockingMechanism() => ParentScope is not null ? ParentScope.ResolveLockingMechanism() : Locks;

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

    public void Complete()
    {
        if (_completed.HasValue == false)
        {
            _completed = true;
        }
    }

    public void Reset() => _completed = null;

    public void Dispose()
    {
        if (this != _efCoreScopeAccessor.AmbientScope)
        {
            var failedMessage =
                $"The {nameof(EfCoreScope)} {InstanceId} being disposed is not the Ambient {nameof(EfCoreScope)} {_efCoreScopeAccessor.AmbientScope?.InstanceId.ToString() ?? "NULL"}. This typically indicates that a child {nameof(EfCoreScope)} was not disposed, or flowed to a child thread that was not awaited, or concurrent threads are accessing the same {nameof(EfCoreScope)} (Ambient context) which is not supported. If using Task.Run (or similar) as a fire and forget tasks or to run threads in parallel you must suppress execution context flow with ExecutionContext.SuppressFlow() and ExecutionContext.RestoreFlow().";
            throw new InvalidOperationException(failedMessage);
        }

        // Decrement the lock counters on the parent if any.
        Locks.ClearLocks(InstanceId);

        if (ParentScope is null)
        {
            DisposeEfCoreDatabase();
            HandleScopedFileSystems(_completed.HasValue && _completed.Value);
            HandleScopedNotifications();
        }
        else
        {
            ParentScope.ChildCompleted(_completed);
        }

        _efCoreScopeProvider.PopAmbientScope();


        HandleScopeContext();

        _disposed = true;
    }

    private void EnsureNotDisposed()
    {
        // We can't be disposed
        if (_disposed)
        {
            throw new ObjectDisposedException($"The {nameof(IEfCoreScope)} with ID ({InstanceId}) is already disposed");
        }

        // And neither can our ancestors if we're trying to be disposed since
        // a child must always be disposed before it's parent.
        // This is a safety check, it's actually not entirely possible that a parent can be
        // disposed before the child since that will end up with a "not the Ambient" exception.
        ParentScope?.EnsureNotDisposed();
    }

    public void ChildCompleted(bool? completed)
    {
        // if child did not complete we cannot complete
        if (completed.HasValue == false || completed.Value == false)
        {
            _completed = false;
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
            Locks.EnsureDbLocks(InstanceId);
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

    private void HandleScopedFileSystems(bool completed)
    {
        if (_scopeFileSystems == true)
        {
            if (completed)
            {
                _scopedFileSystem?.Complete();
            }

            _scopedFileSystem?.Dispose();
            _scopedFileSystem = null;
        }
    }

    private void HandleScopedNotifications() => _notificationPublisher?.ScopeExit(_completed.HasValue && _completed.Value);

    private void DisposeEfCoreDatabase()
    {
        var completed = _completed.HasValue && _completed.Value;
        if (_umbracoEfCoreDatabase is not null)
        {
            // Transaction connection can be null here if we get chosen as the deadlock victim.
            if (_umbracoEfCoreDatabase.UmbracoEFContext.Database.CurrentTransaction?.GetDbTransaction().Connection is not null)
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


            _umbracoEfCoreDatabase.Dispose();
        }

        _efCoreDatabaseFactory.Dispose();
    }
}
