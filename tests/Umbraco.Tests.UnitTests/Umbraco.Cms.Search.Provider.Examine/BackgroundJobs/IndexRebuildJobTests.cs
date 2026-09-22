// Copyright (c) Umbraco.
// See LICENSE for more details.

using Examine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;
using Umbraco.Cms.Search.Core.Services;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;
using Umbraco.Cms.Search.Provider.Examine.BackgroundJobs;
using Umbraco.Cms.Search.Provider.Examine.Services;
using IndexOptions = Umbraco.Cms.Search.Core.Configuration.IndexOptions;
using ISearcher = Umbraco.Cms.Search.Core.Services.ISearcher;
using SearchResult = Umbraco.Cms.Search.Core.Models.Searching.SearchResult;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Search.Provider.Examine.BackgroundJobs;

[TestFixture]
public class IndexRebuildJobTests
{
    private const string IndexAlias = "Umb_Content";
    private const string PhysicalIndexName = "Umb_Content_Physical";
    private const string CurrentOrigin = "current-origin";

    private Mock<IExamineManager> _examineManager;
    private Mock<IActiveIndexManager> _activeIndexManager;
    private Mock<IContentIndexingService> _contentIndexingService;

    [SetUp]
    public void SetUp()
    {
        _examineManager = new Mock<IExamineManager>();
        _activeIndexManager = new Mock<IActiveIndexManager>();
        _activeIndexManager.Setup(x => x.ResolveActiveIndexName(IndexAlias)).Returns(PhysicalIndexName);
        _contentIndexingService = new Mock<IContentIndexingService>();
    }

    [Test]
    public void Job_Is_Delayed_To_Let_The_Server_Finish_Starting_Up()
    {
        IndexRebuildJob sut = CreateSut();

        Assert.AreEqual(TimeSpan.FromMinutes(2), sut.Delay);
    }

    [Test]
    public void Job_Remains_Scheduled_Until_It_Has_Run()
    {
        IndexRebuildJob sut = CreateSut();

        // The job is skipped entirely while the runtime is still installing or upgrading, so it has to stay
        // schedulable until a pass actually happens - otherwise the single attempt is lost.
        Assert.AreNotEqual(Timeout.InfiniteTimeSpan, sut.Period);
        Assert.Greater(sut.Period, TimeSpan.Zero);
    }

    [Test]
    public async Task Job_Retires_Itself_After_Rebuilding()
    {
        IIndex index = CreateIndex(exists: false);
        _examineManager.Setup(x => x.TryGetIndex(PhysicalIndexName, out index)).Returns(true);

        IndexRebuildJob sut = CreateSut();
        Assert.AreNotEqual(Timeout.InfiniteTimeSpan, sut.Period);

        await sut.RunJobAsync(CancellationToken.None);

        Assert.AreEqual(Timeout.InfiniteTimeSpan, sut.Period);
    }

    [Test]
    public async Task Job_Only_Rebuilds_On_The_First_Run()
    {
        IIndex index = CreateIndex(exists: false);
        _examineManager.Setup(x => x.TryGetIndex(PhysicalIndexName, out index)).Returns(true);

        IndexRebuildJob sut = CreateSut();

        // Changing the period does not take effect until the wait after the current one, so the scheduler can call
        // back before the job has retired. A second pass must not re-queue rebuilds that are still in flight.
        await sut.RunJobAsync(CancellationToken.None);
        await sut.RunJobAsync(CancellationToken.None);

        _contentIndexingService.Verify(x => x.Rebuild(IndexAlias, CurrentOrigin), Times.Once);
    }

    [Test]
    public async Task Job_Retires_Itself_When_No_Index_Needs_Rebuilding()
    {
        IIndex index = CreateIndex(exists: true);
        _examineManager.Setup(x => x.TryGetIndex(PhysicalIndexName, out index)).Returns(true);

        IndexRebuildJob sut = CreateSut();
        Assert.AreNotEqual(Timeout.InfiniteTimeSpan, sut.Period);

        await sut.RunJobAsync(CancellationToken.None);

        Assert.AreEqual(Timeout.InfiniteTimeSpan, sut.Period);
    }

