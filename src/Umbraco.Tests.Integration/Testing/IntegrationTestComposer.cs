﻿using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
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
using Umbraco.Core.WebAssets;
using Umbraco.Examine;
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
        }

        /// <summary>
        /// Used to register a replacement for <see cref="ILocalizedTextService"/> where the file sources are the ones within the netcore project so
        /// we don't need to copy files
        /// </summary>
        /// <returns></returns>
        private ILocalizedTextService GetLocalizedTextService(IFactory factory)
        {
            var globalSettings = factory.GetInstance<IOptions<GlobalSettings>>();
            var logger = factory.GetInstance<ILogger>();
            var appCaches = factory.GetInstance<AppCaches>();

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
                        logger,
                        appCaches,
                        mainLangFolder);

                }),
                logger);

            return localizedTextService;
        }

        // replace the default so there is no background index rebuilder
        private class TestBackgroundIndexRebuilder : BackgroundIndexRebuilder
        {
            public TestBackgroundIndexRebuilder(IMainDom mainDom, IProfilingLogger logger, IApplicationShutdownRegistry hostingEnvironment, IndexRebuilder indexRebuilder)
                : base(mainDom, logger, hostingEnvironment, indexRebuilder)
            {
            }

            public override void RebuildIndexes(bool onlyEmptyIndexes, int waitMilliseconds = 0)
            {
                // noop
            }
        }

    }
}
