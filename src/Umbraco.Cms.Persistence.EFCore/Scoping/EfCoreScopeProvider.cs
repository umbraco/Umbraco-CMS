using System.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.EfCore;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScopeProvider : ScopeProvider
{
    private readonly CoreDebugSettings _coreDebugSettings;
    private readonly MediaFileManager _mediaFileManager;
    private readonly IEventAggregator _eventAggregator;
    private readonly ILoggerFactory _loggerFactory;
    private readonly FileSystems _fileSystems;
    private readonly IUmbracoEfCoreDatabaseFactory _umbracoEfCoreDatabaseFactory;

    public EfCoreScopeProvider(
        IAmbientScopeStack ambientScopeStack,
        IAmbientScopeContextStack ambientContextStack,
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        IUmbracoDatabaseFactory databaseFactory,
        FileSystems fileSystems,
        IOptionsMonitor<CoreDebugSettings> coreDebugSettings,
        MediaFileManager mediaFileManager,
        ILoggerFactory loggerFactory,
        IEventAggregator eventAggregator,
        IUmbracoEfCoreDatabaseFactory umbracoEfCoreDatabaseFactory)
        : base(
        ambientScopeStack,
        ambientContextStack,
        distributedLockingMechanismFactory,
        databaseFactory,
        fileSystems,
        coreDebugSettings,
        mediaFileManager,
        loggerFactory,
        eventAggregator)
    {
        _fileSystems = fileSystems;
        _mediaFileManager = mediaFileManager;
        _loggerFactory = loggerFactory;
        _eventAggregator = eventAggregator;
        _umbracoEfCoreDatabaseFactory = umbracoEfCoreDatabaseFactory;
        _coreDebugSettings = coreDebugSettings.CurrentValue;
    }

    public EfCoreScopeProvider(
        IDistributedLockingMechanismFactory distributedLockingMechanismFactory,
        IUmbracoDatabaseFactory databaseFactory,
        FileSystems fileSystems,
        IOptionsMonitor<CoreDebugSettings> coreDebugSettings,
        MediaFileManager mediaFileManager,
        ILoggerFactory loggerFactory,
        IRequestCache requestCache,
        IEventAggregator eventAggregator, IUmbracoEfCoreDatabaseFactory umbracoEfCoreDatabaseFactory)
        : base(
        distributedLockingMechanismFactory,
        databaseFactory,
        fileSystems,
        coreDebugSettings,
        mediaFileManager,
        loggerFactory,
        requestCache,
        eventAggregator)
    {
        _fileSystems = fileSystems;
        _mediaFileManager = mediaFileManager;
        _loggerFactory = loggerFactory;
        _eventAggregator = eventAggregator;
        _umbracoEfCoreDatabaseFactory = umbracoEfCoreDatabaseFactory;
        _coreDebugSettings = coreDebugSettings.CurrentValue;
    }

    public IScope CreateScope(
        IsolationLevel isolationLevel = IsolationLevel.Unspecified,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        IEventDispatcher? eventDispatcher = null,
        IScopedNotificationPublisher? notificationPublisher = null,
        bool? scopeFileSystems = null,
        bool callContext = false,
        bool autoComplete = false)
    {
        Scope? ambientScope = AmbientScope;
        if (ambientScope == null)
        {
            IScopeContext? ambientContext = AmbientContext;
            ScopeContext? newContext = ambientContext == null ? new ScopeContext() : null;
            var scope = new EfCoreScope(
                this,
                _coreDebugSettings,
                _mediaFileManager,
                _eventAggregator,
                _loggerFactory.CreateLogger<Scope>(),
                _fileSystems,
                false,
                newContext,
                _umbracoEfCoreDatabaseFactory,
                isolationLevel,
                repositoryCacheMode,
                eventDispatcher,
                notificationPublisher,
                scopeFileSystems,
                callContext,
                autoComplete);

            // assign only if scope creation did not throw!
            PushAmbientScope(scope);
            if (newContext != null)
            {
                PushAmbientScopeContext(newContext);
            }
            return scope;
        }

        var nested = new EfCoreScope(
            this,
            _coreDebugSettings,
            _mediaFileManager,
            _eventAggregator,
            _loggerFactory.CreateLogger<Scope>(),
            _fileSystems,
            ambientScope,
            _umbracoEfCoreDatabaseFactory,
            isolationLevel,
            repositoryCacheMode,
            eventDispatcher,
            notificationPublisher,
            scopeFileSystems,
            callContext,
            autoComplete);
        PushAmbientScope(nested);
        return nested;
    }
}