    [Test]
    public void Job_Runs_On_Every_Server_Role()
    {
        IndexRebuildJob sut = CreateSut();

        CollectionAssert.AreEquivalent(Enum.GetValues<ServerRole>(), sut.ServerRoles);
    }

    [Test]
    public async Task Does_Not_Rebuild_When_Active_Physical_Index_Already_Exists()
    {
        IIndex index = CreateIndex(exists: true);
        _examineManager.Setup(x => x.TryGetIndex(PhysicalIndexName, out index)).Returns(true);

        IndexRebuildJob sut = CreateSut();
        await sut.RunJobAsync(CancellationToken.None);

        _contentIndexingService.Verify(x => x.Rebuild(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task Rebuilds_When_Active_Physical_Index_Does_Not_Exist()
    {
        IIndex index = CreateIndex(exists: false);
        _examineManager.Setup(x => x.TryGetIndex(PhysicalIndexName, out index)).Returns(true);

        IndexRebuildJob sut = CreateSut();
        await sut.RunJobAsync(CancellationToken.None);

        _contentIndexingService.Verify(x => x.Rebuild(IndexAlias, CurrentOrigin), Times.Once);
    }

    [Test]
    public async Task Does_Not_Rebuild_When_Not_A_Registered_Examine_Index()
    {
        IIndex? index = null;
        _examineManager.Setup(x => x.TryGetIndex(PhysicalIndexName, out index)).Returns(false);

        IndexRebuildJob sut = CreateSut();
        await sut.RunJobAsync(CancellationToken.None);

        _contentIndexingService.Verify(x => x.Rebuild(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    private static IIndex CreateIndex(bool exists)
    {
        var index = new Mock<IIndex>();
        index.Setup(x => x.IndexExists()).Returns(exists);
        return index.Object;
    }

    private IndexRebuildJob CreateSut()
    {
        var indexOptions = new IndexOptions();
        indexOptions.RegisterContentIndex<NoopIndexer, NoopSearcher, NoopContentChangeStrategy>(
            IndexAlias,
            UmbracoObjectTypes.Document);

        var originProvider = new Mock<IOriginProvider>();
        originProvider.Setup(x => x.GetCurrent()).Returns(CurrentOrigin);

        return new IndexRebuildJob(
            _examineManager.Object,
            _activeIndexManager.Object,
            _contentIndexingService.Object,
            Options.Create(indexOptions),
            Mock.Of<ILogger<IndexRebuildJob>>(),
            originProvider.Object);
    }

    private sealed class NoopIndexer : IIndexer
    {
        public Task AddOrUpdateAsync(string indexAlias, Guid id, UmbracoObjectTypes objectType, IEnumerable<Variation> variations, IEnumerable<IndexField> fields, ContentProtection? protection)
            => Task.CompletedTask;

        public Task DeleteAsync(string indexAlias, IEnumerable<Guid> ids) => Task.CompletedTask;

        public Task ResetAsync(string indexAlias) => Task.CompletedTask;

        public Task<IndexMetadata> GetMetadataAsync(string indexAlias) => throw new NotImplementedException();
    }

    private sealed class NoopSearcher : ISearcher
    {
        public Task<SearchResult> SearchAsync(
            string indexAlias,
            string? query = null,
            IEnumerable<Filter>? filters = null,
            IEnumerable<Facet>? facets = null,
            IEnumerable<Sorter>? sorters = null,
            string? culture = null,
            string? segment = null,
            AccessContext? accessContext = null,
            int skip = 0,
            int take = 10,
            int maxSuggestions = 0)
            => throw new NotImplementedException();
    }

    private sealed class NoopContentChangeStrategy : IContentChangeStrategy
    {
        public Task HandleAsync(IEnumerable<ContentIndexInfo> indexInfos, IEnumerable<ContentChange> changes, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task RebuildAsync(ContentIndexInfo indexInfo, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
