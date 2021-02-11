// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.WebAssets;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.TestHelpers.Stubs;
using Umbraco.Cms.Tests.Integration.Implementations;
using Umbraco.Core.Services.Implement;
using Umbraco.Examine;
using Umbraco.Extensions;
using Umbraco.Infrastructure.HostedServices;
using Umbraco.Web.Search;

namespace Umbraco.Cms.Tests.Integration.DependencyInjection
{
    /// <summary>
    /// This is used to replace certain services that are normally registered from our Core / Infrastructure that
    /// we do not want active within integration tests
    /// </summary>
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        /// Uses/Replaces services with testing services
        /// </summary>
        public static IUmbracoBuilder AddTestServices(this IUmbracoBuilder builder, TestHelper testHelper, AppCaches appCaches = null)
        {
            builder.Services.AddUnique(appCaches ?? AppCaches.NoCache);
            builder.Services.AddUnique(Mock.Of<IUmbracoBootPermissionChecker>());
            builder.Services.AddUnique(testHelper.MainDom);

            builder.Services.AddUnique<BackgroundIndexRebuilder, TestBackgroundIndexRebuilder>();
            builder.Services.AddUnique(factory => Mock.Of<IRuntimeMinifier>());

            // we don't want persisted nucache files in tests
            builder.Services.AddTransient(factory => new PublishedSnapshotServiceOptions { IgnoreLocalDb = true });

#if IS_WINDOWS
            // ensure all lucene indexes are using RAM directory (no file system)
            builder.Services.AddUnique<ILuceneDirectoryFactory, LuceneRAMDirectoryFactory>();
#endif

            // replace this service so that it can lookup the correct file locations
            builder.Services.AddUnique(GetLocalizedTextService);

            builder.Services.AddUnique<IServerMessenger, NoopServerMessenger>();
            builder.Services.AddUnique<IProfiler, TestProfiler>();

            return builder;
        }

        /// <summary>
        /// Used to register a replacement for <see cref="ILocalizedTextService"/> where the file sources are the ones within the netcore project so
        /// we don't need to copy files
        /// </summary>
        private static ILocalizedTextService GetLocalizedTextService(IServiceProvider factory)
        {
            IOptions<GlobalSettings> globalSettings = factory.GetRequiredService<IOptions<GlobalSettings>>();
            ILoggerFactory loggerFactory = factory.GetRequiredService<ILoggerFactory>();
            AppCaches appCaches = factory.GetRequiredService<AppCaches>();

            var localizedTextService = new LocalizedTextService(
                new Lazy<LocalizedTextServiceFileSources>(() =>
                {
                    // get the src folder
                    var currFolder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
                    while (!currFolder.Name.Equals("src", StringComparison.InvariantCultureIgnoreCase))
                    {
                        currFolder = currFolder.Parent;
                    }

                    DirectoryInfo netcoreUI = currFolder.GetDirectories("Umbraco.Web.UI.NetCore", SearchOption.TopDirectoryOnly).First();
                    var mainLangFolder = new DirectoryInfo(Path.Combine(netcoreUI.FullName, globalSettings.Value.UmbracoPath.TrimStart("~/"), "config", "lang"));

                    return new LocalizedTextServiceFileSources(
                        loggerFactory.CreateLogger<LocalizedTextServiceFileSources>(),
                        appCaches,
                        mainLangFolder);
                }),
                loggerFactory.CreateLogger<LocalizedTextService>());

            return localizedTextService;
        }

        // replace the default so there is no background index rebuilder
        private class TestBackgroundIndexRebuilder : BackgroundIndexRebuilder
        {
            public TestBackgroundIndexRebuilder(
                IMainDom mainDom,
                ILogger<BackgroundIndexRebuilder> logger,
                IndexRebuilder indexRebuilder,
                IBackgroundTaskQueue backgroundTaskQueue)
                : base(mainDom, logger, indexRebuilder, backgroundTaskQueue)
            {
            }

            public override void RebuildIndexes(bool onlyEmptyIndexes, TimeSpan? delay = null)
            {
                // noop
            }
        }

        private class NoopServerMessenger : IServerMessenger
        {
            public NoopServerMessenger()
            {
            }

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
}
