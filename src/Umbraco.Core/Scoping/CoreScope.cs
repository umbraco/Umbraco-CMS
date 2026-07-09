using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.Scoping;

/// <summary>
///     Represents a core scope that manages transactional operations, caching, locking, and notifications.
/// </summary>
/// <remarks>
///     <para>Scopes can be nested; a child scope must be completed before its parent.</para>
///     <para>The scope manages file system scoping, isolated caches, and notification publishing.</para>
/// </remarks>
public class CoreScope : ICoreScope
{
    /// <summary>
    ///     Indicates whether the scope has been completed.
    /// </summary>
    /// <remarks>
    ///     TODO (V18): Rename to _completed to comply with SA1306 (field names should begin with lowercase), or consider converting to a property.
    /// </remarks>
    protected bool? Completed;
    private ICompletable? _scopedFileSystem;
    private IScopedNotificationPublisher? _notificationPublisher;
    private IsolatedCaches? _isolatedCaches;
    private ICoreScope? _parentScope;

    private readonly RepositoryCacheMode _repositoryCacheMode;
    private readonly bool? _shouldScopeFileSystems;
    private readonly IEventAggregator _eventAggregator;

    private bool _disposed;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CoreScope"/> class as a root scope.
    /// </summary>
    /// <param name="distributedLockingMechanismFactory">The factory for creating distributed locking mechanisms.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="scopedFileSystem">The file systems to potentially scope.</param>
    /// <param name="eventAggregator">The event aggregator for publishing notifications.</param>
    /// <param name="repositoryCacheMode">The repository cache mode for this scope.</param>
    /// <param name="shouldScopeFileSystems">A value indicating whether to scope the file systems.</param>
    /// <param name="notificationPublisher">An optional scoped notification publisher.</param>
    protected CoreScope(
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        FileSystems scopedFileSystem,
        IEventAggregator eventAggregator,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? shouldScopeFileSystems = null,
        IScopedNotificationPublisher? notificationPublisher = null)
    {
        _eventAggregator = eventAggregator;
        InstanceId = Guid.NewGuid();
        CreatedThreadId = Environment.CurrentManagedThreadId;
        Locks = ParentScope is null
            ? new LockingMechanism(distributedLockingMechanismFactory, loggerFactory.CreateLogger<LockingMechanism>())
            : ResolveLockingMechanism();
        _repositoryCacheMode = repositoryCacheMode;
        _shouldScopeFileSystems = shouldScopeFileSystems;
        _notificationPublisher = notificationPublisher;

        if (_shouldScopeFileSystems is true)
        {
            _scopedFileSystem = scopedFileSystem.Shadow();
        }
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="CoreScope"/> class with an optional parent scope.
    /// </summary>
    /// <param name="parentScope">The parent scope, or <c>null</c> if this is a root scope.</param>
    /// <param name="distributedLockingMechanismFactory">The factory for creating distributed locking mechanisms.</param>
    /// <param name="loggerFactory">The logger factory for creating loggers.</param>
    /// <param name="scopedFileSystem">The file systems to potentially scope.</param>
    /// <param name="eventAggregator">The event aggregator for publishing notifications.</param>
    /// <param name="repositoryCacheMode">The repository cache mode for this scope.</param>
    /// <param name="shouldScopeFileSystems">A value indicating whether to scope the file systems.</param>
    /// <param name="notificationPublisher">An optional scoped notification publisher.</param>
    /// <exception cref="ArgumentException">
    ///     Thrown when the repository cache mode is lower than the parent's mode,
    ///     when a notification publisher is specified on a nested scope,
    ///     or when the file system scoping differs from the parent.
    /// </exception>
    protected CoreScope(
        ICoreScope? parentScope,
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        ILoggerFactory loggerFactory,
        FileSystems scopedFileSystem,
        IEventAggregator eventAggregator,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? shouldScopeFileSystems = null,
        IScopedNotificationPublisher? notificationPublisher = null)
    {
        _eventAggregator = eventAggregator;
        InstanceId = Guid.NewGuid();
        CreatedThreadId = Environment.CurrentManagedThreadId;
        _repositoryCacheMode = repositoryCacheMode;
        _shouldScopeFileSystems = shouldScopeFileSystems;
        _notificationPublisher = notificationPublisher;

        if (parentScope is null)
        {
            Locks = new LockingMechanism(distributedLockingMechanismFactory, loggerFactory.CreateLogger<LockingMechanism>());
            if (_shouldScopeFileSystems is true)
            {
                _scopedFileSystem = scopedFileSystem.Shadow();
            }

            return;
        }

        Locks = parentScope.Locks;

        // cannot specify a different mode!
        // TODO: means that it's OK to go from L2 to None for reading purposes, but writing would be BAD!
        // this is for XmlStore that wants to bypass caches when rebuilding XML (same for NuCache)
        if (repositoryCacheMode != RepositoryCacheMode.Unspecified &&
            parentScope.RepositoryCacheMode > repositoryCacheMode)
        {
            throw new ArgumentException(
                $"Value '{repositoryCacheMode}' cannot be lower than parent value '{parentScope.RepositoryCacheMode}'.", nameof(repositoryCacheMode));
        }

        // Only the outermost scope can specify the notification publisher
        if (_notificationPublisher != null)
        {
            throw new ArgumentException("Value cannot be specified on nested scope.", nameof(_notificationPublisher));
        }

        _parentScope = parentScope;

        // cannot specify a different fs scope!
        // can be 'true' only on outer scope (and false does not make much sense)
        if (_shouldScopeFileSystems != null && ParentScope?._shouldScopeFileSystems != _shouldScopeFileSystems)
        {
            throw new ArgumentException(
                $"Value '{_shouldScopeFileSystems.Value}' be different from parent value '{ParentScope?._shouldScopeFileSystems}'.",
                nameof(_shouldScopeFileSystems));
        }
    }

    /// <summary>
    ///     Gets the parent scope as a <see cref="CoreScope"/>.
    /// </summary>
    private CoreScope? ParentScope => (CoreScope?)_parentScope;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Guid InstanceId { get; }

    /// <inheritdoc />
    public int CreatedThreadId { get; }

    /// <inheritdoc />
    public ILockingMechanism Locks { get; }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public RepositoryCacheMode RepositoryCacheMode
    {
        get
        {
            if (_repositoryCacheMode != RepositoryCacheMode.Unspecified)
            {
                return _repositoryCacheMode;
            }

            return ParentScope?.RepositoryCacheMode ?? RepositoryCacheMode.Default;
        }
    }

    /// <inheritdoc />
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

    /// <summary>
    ///     Gets a value indicating whether file systems are scoped for this scope.
    /// </summary>
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

    /// <summary>
    /// Completes a scope
    /// </summary>
    /// <returns>A value indicating whether the scope is completed or not.</returns>
    public bool Complete()
    {
        if (Completed.HasValue == false)
        {
            Completed = true;
        }

        return Completed.Value;
    }

    /// <inheritdoc />
    public void ReadLock(params int[] lockIds) => Locks.ReadLock(InstanceId, null, lockIds);

    /// <inheritdoc />
    public void WriteLock(params int[] lockIds) => Locks.WriteLock(InstanceId, null, lockIds);

    /// <inheritdoc />
    public void WriteLock(TimeSpan timeout, int lockId) => Locks.ReadLock(InstanceId, timeout, lockId);

    /// <inheritdoc />
    public void ReadLock(TimeSpan timeout, int lockId) => Locks.WriteLock(InstanceId, timeout, lockId);

    /// <inheritdoc />
    public void EagerWriteLock(params int[] lockIds) => Locks.EagerWriteLock(InstanceId, null, lockIds);

    /// <inheritdoc />
    public void EagerWriteLock(TimeSpan timeout, int lockId) => Locks.EagerWriteLock(InstanceId, timeout, lockId);

    /// <inheritdoc />
    public void EagerReadLock(TimeSpan timeout, int lockId) => Locks.EagerReadLock(InstanceId, timeout, lockId);

    /// <inheritdoc />
    public void EagerReadLock(params int[] lockIds) => Locks.EagerReadLock(InstanceId, TimeSpan.Zero, lockIds);

    /// <summary>
    ///     Disposes the scope, handling file systems, notifications, and parent scope completion.
    /// </summary>
    public virtual void Dispose()
    {
        if (ParentScope is null)
        {
            HandleScopedFileSystems();
            HandleScopedNotifications();
        }
        else
        {
            ParentScope.ChildCompleted(Completed);
        }

        _disposed = true;
    }

    /// <summary>
    ///     Called when a child scope has completed, to update the parent's completion status.
    /// </summary>
    /// <param name="completed">A value indicating whether the child completed successfully.</param>
    protected void ChildCompleted(bool? completed)
    {
        // if child did not complete we cannot complete
        if (completed.HasValue == false || completed.Value == false)
        {
            Completed = false;
        }
    }

    /// <summary>
    ///     Handles the completion and disposal of scoped file systems.
    /// </summary>
    protected void HandleScopedFileSystems()
    {
        if (_shouldScopeFileSystems == true)
        {
            if (Completed.HasValue && Completed.Value)
            {
                _scopedFileSystem?.Complete();
            }

            _scopedFileSystem?.Dispose();
            _scopedFileSystem = null;
        }
    }

    /// <summary>
    ///     Sets the parent scope for this scope.
    /// </summary>
    /// <param name="coreScope">The parent scope to set.</param>
    protected void SetParentScope(ICoreScope coreScope)
    {
        _parentScope = coreScope;
    }

    /// <summary>
    ///     Gets a value indicating whether this scope has a parent scope.
    /// </summary>
    protected bool HasParentScope => _parentScope is not null;

    /// <summary>
    ///     Handles the scoped notifications when the scope exits.
    /// </summary>
    protected void HandleScopedNotifications() => _notificationPublisher?.ScopeExit(Completed.HasValue && Completed.Value);

    /// <summary>
    ///     Ensures that this scope and all ancestor scopes have not been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the scope has already been disposed.</exception>
    private void EnsureNotDisposed()
    {
        // We can't be disposed
        if (_disposed)
        {
            throw new ObjectDisposedException($"The {nameof(CoreScope)} with ID ({InstanceId}) is already disposed");
        }

        // And neither can our ancestors if we're trying to be disposed since
        // a child must always be disposed before it's parent.
        // This is a safety check, it's actually not entirely possible that a parent can be
        // disposed before the child since that will end up with a "not the Ambient" exception.
        ParentScope?.EnsureNotDisposed();
    }

    /// <summary>
    ///     Resolves the locking mechanism, traversing up the parent chain if necessary.
    /// </summary>
    /// <returns>The locking mechanism for this scope hierarchy.</returns>
    private ILockingMechanism ResolveLockingMechanism() =>
        ParentScope is not null ? ParentScope.ResolveLockingMechanism() : Locks;
}
