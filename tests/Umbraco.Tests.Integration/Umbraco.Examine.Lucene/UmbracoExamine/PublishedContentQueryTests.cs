using System.Collections;
using System.Globalization;
using Examine;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Examine.Search;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Testing;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Examine.Lucene.UmbracoExamine;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
internal sealed class PublishedContentQueryTests : ExamineBaseTest
{
    private class TestIndex : LuceneIndex, IUmbracoIndex
    {
        private readonly string[] _fieldNames;

        public TestIndex(ILoggerFactory loggerFactory, string name, Directory luceneDirectory, string[] fieldNames)
            : base(
                loggerFactory,
                name,
                IndexInitializer.GetOptions(
                    name,
                    new LuceneDirectoryIndexOptions
                    {
                        DirectoryFactory = new GenericDirectoryFactory(s => luceneDirectory)
                    })) =>
            _fieldNames = fieldNames;

        public bool EnableDefaultEventHandler => throw new NotImplementedException();
        public bool PublishedValuesOnly => throw new NotImplementedException();
        public bool SupportProtectedContent => throw new NotImplementedException();
        public IEnumerable<string> GetFields() => _fieldNames;
    }

    private TestIndex CreateTestIndex(Directory luceneDirectory, (string Name, string Culture)[] fields)
    {
        var index = new TestIndex(LoggerFactory, "TestIndex", luceneDirectory, fields.Select(f => f.Name).ToArray());

        using (index.WithThreadingMode(IndexThreadingMode.Synchronous))
        {
            // populate with some test data
            for (var i = 0; i < fields.Length; i++)
            {
                var (name, culture) = fields[i];

                index.IndexItem(new ValueSet(
                    $"{i + 1}",
                    "content",
                    new Dictionary<string, object>
                    {
                        [name] = "Hello world, there are products here",
                        [UmbracoExamineFieldNames.VariesByCultureFieldName] = string.IsNullOrEmpty(culture) ? "n" : "y",
                        [string.IsNullOrEmpty(culture) ? UmbracoExamineFieldNames.PublishedFieldName : $"{UmbracoExamineFieldNames.PublishedFieldName}_{culture}"] = "y"
                    }));
            }
        }

        return index;
    }

    private PublishedContentQuery CreatePublishedContentQuery(
        IIndex? indexer = null,
        string indexName = "TestIndex",
        IExamineManager? examineManager = null,
        IPublishedContentCache? contentCache = null,
        IPublishedMediaCache? mediaCache = null,
        IVariationContextAccessor? variationContextAccessor = null,
        IDocumentNavigationQueryService? documentNavigationQueryService = null,
        IMediaNavigationQueryService? mediaNavigationQueryService = null)
    {
        if (examineManager is null)
        {
            var examineManagerMock = new Mock<IExamineManager>();
            if (indexer is not null)
            {
                var outIndex = indexer;
                examineManagerMock.Setup(x => x.TryGetIndex(indexName, out outIndex)).Returns(true);
            }

            examineManager = examineManagerMock.Object;
        }

        if (contentCache is null)
        {
            var contentCacheMock = new Mock<IPublishedContentCache>();
            contentCacheMock.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns((int intId) => Mock.Of<IPublishedContent>(x => x.Id == intId));
            contentCache = contentCacheMock.Object;
        }

        if (mediaCache is null)
        {
            mediaCache = Mock.Of<IPublishedMediaCache>();
        }

        if (variationContextAccessor is null)
        {
            var variationContextAccessorMock = new Mock<IVariationContextAccessor>();
            variationContextAccessorMock.SetupProperty(x => x.VariationContext, new VariationContext());
            variationContextAccessor = variationContextAccessorMock.Object;
        }

        documentNavigationQueryService ??= Mock.Of<IDocumentNavigationQueryService>();
        mediaNavigationQueryService ??= Mock.Of<IMediaNavigationQueryService>();

        return new PublishedContentQuery(
            variationContextAccessor,
            examineManager,
            contentCache,
            mediaCache,
            documentNavigationQueryService,
            mediaNavigationQueryService);
    }

