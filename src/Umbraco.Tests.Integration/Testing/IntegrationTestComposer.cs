using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Core.Sync;
using Umbraco.Core.WebAssets;
using Umbraco.Examine;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web.Compose;
using Umbraco.Web.PublishedCache.NuCache;
using Umbraco.Web.Scheduling;
using Umbraco.Web.Search;

namespace Umbraco.Tests.Integration.Testing
{
    /// <summary>
    /// This is used to replace certain services that are normally registered from our Core / Infrastructure that
    /// we do not want active within integration tests
    /// </summary>
    /// <remarks>
    /// This is a IUserComposer so that it runs after all core composers
    /// </remarks>
    [RuntimeLevel(MinLevel = RuntimeLevel.Boot)]
    public class IntegrationTestComposer : ComponentComposer<IntegrationTestComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Components().Remove<SchedulerComponent>();
            composition.Components().Remove<DatabaseServerRegistrarAndMessengerComponent>();
            composition.RegisterUnique<BackgroundIndexRebuilder, TestBackgroundIndexRebuilder>();
            composition.RegisterUnique<IRuntimeMinifier>(factory => Mock.Of<IRuntimeMinifier>());

            // we don't want persisted nucache files in tests
            composition.Register(factory => new PublishedSnapshotServiceOptions { IgnoreLocalDb = true });

            // ensure all lucene indexes are using RAM directory (no file system)
            composition.RegisterUnique<ILuceneDirectoryFactory, LuceneRAMDirectoryFactory>();

            // replace this service so that it can lookup the correct file locations
            composition.RegisterUnique<ILocalizedTextService>(GetLocalizedTextService);

            composition.RegisterUnique<IServerMessenger, NoopServerMessenger>();
            composition.RegisterUnique<IProfiler, TestProfiler>();


        }

        /// <summary>
        /// Used to register a replacement for <see cref="ILocalizedTextService"/> where the file sources are the ones within the netcore project so
        /// we don't need to copy files
        /// </summary>
        /// <returns></returns>
        private ILocalizedTextService GetLocalizedTextService(IServiceProvider factory)
        {
            var globalSettings = factory.GetRequiredService<IOptions<GlobalSettings>>();
            var loggerFactory = factory.GetRequiredService<ILoggerFactory>();
            var appCaches = factory.GetRequiredService<AppCaches>();

            var localizedTextService = new LocalizedTextService(
                new Lazy<LocalizedTextServiceFileSources>(() =>
                {
                    // get the src folder
                    var currFolder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
                    while(!currFolder.Name.Equals("src", StringComparison.InvariantCultureIgnoreCase))
                    {
                        currFolder = currFolder.Parent;
                    }
                    var netcoreUI = currFolder.GetDirectories("Umbraco.Web.UI.NetCore", SearchOption.TopDirectoryOnly).First();
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
            public TestBackgroundIndexRebuilder(IMainDom mainDom, IProfilingLogger profilingLogger , ILoggerFactory loggerFactory, IApplicationShutdownRegistry hostingEnvironment, IndexRebuilder indexRebuilder)
                : base(mainDom, profilingLogger , loggerFactory, hostingEnvironment, indexRebuilder)
            {
            }

            public override void RebuildIndexes(bool onlyEmptyIndexes, int waitMilliseconds = 0)
            {
                // noop
            }
        }

        private class NoopServerMessenger : IServerMessenger
        {
            public NoopServerMessenger()
            { }

            public void PerformRefresh<TPayload>(ICacheRefresher refresher, TPayload[] payload)
            {

            }

            public void PerformRefresh<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
            {

            }

            public void PerformRefresh<T>(ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances)
            {

            }

            public void PerformRemove<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances)
            {

            }

            public void PerformRemove(ICacheRefresher refresher, params int[] numericIds)
            {

            }

            public void PerformRefresh(ICacheRefresher refresher, params int[] numericIds)
            {

            }

            public void PerformRefresh(ICacheRefresher refresher, params Guid[] guidIds)
            {

            }

            public void PerformRefreshAll(ICacheRefresher refresher)
            {

            }
        }

    }
}
