using System.Data;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.EfCore;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Umbraco.Cms.Persistence.EFCore.Scoping;

internal class EfCoreScope : Scope
{
    private readonly IUmbracoEfCoreDatabaseFactory _efCoreDatabaseFactory;
    private IUmbracoEfCoreDatabase? _umbracoEfCoreDatabase;
    private bool? _completed;

    public EfCoreScope(
        ScopeProvider scopeProvider,
        CoreDebugSettings coreDebugSettings,
        MediaFileManager mediaFileManager,
        IEventAggregator eventAggregator,
        ILogger<Scope> logger,
        FileSystems fileSystems,
        bool detachable,
        IScopeContext? scopeContext,
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IsolationLevel isolationLevel = IsolationLevel.Unspecified,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        IEventDispatcher? eventDispatcher = null,
        IScopedNotificationPublisher? scopedNotificationPublisher = null,
        bool? scopeFileSystems = null,
        bool callContext = false,
        bool autoComplete = false)
        : base(
            scopeProvider,
            coreDebugSettings,
            mediaFileManager,
            eventAggregator,
            logger,
            fileSystems,
            detachable,
            scopeContext,
            isolationLevel,
            repositoryCacheMode,
            eventDispatcher,
            scopedNotificationPublisher,
            scopeFileSystems,
            callContext,
            autoComplete)
    {
        _efCoreDatabaseFactory = efCoreDatabaseFactory;
    }

    public EfCoreScope(
        ScopeProvider scopeProvider,
        CoreDebugSettings coreDebugSettings,
        MediaFileManager mediaFileManager,
        IEventAggregator eventAggregator,
        ILogger<Scope> logger,
        FileSystems fileSystems,
        Scope parent,
        IUmbracoEfCoreDatabaseFactory efCoreDatabaseFactory,
        IsolationLevel isolationLevel = IsolationLevel.Unspecified,
        RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
        IEventDispatcher? eventDispatcher = null,
        IScopedNotificationPublisher? notificationPublisher = null,
        bool? scopeFileSystems = null,
        bool callContext = false,
        bool autoComplete = false)
        : base(
            scopeProvider,
            coreDebugSettings,
            mediaFileManager,
            eventAggregator,
            logger,
            fileSystems,
            parent,
            isolationLevel,
            repositoryCacheMode,
            eventDispatcher,
            notificationPublisher,
            scopeFileSystems,
            callContext,
            autoComplete)
    {
        _efCoreDatabaseFactory = efCoreDatabaseFactory;
    }

    public IUmbracoEfCoreDatabase EfCoreDatabase
    {
        get
        {
            EnsureNotDisposed();

            if (_umbracoEfCoreDatabase is not null)
            {
                return _umbracoEfCoreDatabase;
            }

            _umbracoEfCoreDatabase = _efCoreDatabaseFactory.Create();
            _umbracoEfCoreDatabase.DbContext.Database.BeginTransaction();
            return _umbracoEfCoreDatabase;
        }
    }

    public void Dispose()
    {
        DisposeEfCoreDatabase();
        base.Dispose();
    }

    private void DisposeEfCoreDatabase()
    {
        var completed = _completed.HasValue && _completed.Value;
        var databaseException = false;
        if (_umbracoEfCoreDatabase != null)
        {
            try
            {
                if (completed)
                {
                    _umbracoEfCoreDatabase.DbContext.Database.CommitTransaction();
                }
                else
                {
                    _umbracoEfCoreDatabase.DbContext.Database.RollbackTransaction();
                }
            }
            finally
            {
                _umbracoEfCoreDatabase.Dispose();
                _umbracoEfCoreDatabase = null;
            }
        }
    }
}
