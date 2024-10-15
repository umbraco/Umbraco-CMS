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

internal sealed class EFCoreScopeProvider<TDbContext> : IEFCoreScopeProvider<TDbContext> where TDbContext : DbContext
{
    private readonly IAmbientEFCoreScopeStack<TDbContext> _ambientEfCoreScopeStack;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEFCoreScopeAccessor<TDbContext> _efCoreScopeAccessor;
    private readonly IAmbientScopeContextStack _ambientEfCoreScopeContextStack;
    private readonly IDistributedLockingMechanismFactory _distributedLockingMechanismFactory;
    private readonly IEventAggregator _eventAggregator;
    private readonly FileSystems _fileSystems;
    private readonly IScopeProvider _scopeProvider;
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;

    // Needed for DI as IAmbientEfCoreScopeStack is internal
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

    internal EFCoreScopeProvider(
        IAmbientEFCoreScopeStack<TDbContext> ambientEfCoreScopeStack,
        ILoggerFactory loggerFactory,
        IEFCoreScopeAccessor<TDbContext> efCoreScopeAccessor,
        IAmbientScopeContextStack ambientEfCoreScopeContextStack,
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        IEventAggregator eventAggregator,
        FileSystems fileSystems,
        IScopeProvider scopeProvider,
        IDbContextFactory<TDbContext> dbContextFactory)
    {
        _ambientEfCoreScopeStack = ambientEfCoreScopeStack;
        _loggerFactory = loggerFactory;
        _efCoreScopeAccessor = efCoreScopeAccessor;
        _ambientEfCoreScopeContextStack = ambientEfCoreScopeContextStack;
        _distributedLockingMechanismFactory = distributedLockingMechanismFactory;
        _eventAggregator = eventAggregator;
        _fileSystems = fileSystems;
        _scopeProvider = scopeProvider;
        _dbContextFactory = dbContextFactory;
        _fileSystems.IsScoped = () => efCoreScopeAccessor.AmbientScope != null && ((EFCoreScope<TDbContext>)efCoreScopeAccessor.AmbientScope).ScopedFileSystems;
    }

    public IEfCoreScope<TDbContext> CreateDetachedScope(
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        bool? scopeFileSystems = null) =>
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

    public void AttachScope(IEfCoreScope<TDbContext> other)
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
        otherScope.OriginalScope = (EFCoreScope<TDbContext>)_ambientEfCoreScopeStack.AmbientScope!;
        otherScope.OriginalContext = AmbientScopeContext;

        PushAmbientScopeContext(otherScope.ScopeContext);
        _ambientEfCoreScopeStack.Push(otherScope);
    }

    public IEfCoreScope<TDbContext> DetachScope()
    {
        if (_ambientEfCoreScopeStack.AmbientScope is not EFCoreDetachableScope<TDbContext> ambientScope)
        {
            throw new InvalidOperationException("Ambient scope is not detachable");
        }

        if (ambientScope == null)
        {
            throw new InvalidOperationException("There is no ambient scope.");
        }

        if (ambientScope.Detachable == false)
        {
            throw new InvalidOperationException("Ambient scope is not detachable.");
        }

        PopAmbientScope();
        PopAmbientScopeContext();

        var originalScope = (EFCoreScope<TDbContext>)_ambientEfCoreScopeStack.AmbientScope!;
        if (originalScope != ambientScope.OriginalScope)
        {
            throw new InvalidOperationException($"The detatched scope ({ambientScope.InstanceId}) does not match the original ({originalScope.InstanceId})");
        }

        IScopeContext? originalScopeContext = AmbientScopeContext;
        if (originalScopeContext != ambientScope.OriginalContext)
        {
            throw new InvalidOperationException($"The detatched scope context does not match the original");
        }

        ambientScope.OriginalScope = null;
        ambientScope.OriginalContext = null;
        ambientScope.Attached = false;
        return ambientScope;
    }


    public IScopeContext? AmbientScopeContext => _ambientEfCoreScopeContextStack.AmbientContext;

    public IEfCoreScope<TDbContext> CreateScope(
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, bool? scopeFileSystems = null)
    {
        if (_ambientEfCoreScopeStack.AmbientScope is null)
        {
            ScopeContext? newContext = _ambientEfCoreScopeContextStack.AmbientContext == null ? new ScopeContext() : null;
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

            _ambientEfCoreScopeStack.Push(ambientScope);
            return ambientScope;
        }

        var efCoreScope = new EFCoreScope<TDbContext>(
            (EFCoreScope<TDbContext>)_ambientEfCoreScopeStack.AmbientScope,
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

        _ambientEfCoreScopeStack.Push(efCoreScope);
        return efCoreScope;
    }

    public void PopAmbientScope() => _ambientEfCoreScopeStack.Pop();

    public void PushAmbientScopeContext(IScopeContext? scopeContext)
    {
        if (scopeContext is null)
        {
            throw new ArgumentNullException(nameof(scopeContext));
        }
        _ambientEfCoreScopeContextStack.Push(scopeContext);
    }

    public void PopAmbientScopeContext() => _ambientEfCoreScopeContextStack.Pop();
}
