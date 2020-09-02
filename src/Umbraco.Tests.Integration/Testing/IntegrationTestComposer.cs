using Moq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Core.WebAssets;
using Umbraco.Examine;
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
            composition.RegisterUnique<BackgroundIndexRebuilder, TestBackgroundIndexRebuilder>();
            composition.RegisterUnique<IRuntimeMinifier>(factory => Mock.Of<IRuntimeMinifier>());

            // we don't want persisted nucache files in tests
            composition.Register(factory => new PublishedSnapshotServiceOptions { IgnoreLocalDb = true });

            // ensure all lucene indexes are using RAM directory (no file system)
            composition.RegisterUnique<ILuceneDirectoryFactory, LuceneRAMDirectoryFactory>();
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