    private static Mock<IPublishedContentCache> CreateContentCache(
        IReadOnlyDictionary<int, IPublishedContent>? contentByInt = null,
        IReadOnlyDictionary<Guid, IPublishedContent>? contentByGuid = null)
    {
        contentByInt ??= new Dictionary<int, IPublishedContent>();
        contentByGuid ??= new Dictionary<Guid, IPublishedContent>();

        var contentCache = new Mock<IPublishedContentCache>(MockBehavior.Strict);
        contentCache.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int id) => contentByInt.TryGetValue(id, out IPublishedContent? content) ? content : null);
        contentCache.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid key) => contentByGuid.TryGetValue(key, out IPublishedContent? content) ? content : null);
        contentCache.Setup(x => x.GetById(false, It.IsAny<Guid>()))
            .Returns((bool _, Guid key) => contentByGuid.TryGetValue(key, out IPublishedContent? content) ? content : null);
        return contentCache;
    }

    private static Mock<IPublishedMediaCache> CreateMediaCache(
        IReadOnlyDictionary<int, IPublishedContent>? mediaByInt = null,
        IReadOnlyDictionary<Guid, IPublishedContent>? mediaByGuid = null)
    {
        mediaByInt ??= new Dictionary<int, IPublishedContent>();
        mediaByGuid ??= new Dictionary<Guid, IPublishedContent>();

        var mediaCache = new Mock<IPublishedMediaCache>(MockBehavior.Strict);
        mediaCache.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int id) => mediaByInt.TryGetValue(id, out IPublishedContent? content) ? content : null);
        mediaCache.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid key) => mediaByGuid.TryGetValue(key, out IPublishedContent? content) ? content : null);
        mediaCache.Setup(x => x.GetById(false, It.IsAny<Guid>()))
            .Returns((bool _, Guid key) => mediaByGuid.TryGetValue(key, out IPublishedContent? content) ? content : null);
        return mediaCache;
    }

    private static ISearchResults CreateSearchResults(long totalItemCount, params string[] ids)
    {
        var searchResults = ids.Select(CreateSearchResult).ToArray();
        return new TestSearchResults(searchResults, totalItemCount);
    }

    private static ISearchResult CreateSearchResult(string id)
    {
        var searchResult = new Mock<ISearchResult>(MockBehavior.Strict);
        searchResult.SetupGet(x => x.Id).Returns(id);
        searchResult.SetupGet(x => x.Score).Returns(1f);
        return searchResult.Object;
    }

    [Test]
    public void Constructor_WithNullVariationContextAccessor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PublishedContentQuery(
            null!,
            Mock.Of<IExamineManager>(),
            Mock.Of<IPublishedContentCache>(),
            Mock.Of<IPublishedMediaCache>(),
            Mock.Of<IDocumentNavigationQueryService>(),
            Mock.Of<IMediaNavigationQueryService>()));
    }

    [Test]
    public void Constructor_WithNullExamineManager_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new PublishedContentQuery(
            Mock.Of<IVariationContextAccessor>(),
            null!,
            Mock.Of<IPublishedContentCache>(),
            Mock.Of<IPublishedMediaCache>(),
            Mock.Of<IDocumentNavigationQueryService>(),
            Mock.Of<IMediaNavigationQueryService>()));
    }

    [Test]
    public void Content_Overloads_ReturnExpectedContent()
    {
        var contentId = 123;
        var contentKey = Guid.NewGuid();
        IPublishedContent content = Mock.Of<IPublishedContent>(x => x.Id == contentId);

        var contentCache = CreateContentCache(
            new Dictionary<int, IPublishedContent> { [contentId] = content },
            new Dictionary<Guid, IPublishedContent> { [contentKey] = content });

        var query = CreatePublishedContentQuery(contentCache: contentCache.Object);
        var contentUdi = new GuidUdi(Constants.UdiEntityType.Document, contentKey);

        Assert.AreSame(content, query.Content(contentId));
        Assert.AreSame(content, query.Content(contentKey));
        Assert.AreSame(content, query.Content(contentUdi));
        Assert.AreSame(content, query.Content((object)contentId));
        Assert.AreSame(content, query.Content((object)contentId.ToString(CultureInfo.InvariantCulture)));
        Assert.AreSame(content, query.Content((object)contentKey));
        Assert.AreSame(content, query.Content((object)contentKey.ToString()));
        Assert.AreSame(content, query.Content((object)contentUdi));
        Assert.AreSame(content, query.Content((object)contentUdi.ToString()));

        Assert.IsNull(query.Content((Udi?)null));
        Assert.IsNull(query.Content(new StringUdi(Constants.UdiEntityType.Member, "member-a")));
        Assert.IsNull(query.Content((object)"umb://member/member-a"));
        Assert.IsNull(query.Content((object)new object()));
    }

    [Test]
    public void Content_CollectionOverloads_FilterMissingValues()
    {
        var contentId = 33;
        var contentKey = Guid.NewGuid();
        IPublishedContent content = Mock.Of<IPublishedContent>(x => x.Id == contentId);
        var contentUdi = new GuidUdi(Constants.UdiEntityType.Document, contentKey);

        var contentCache = CreateContentCache(
            new Dictionary<int, IPublishedContent> { [contentId] = content },
            new Dictionary<Guid, IPublishedContent> { [contentKey] = content });

        var query = CreatePublishedContentQuery(contentCache: contentCache.Object);

        CollectionAssert.AreEqual(new[] { contentId }, query.Content(new[] { contentId, 99 }).Select(x => x.Id).ToArray());
        CollectionAssert.AreEqual(new[] { contentId }, query.Content(new[] { contentKey, Guid.NewGuid() }).Select(x => x.Id).ToArray());

        var objectResults = query.Content(
            new object[] { contentId, contentKey.ToString(), contentUdi.ToString(), "not-a-guid", new object() }).ToArray();
        Assert.AreEqual(3, objectResults.Length);
        Assert.That(objectResults.All(x => x.Id == contentId));
    }

    [Test]
    public void ContentAtRoot_ReturnsItemsFromRootKeys()
    {
        var firstKey = Guid.NewGuid();
        var secondKey = Guid.NewGuid();
        var missingKey = Guid.NewGuid();
        IEnumerable<Guid> rootKeys = new[] { firstKey, secondKey, missingKey };

        IPublishedContent first = Mock.Of<IPublishedContent>(x => x.Id == 1);
        IPublishedContent second = Mock.Of<IPublishedContent>(x => x.Id == 2);

        var contentCache = CreateContentCache(
            contentByGuid: new Dictionary<Guid, IPublishedContent>
            {
                [firstKey] = first,
                [secondKey] = second
            });

        var navigationQueryService = new Mock<IDocumentNavigationQueryService>(MockBehavior.Strict);
        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeys)).Returns(true);

        var query = CreatePublishedContentQuery(
            contentCache: contentCache.Object,
            documentNavigationQueryService: navigationQueryService.Object);

        CollectionAssert.AreEqual(new[] { 1, 2 }, query.ContentAtRoot().Select(x => x.Id).ToArray());
    }

    [Test]
    public void ContentAtRoot_ReturnsEmptyWhenRootKeysAreUnavailable()
    {
        IEnumerable<Guid> rootKeys = Array.Empty<Guid>();
        var navigationQueryService = new Mock<IDocumentNavigationQueryService>(MockBehavior.Strict);
        navigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeys)).Returns(false);

        var query = CreatePublishedContentQuery(
            contentCache: new Mock<IPublishedContentCache>(MockBehavior.Strict).Object,
            documentNavigationQueryService: navigationQueryService.Object);

        Assert.IsEmpty(query.ContentAtRoot());
    }

    [Test]
    public void Media_Overloads_ReturnExpectedMedia()
    {
        var mediaId = 543;
        var mediaKey = Guid.NewGuid();
        IPublishedContent media = Mock.Of<IPublishedContent>(x => x.Id == mediaId);
        var mediaUdi = new GuidUdi(Constants.UdiEntityType.Media, mediaKey);

        var mediaCache = CreateMediaCache(
            new Dictionary<int, IPublishedContent> { [mediaId] = media },
            new Dictionary<Guid, IPublishedContent> { [mediaKey] = media });

        var query = CreatePublishedContentQuery(mediaCache: mediaCache.Object);

        Assert.AreSame(media, query.Media(mediaId));
        Assert.AreSame(media, query.Media(mediaKey));
        Assert.AreSame(media, query.Media(mediaUdi));
        Assert.AreSame(media, query.Media((object)mediaId));
        Assert.AreSame(media, query.Media((object)mediaId.ToString(CultureInfo.InvariantCulture)));
        Assert.AreSame(media, query.Media((object)mediaKey));
        Assert.AreSame(media, query.Media((object)mediaKey.ToString()));
        Assert.AreSame(media, query.Media((object)mediaUdi));
        Assert.AreSame(media, query.Media((object)mediaUdi.ToString()));

        Assert.IsNull(query.Media((Udi?)null));
        Assert.IsNull(query.Media(new StringUdi(Constants.UdiEntityType.Member, "member-a")));
        Assert.IsNull(query.Media((object)"umb://member/member-a"));
        Assert.IsNull(query.Media((object)new object()));
    }

    [Test]
    public void Media_CollectionOverloads_FilterMissingValues()
    {
        var mediaId = 3;
        var mediaKey = Guid.NewGuid();
        IPublishedContent media = Mock.Of<IPublishedContent>(x => x.Id == mediaId);
        var mediaUdi = new GuidUdi(Constants.UdiEntityType.Media, mediaKey);

        var mediaCache = CreateMediaCache(
            new Dictionary<int, IPublishedContent> { [mediaId] = media },
            new Dictionary<Guid, IPublishedContent> { [mediaKey] = media });

        var query = CreatePublishedContentQuery(mediaCache: mediaCache.Object);

        CollectionAssert.AreEqual(new[] { mediaId }, query.Media(new[] { mediaId, 99 }).Select(x => x.Id).ToArray());
        CollectionAssert.AreEqual(new[] { mediaId }, query.Media(new[] { mediaKey, Guid.NewGuid() }).Select(x => x.Id).ToArray());

        var objectResults = query.Media(
            new object[] { mediaId, mediaKey.ToString(), mediaUdi.ToString(), "not-a-guid", new object() }).ToArray();
        Assert.AreEqual(3, objectResults.Length);
        Assert.That(objectResults.All(x => x.Id == mediaId));
    }

    [Test]
    public void MediaAtRoot_UsesMediaCacheForRootKeys()
    {
        var mediaKey = Guid.NewGuid();
        IEnumerable<Guid> rootKeys = new[] { mediaKey };
        IPublishedContent media = Mock.Of<IPublishedContent>(x => x.Id == 7);

        var contentCache = new Mock<IPublishedContentCache>(MockBehavior.Strict);
        var mediaCache = CreateMediaCache(mediaByGuid: new Dictionary<Guid, IPublishedContent> { [mediaKey] = media });

        var mediaNavigationQueryService = new Mock<IMediaNavigationQueryService>(MockBehavior.Strict);
        mediaNavigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeys)).Returns(true);

        var query = CreatePublishedContentQuery(
            contentCache: contentCache.Object,
            mediaCache: mediaCache.Object,
            mediaNavigationQueryService: mediaNavigationQueryService.Object);

        CollectionAssert.AreEqual(new[] { 7 }, query.MediaAtRoot().Select(x => x.Id).ToArray());
        mediaCache.Verify(x => x.GetById(false, mediaKey), Times.Once);
        contentCache.VerifyNoOtherCalls();
    }

    [Test]
    public void MediaAtRoot_ReturnsEmptyWhenRootKeysAreUnavailable()
    {
        IEnumerable<Guid> rootKeys = Array.Empty<Guid>();
        var mediaNavigationQueryService = new Mock<IMediaNavigationQueryService>(MockBehavior.Strict);
        mediaNavigationQueryService.Setup(x => x.TryGetRootKeys(out rootKeys)).Returns(false);

        var query = CreatePublishedContentQuery(
            mediaCache: new Mock<IPublishedMediaCache>(MockBehavior.Strict).Object,
            mediaNavigationQueryService: mediaNavigationQueryService.Object);

        Assert.IsEmpty(query.MediaAtRoot());
    }

    [Test]
    public void Search_WithNegativeSkip_Throws()
    {
        var query = CreatePublishedContentQuery();
        Assert.Throws<ArgumentOutOfRangeException>(() => query.Search("Products", -1, 0, out _));
    }

    [Test]
    public void Search_WithNegativeTake_Throws()
    {
        var query = CreatePublishedContentQuery();
        Assert.Throws<ArgumentOutOfRangeException>(() => query.Search("Products", 0, -1, out _));
    }

    [Test]
    public void Search_WithMissingIndex_Throws()
    {
        var query = CreatePublishedContentQuery(examineManager: Mock.Of<IExamineManager>());
        Assert.Throws<InvalidOperationException>(() => query.Search("Products", "*", "MissingIndex"));
    }

    [Test]
    public void Search_WithEmptyIndexName_UsesExternalIndexByDefault()
    {
        using var luceneDir = new RandomIdRAMDirectory();
        var fields = new[] { (Name: "title", Culture: (string)null) };
        using var indexer = CreateTestIndex(luceneDir, fields);

        var query = CreatePublishedContentQuery(
            indexer,
            indexName: Constants.UmbracoIndexes.ExternalIndexName);

        var ids = query.Search("Products", "*", string.Empty).Select(x => x.Content.Id).ToArray();
        CollectionAssert.AreEqual(new[] { 1 }, ids);
    }

    [Test]
    public void Search_WithNullIndexName_UsesExternalIndexByDefault()
    {
        using var luceneDir = new RandomIdRAMDirectory();
        var fields = new[] { (Name: "title", Culture: (string)null) };
        using var indexer = CreateTestIndex(luceneDir, fields);

        var query = CreatePublishedContentQuery(
            indexer,
            indexName: Constants.UmbracoIndexes.ExternalIndexName);

        string nullIndexName = null!;
        var ids = query.Search("Products", "*", nullIndexName).Select(x => x.Content.Id).ToArray();
        CollectionAssert.AreEqual(new[] { 1 }, ids);
    }

    [Test]
    public void Search_QueryOverload_WithNegativeSkip_Throws()
    {
        var query = CreatePublishedContentQuery();
        var queryExecutor = new TestQueryExecutor(CreateSearchResults(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => query.Search(queryExecutor, -1, 0, out _, "en-US"));
    }

    [Test]
    public void Search_QueryOverload_WithNegativeTake_Throws()
    {
        var query = CreatePublishedContentQuery();
        var queryExecutor = new TestQueryExecutor(CreateSearchResults(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => query.Search(queryExecutor, 0, -1, out _, "en-US"));
    }

    [Test]
    public void Search_QueryOverload_ExecutesAndMapsResults()
    {
        var content = Mock.Of<IPublishedContent>(x => x.Id == 11);
        var contentCache = CreateContentCache(new Dictionary<int, IPublishedContent> { [11] = content });
        var query = CreatePublishedContentQuery(contentCache: contentCache.Object);
        var queryExecutor = new TestQueryExecutor(CreateSearchResults(1, "11"));

        var results = query.Search(queryExecutor).ToArray();

        CollectionAssert.AreEqual(new[] { 11 }, results.Select(x => x.Content.Id).ToArray());
        Assert.AreEqual(1, queryExecutor.ExecuteCount);
        Assert.AreEqual(0, queryExecutor.LastQueryOptions.Skip);
        Assert.AreEqual(100, queryExecutor.LastQueryOptions.Take);
    }

    [Test]
    public void Search_QueryPagingOverload_UsesSkipAndTakeAndReturnsTotalRecords()
    {
        var content = Mock.Of<IPublishedContent>(x => x.Id == 12);
        var contentCache = CreateContentCache(new Dictionary<int, IPublishedContent> { [12] = content });
        var query = CreatePublishedContentQuery(contentCache: contentCache.Object);
        var queryExecutor = new TestQueryExecutor(CreateSearchResults(44, "12"));

        var results = query.Search(queryExecutor, 3, 2, out var totalRecords).ToArray();

        Assert.AreEqual(44, totalRecords);
        CollectionAssert.AreEqual(new[] { 12 }, results.Select(x => x.Content.Id).ToArray());
        Assert.AreEqual(3, queryExecutor.LastQueryOptions.Skip);
        Assert.AreEqual(2, queryExecutor.LastQueryOptions.Take);
    }

    [Test]
    public void Search_WithOrdering_SelectsExpectedReturnedFields()
    {
        var content = Mock.Of<IPublishedContent>(x => x.Id == 99);
        var contentCache = CreateContentCache(new Dictionary<int, IPublishedContent> { [99] = content });
        var query = CreatePublishedContentQuery(contentCache: contentCache.Object);
        var ordering = new TestOrderingQueryExecutor(CreateSearchResults(1, "99"));

        var results = query.Search(ordering, 0, 0, out var totalRecords).ToArray();

        Assert.AreEqual(1, totalRecords);
        CollectionAssert.AreEquivalent(
            new[] { ExamineFieldNames.ItemIdFieldName, ExamineFieldNames.CategoryFieldName },
            ordering.SelectedFieldNames);
        CollectionAssert.AreEqual(new[] { 99 }, results.Select(x => x.Content.Id).ToArray());
    }

    [Test]
    public void Search_WithCulture_ContextualizesDuringEnumerationAndResetsAfterDispose()
    {
        var content = Mock.Of<IPublishedContent>(x => x.Id == 14);
        var contentCache = CreateContentCache(new Dictionary<int, IPublishedContent> { [14] = content });
        var variationContextAccessor = new Mock<IVariationContextAccessor>();
        variationContextAccessor.SetupProperty(x => x.VariationContext, new VariationContext("en-US"));

        var query = CreatePublishedContentQuery(
            contentCache: contentCache.Object,
            variationContextAccessor: variationContextAccessor.Object);

        var queryExecutor = new TestQueryExecutor(CreateSearchResults(1, "14"));
        var results = query.Search(queryExecutor, 0, 0, out _, "fr-FR");

        Assert.AreEqual("en-US", variationContextAccessor.Object.VariationContext?.Culture);

        using (var enumerator = results.GetEnumerator())
        {
            Assert.AreEqual("fr-FR", variationContextAccessor.Object.VariationContext?.Culture);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(14, enumerator.Current.Content.Id);
        }

        Assert.AreEqual("en-US", variationContextAccessor.Object.VariationContext?.Culture);
    }

    [Test]
    public void Search_WithExistingCulture_DoesNotReplaceVariationContext()
    {
        var content = Mock.Of<IPublishedContent>(x => x.Id == 15);
        var contentCache = CreateContentCache(new Dictionary<int, IPublishedContent> { [15] = content });
        var variationContextAccessor = new Mock<IVariationContextAccessor>();
        variationContextAccessor.SetupProperty(x => x.VariationContext, new VariationContext("fr-FR"));

        var query = CreatePublishedContentQuery(
            contentCache: contentCache.Object,
            variationContextAccessor: variationContextAccessor.Object);

        var queryExecutor = new TestQueryExecutor(CreateSearchResults(1, "15"));
        var results = query.Search(queryExecutor, 0, 0, out _, "fr-FR");

        using (var enumerator = results.GetEnumerator())
        {
            Assert.AreEqual("fr-FR", variationContextAccessor.Object.VariationContext?.Culture);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(15, enumerator.Current.Content.Id);
        }

        Assert.AreEqual("fr-FR", variationContextAccessor.Object.VariationContext?.Culture);
    }

    [TestCase("fr-fr", ExpectedResult = "1, 3", Description = "Search Culture: fr-fr. Must return both fr-fr and invariant results")]
    [TestCase("en-us", ExpectedResult = "1, 2", Description = "Search Culture: en-us. Must return both en-us and invariant results")]
    [TestCase("*", ExpectedResult = "1, 2, 3", Description = "Search Culture: *. Must return all cultures and all invariant results")]
    [TestCase(null, ExpectedResult = "1", Description = "Search Culture: null. Must return only invariant results")]
    [LongRunning]
    public string Search(string culture)
    {
        using (var luceneDir = new RandomIdRAMDirectory())
        {
            var fields = new[] { (Name: "title", Culture: null), (Name: "title_en-us", Culture: "en-us"), (Name: "title_fr-fr", Culture: "fr-fr") };
            using (var indexer = CreateTestIndex(luceneDir, fields))
            {
                var pcq = CreatePublishedContentQuery(indexer);

                var results = pcq.Search("Products", culture, "TestIndex");

                var ids = results.Select(x => x.Content.Id).ToArray();

                return string.Join(", ", ids);
            }
        }
    }

    private class TestQueryExecutor : IQueryExecutor
    {
        private readonly ISearchResults _searchResults;

        public TestQueryExecutor(ISearchResults searchResults) => _searchResults = searchResults;

        public int ExecuteCount { get; private set; }

        public QueryOptions LastQueryOptions { get; private set; } = QueryOptions.Default;

        public ISearchResults Execute(QueryOptions options)
        {
            options ??= QueryOptions.Default;
            LastQueryOptions = options;
            ExecuteCount++;
            return _searchResults;
        }
    }

    private sealed class TestOrderingQueryExecutor : TestQueryExecutor, IOrdering
    {
        public TestOrderingQueryExecutor(ISearchResults searchResults)
            : base(searchResults)
        {
        }

        public ISet<string> SelectedFieldNames { get; private set; } = new HashSet<string>();

        public IOrdering OrderBy(params SortableField[] fields) => this;

        public IOrdering OrderByDescending(params SortableField[] fields) => this;

        public IOrdering SelectAllFields()
        {
            SelectedFieldNames = new HashSet<string>();
            return this;
        }

        public IOrdering SelectField(string fieldName)
        {
            SelectedFieldNames = new HashSet<string> { fieldName };
            return this;
        }

        public IOrdering SelectFields(ISet<string> fieldNames)
        {
            SelectedFieldNames = fieldNames;
            return this;
        }
    }

    private sealed class TestSearchResults : ISearchResults
    {
        private readonly IReadOnlyCollection<ISearchResult> _searchResults;

        public TestSearchResults(IReadOnlyCollection<ISearchResult> searchResults, long totalItemCount)
        {
            _searchResults = searchResults;
            TotalItemCount = totalItemCount;
        }

        public long TotalItemCount { get; }

        public IEnumerator<ISearchResult> GetEnumerator() => _searchResults.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
