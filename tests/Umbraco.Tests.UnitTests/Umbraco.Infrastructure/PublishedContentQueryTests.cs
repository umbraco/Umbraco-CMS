using System.Collections;
using System.Globalization;
using Examine;
using Examine.Search;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure;

[TestFixture]
internal sealed class PublishedContentQueryTests
{
    private PublishedContentQuery CreatePublishedContentQuery(
        IExamineManager? examineManager = null,
        IPublishedContentCache? contentCache = null,
        IPublishedMediaCache? mediaCache = null,
        IVariationContextAccessor? variationContextAccessor = null,
        IDocumentNavigationQueryService? documentNavigationQueryService = null,
        IMediaNavigationQueryService? mediaNavigationQueryService = null)
    {
        examineManager ??= Mock.Of<IExamineManager>();

        if (contentCache is null)
        {
            var contentCacheMock = new Mock<IPublishedContentCache>();
            contentCacheMock.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns((int intId) => Mock.Of<IPublishedContent>(x => x.Id == intId));
            contentCache = contentCacheMock.Object;
        }

        mediaCache ??= Mock.Of<IPublishedMediaCache>();

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

        Assert.Multiple(() =>
        {
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
        });
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

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(new[] { contentId }, query.Content([contentId, 99]).Select(x => x.Id).ToArray());
            CollectionAssert.AreEqual(new[] { contentId }, query.Content([contentKey, Guid.NewGuid()]).Select(x => x.Id).ToArray());

            var objectResults = query.Content(
                [contentId, contentKey.ToString(), contentUdi.ToString(), "not-a-guid", new object()]).ToArray();
            Assert.AreEqual(3, objectResults.Length);
            Assert.That(objectResults.All(x => x.Id == contentId));
        });
    }

    [Test]
    public void ContentAtRoot_ReturnsItemsFromRootKeys()
    {
        var firstKey = Guid.NewGuid();
        var secondKey = Guid.NewGuid();
        var missingKey = Guid.NewGuid();
        IEnumerable<Guid> rootKeys = [firstKey, secondKey, missingKey];

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

        Assert.Multiple(() =>
        {
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
        });
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

        Assert.Multiple(() =>
        {
            CollectionAssert.AreEqual(new[] { mediaId }, query.Media([mediaId, 99]).Select(x => x.Id).ToArray());
            CollectionAssert.AreEqual(new[] { mediaId }, query.Media([mediaKey, Guid.NewGuid()]).Select(x => x.Id).ToArray());

            var objectResults = query.Media(
                [mediaId, mediaKey.ToString(), mediaUdi.ToString(), "not-a-guid", new object()]).ToArray();
            Assert.AreEqual(3, objectResults.Length);
            Assert.That(objectResults.All(x => x.Id == mediaId));
        });
    }

    [Test]
    public void MediaAtRoot_UsesMediaCacheForRootKeys()
    {
        var mediaKey = Guid.NewGuid();
        IEnumerable<Guid> rootKeys = [mediaKey];
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
