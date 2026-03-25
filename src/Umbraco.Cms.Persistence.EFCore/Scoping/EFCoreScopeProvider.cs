using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

/// <summary>
/// Provides functionality to create and manage EF Core scopes.
/// </summary>
/// <typeparam name="TDbContext">The type of DbContext.</typeparam>
internal sealed class EFCoreScopeProvider<TDbContext> : IEFCoreScopeProvider<TDbContext> where TDbContext : DbContext
{
    private readonly IAmbientEFCoreScopeStack<TDbContext> _ambientEFCoreScopeStack;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEFCoreScopeAccessor<TDbContext> _efCoreScopeAccessor;
    private readonly IAmbientScopeContextStack _ambientScopeContextStack;
    private readonly IDistributedLockingMechanismFactory _distributedLockingMechanismFactory;
    private readonly IEventAggregator _eventAggregator;
    private readonly FileSystems _fileSystems;
    private readonly IScopeProvider _scopeProvider;
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreScopeProvider{TDbContext}"/> class.
    /// </summary>
    /// <remarks>Needed for DI as IAmbientEFCoreScopeStack is internal.</remarks>
    public EFCoreScopeProvider()
        : this(
            StaticServiceProvider.Instance.GetRequiredService<IAmbientEFCoreScopeStack<TDbContext>>(),
            StaticServiceProvider.Instance.GetRequiredService<ILoggerFactory>(),
            StaticServiceProvider.Instance.GetRequiredService<IEFCoreScopeAccessor<TDbContext>>(),
            StaticServiceProvider.Instance.GetRequiredService<IAmbientScopeContextStack>(),
            StaticServiceProvider.Instance.GetRequiredService<IDistributedLockingMechanismFactory>(),
            StaticServiceProvider.Instance.GetRequiredService<IEventAggregator>(),
            StaticServiceProvider.Instance.GetRequiredService<FileSystems>(),
            StaticServiceProvider.Instance.GetRequiredService<IScopeProvider>(),
            StaticServiceProvider.Instance.GetRequiredService<IDbContextFactory<TDbContext>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EFCoreScopeProvider{TDbContext}"/> class with explicit dependencies.
    /// </summary>
    /// <param name="ambientEFCoreScopeStack">The ambient scope stack.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="efCoreScopeAccessor">The scope accessor.</param>
    /// <param name="ambientScopeContextStack">The ambient scope context stack.</param>
    /// <param name="distributedLockingMechanismFactory">The distributed locking mechanism factory.</param>
    /// <param name="eventAggregator">The event aggregator.</param>
    /// <param name="fileSystems">The file systems.</param>
    /// <param name="scopeProvider">The scope provider.</param>
    /// <param name="dbContextFactory">The DbContext factory.</param>
    internal EFCoreScopeProvider(
        IAmbientEFCoreScopeStack<TDbContext> ambientEFCoreScopeStack,
        ILoggerFactory loggerFactory,
        IEFCoreScopeAccessor<TDbContext> efCoreScopeAccessor,
        IAmbientScopeContextStack ambientScopeContextStack,
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        IEventAggregator eventAggregator,
        FileSystems fileSystems,
        IScopeProvider scopeProvider,
        IDbContextFactory<TDbContext> dbContextFactory)
    {
        _ambientEFCoreScopeStack = ambientEFCoreScopeStack;
        _loggerFactory = loggerFactory;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _ambientScopeContextStack = ambientScopeContextStack;
        _distributedLockingMechanismFactory = distributedLockingMechanismFactory;
        _eventAggregator = eventAggregator;
        _fileSystems = fileSystems;
        _scopeProvider = scopeProvider;
        _dbContextFactory = dbContextFactory;
        _fileSystems.IsScoped = () => efCoreScopeAccessor.AmbientScope != null && ((EFCoreScope<TDbContext>)efCoreScopeAccessor.AmbientScope).ScopedFileSystems;
    }

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    public IEfCoreScope<TDbContext> CreateDetachedScope(
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null) =>
#pragma warning restore CS0618 // Type or member is obsolete
        new EFCoreDetachableScope<TDbContext>(
            _distributedLockingMechanismFactory,
            _loggerFactory,
            _efCoreScopeAccessor,
            _fileSystems,
            this,
            null,
            _eventAggregator,
            _dbContextFactory,
            repositoryCacheMode,
            scopeFileSystems);

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    public void AttachScope(IEfCoreScope<TDbContext> other)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        // IScopeProvider.AttachScope works with an IEFCoreScope
        // but here we can only deal with our own Scope class
        if (other is not EFCoreDetachableScope<TDbContext> otherScope)
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
        otherScope.OriginalScope = (EFCoreScope<TDbContext>)_ambientEFCoreScopeStack.AmbientScope!;
        otherScope.OriginalContext = AmbientScopeContext;

        PushAmbientScopeContext(otherScope.ScopeContext);
        _ambientEFCoreScopeStack.Push(otherScope);
    }

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    public IEfCoreScope<TDbContext> DetachScope()
#pragma warning restore CS0618 // Type or member is obsolete
    {
        if (_ambientEFCoreScopeStack.AmbientScope is not EFCoreDetachableScope<TDbContext> ambientScope)
        {
            throw new InvalidOperationException("Ambient scope is not detachable");
        }

        if (ambientScope.Detachable == false)
        {
            throw new InvalidOperationException("Ambient scope is not detachable.");
        }

        PopAmbientScope();
        PopAmbientScopeContext();

        var originalScope = (EFCoreScope<TDbContext>)_ambientEFCoreScopeStack.AmbientScope!;
        if (originalScope != ambientScope.OriginalScope)
        {
            throw new InvalidOperationException($"The detached scope ({ambientScope.InstanceId}) does not match the original ({originalScope.InstanceId})");
        }

        IScopeContext? originalScopeContext = AmbientScopeContext;
        if (originalScopeContext != ambientScope.OriginalContext)
        {
            throw new InvalidOperationException($"The detached scope context does not match the original");
        }

        ambientScope.OriginalScope = null;
        ambientScope.OriginalContext = null;
        ambientScope.Attached = false;
        return ambientScope;
    }


