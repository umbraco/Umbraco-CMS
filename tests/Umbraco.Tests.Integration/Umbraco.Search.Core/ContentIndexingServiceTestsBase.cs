using Umbraco.Cms.Core.HostedServices;
using NUnit.Framework;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Search.Core.DependencyInjection;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Testing.Search;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Core;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
public abstract class ContentIndexingServiceTestsBase : UmbracoIntegrationTest
{
    protected TestContentChangeStrategy Strategy { get; } = new();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        base.CustomTestSetup(builder);

        builder.AddSearchCore();

        builder.Services.AddUnique<IBackgroundTaskQueue, ImmediateBackgroundTaskQueue>();
        builder.Services.AddUnique<IServerMessenger, LocalServerMessenger>();
        builder.Services.AddUnique<IServerEventRouter, NoOpServerEventRouter>();
    }

    protected class TestContentChangeStrategy : IPublishedContentChangeStrategy, IDraftContentChangeStrategy
    {
        public Task HandleAsync(IEnumerable<ContentIndexInfo> indexInfos, IEnumerable<ContentChange> changes, CancellationToken cancellationToken)
        {
            HandledIndexInfos.Add(indexInfos.ToList());
            return Task.CompletedTask;
        }

        public Task RebuildAsync(ContentIndexInfo indexInfo, CancellationToken cancellationToken)
        {
            HandledIndexInfos.Add([indexInfo]);
            return Task.CompletedTask;
        }

        public List<List<ContentIndexInfo>> HandledIndexInfos { get; } = new();
    }
}
