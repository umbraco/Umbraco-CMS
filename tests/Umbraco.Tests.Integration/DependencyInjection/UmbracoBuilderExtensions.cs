// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Infrastructure.HostedServices;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Persistence.EFCore.Locking;
using Umbraco.Cms.Persistence.EFCore.Scoping;
using Umbraco.Cms.Tests.Common.TestHelpers.Stubs;
using Umbraco.Cms.Tests.Integration.Implementations;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Persistence.EFCore.DbContext;

namespace Umbraco.Cms.Tests.Integration.DependencyInjection;

/// <summary>
///     This is used to replace certain services that are normally registered from our Core / Infrastructure that
///     we do not want active within integration tests
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Uses/Replaces services with testing services
    /// </summary>
    public static IUmbracoBuilder AddTestServices(this IUmbracoBuilder builder, TestHelper testHelper)
    {
        builder.Services.AddUnique(AppCaches.NoCache);
        builder.Services.AddUnique(Mock.Of<IUmbracoBootPermissionChecker>());
        builder.Services.AddUnique(testHelper.MainDom);

        builder.Services.AddUnique<IIndexRebuilder, TestBackgroundIndexRebuilder>();

#if IS_WINDOWS
        // ensure all lucene indexes are using RAM directory (no file system)
        builder.Services.AddUnique<IDirectoryFactory, LuceneRAMDirectoryFactory>();
#endif

        // replace this service so that it can lookup the correct file locations
        builder.Services.AddUnique(GetLocalizedTextService);

        builder.Services.AddUnique<IServerMessenger, NoopServerMessenger>();
        builder.Services.AddUnique<IProfiler, TestProfiler>();

        builder.Services.AddDbContext<TestUmbracoDbContext>(
            (serviceProvider, options) =>
            {
                var testDatabaseType = builder.Config.GetValue<TestDatabaseSettings.TestDatabaseType>("Tests:Database:DatabaseType");
                if (testDatabaseType is TestDatabaseSettings.TestDatabaseType.Sqlite)
                {
                    options.UseSqlite(serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue.ConnectionString);
                }
                else
                {
                    // If not Sqlite, assume SqlServer
                    options.UseSqlServer(serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue.ConnectionString);
                }
            },
            optionsLifetime: ServiceLifetime.Singleton);

        builder.Services.AddDbContextFactory<TestUmbracoDbContext>(
            (serviceProvider, options) =>
            {
                var testDatabaseType = builder.Config.GetValue<TestDatabaseSettings.TestDatabaseType>("Tests:Database:DatabaseType");
                if (testDatabaseType is TestDatabaseSettings.TestDatabaseType.Sqlite)
                {
                    options.UseSqlite(serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue.ConnectionString);
                }
                else
                {
                    // If not Sqlite, assume SqlServer
                    options.UseSqlServer(serviceProvider.GetRequiredService<IOptionsMonitor<ConnectionStrings>>().CurrentValue.ConnectionString);
                }
            });

        builder.Services.AddUnique<IAmbientEFCoreScopeStack<TestUmbracoDbContext>, AmbientEFCoreScopeStack<TestUmbracoDbContext>>();
        builder.Services.AddUnique<IEFCoreScopeAccessor<TestUmbracoDbContext>, EFCoreScopeAccessor<TestUmbracoDbContext>>();
        builder.Services.AddUnique<IEFCoreScopeProvider<TestUmbracoDbContext>, EFCoreScopeProvider<TestUmbracoDbContext>>();
        builder.Services.AddSingleton<IDistributedLockingMechanism, SqliteEFCoreDistributedLockingMechanism<TestUmbracoDbContext>>();
        builder.Services.AddSingleton<IDistributedLockingMechanism, SqlServerEFCoreDistributedLockingMechanism<TestUmbracoDbContext>>();

        builder.Services.AddSingleton<IReservedFieldNamesService, ReservedFieldNamesService>();

        return builder;
    }

    /// <summary>
    ///     Used to register a replacement for <see cref="ILocalizedTextService" /> where the file sources are the ones within
    ///     the netcore project so
    ///     we don't need to copy files
    /// </summary>
    private static ILocalizedTextService GetLocalizedTextService(IServiceProvider factory)
    {
        var loggerFactory = factory.GetRequiredService<ILoggerFactory>();
        var appCaches = factory.GetRequiredService<AppCaches>();

        var localizedTextService = new LocalizedTextService(
            new Lazy<LocalizedTextServiceFileSources>(() =>
            {
                // get the src folder
                var root = TestContext.CurrentContext.TestDirectory.Split("tests")[0];
                var srcFolder = Path.Combine(root, "src");

                var currFolder = new DirectoryInfo(srcFolder);

                if (!currFolder.Exists)
                {
                    currFolder = new DirectoryInfo(Path.GetTempPath());
                }

                var uiProject = currFolder.GetDirectories("Umbraco.Web.UI", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (uiProject == null)
                {
                    uiProject = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "Umbraco.Web.UI"));
                    uiProject.Create();
                }

                var mainLangFolder = new DirectoryInfo(Path.Combine(uiProject.FullName, Constants.System.DefaultUmbracoPath.TrimStart(Constants.CharArrays.TildeForwardSlash), "config", "lang"));

                return new LocalizedTextServiceFileSources(
                    loggerFactory.CreateLogger<LocalizedTextServiceFileSources>(),
                    appCaches,
                    currFolder,
                    Array.Empty<LocalizedTextServiceSupplementaryFileSource>(),
                    new EmbeddedFileProvider(typeof(IAssemblyProvider).Assembly, "Umbraco.Cms.Core.EmbeddedResources.Lang").GetDirectoryContents(string.Empty));
            }),
            loggerFactory.CreateLogger<LocalizedTextService>());

        return localizedTextService;
    }

    // replace the default so there is no background index rebuilder
    private class TestBackgroundIndexRebuilder : ExamineIndexRebuilder
    {
        public TestBackgroundIndexRebuilder(
            IMainDom mainDom,
            IRuntimeState runtimeState,
            ILogger<ExamineIndexRebuilder> logger,
            IExamineManager examineManager,
            IEnumerable<IIndexPopulator> populators,
            IBackgroundTaskQueue backgroundTaskQueue)
            : base(
            mainDom,
            runtimeState,
            logger,
            examineManager,
            populators,
            backgroundTaskQueue)
        {
        }

        public override void RebuildIndex(string indexName, TimeSpan? delay = null, bool useBackgroundThread = true)
        {
            // noop
        }

        public override void RebuildIndexes(bool onlyEmptyIndexes, TimeSpan? delay = null, bool useBackgroundThread = true)
        {
            // noop
        }
    }

    private class NoopServerMessenger : IServerMessenger
    {
        public void QueueRefresh<TPayload>(ICacheRefresher refresher, TPayload[] payload)
        {
        }

        public void QueueRefresh<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
        {
        }

        public void QueueRefresh<T>(ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances)
        {
        }

        public void QueueRemove<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
        {
        }

        public void QueueRemove(ICacheRefresher refresher, params int[] numericIds)
        {
        }

        public void QueueRefresh(ICacheRefresher refresher, params int[] numericIds)
        {
        }

        public void QueueRefresh(ICacheRefresher refresher, params Guid[] guidIds)
        {
        }

        public void QueueRefreshAll(ICacheRefresher refresher)
        {
        }

        public void Sync() { }

        public void SendMessages() { }
    }
}