    /// <inheritdoc />
    public IScopeContext? AmbientScopeContext => _ambientScopeContextStack.AmbientContext;

    /// <inheritdoc />
#pragma warning disable CS0618 // Type or member is obsolete
    public IEfCoreScope<TDbContext> CreateScope(
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        if (_ambientEFCoreScopeStack.AmbientScope is null)
        {
            ScopeContext? newContext = _ambientScopeContextStack.AmbientContext == null ? new ScopeContext() : null;
            IScope parentScope = _scopeProvider.CreateScope(IsolationLevel.Unspecified, repositoryCacheMode, null, null, scopeFileSystems);
            var ambientScope = new EFCoreScope<TDbContext>(
                parentScope,
                _distributedLockingMechanismFactory,
                _loggerFactory,
                _efCoreScopeAccessor,
                _fileSystems,
                this,
                newContext,
                _eventAggregator,
                _dbContextFactory,
                repositoryCacheMode,
                scopeFileSystems);

            if (newContext != null)
            {
                PushAmbientScopeContext(newContext);
            }

            _ambientEFCoreScopeStack.Push(ambientScope);
            return ambientScope;
        }

        var efCoreScope = new EFCoreScope<TDbContext>(
            (EFCoreScope<TDbContext>)_ambientEFCoreScopeStack.AmbientScope,
            _distributedLockingMechanismFactory,
            _loggerFactory,
            _efCoreScopeAccessor,
            _fileSystems,
            this,
            null,
            _eventAggregator,
            _dbContextFactory,
            repositoryCacheMode,
            scopeFileSystems);

        _ambientEFCoreScopeStack.Push(efCoreScope);
        return efCoreScope;
    }

    /// <summary>
    /// Removes the current ambient scope from the stack.
    /// </summary>
    public void PopAmbientScope() => _ambientEFCoreScopeStack.Pop();

    /// <summary>
    /// Pushes a scope context onto the ambient scope context stack.
    /// </summary>
    /// <param name="scopeContext">The scope context to push.</param>
    /// <exception cref="ArgumentNullException">Thrown when scopeContext is null.</exception>
    public void PushAmbientScopeContext(IScopeContext? scopeContext)
    {
        if (scopeContext is null)
        {
            throw new ArgumentNullException(nameof(scopeContext));
        }
        _ambientScopeContextStack.Push(scopeContext);
    }

    /// <summary>
    /// Removes the current scope context from the ambient scope context stack.
    /// </summary>
    public void PopAmbientScopeContext() => _ambientScopeContextStack.Pop();
}
