using System.Collections;
using System.Globalization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure;

[TestFixture]
internal sealed class PublishedContentQueryTests
{
    private PublishedContentQuery CreatePublishedContentQuery(
        IPublishedContentCache? contentCache = null,
        IPublishedMediaCache? mediaCache = null,
        IDocumentNavigationQueryService? documentNavigationQueryService = null,
        IMediaNavigationQueryService? mediaNavigationQueryService = null)
    {

        if (contentCache is null)
        {
            var contentCacheMock = new Mock<IPublishedContentCache>();
            contentCacheMock.Setup(x => x.GetById(It.IsAny<int>()))
                .Returns((int intId) => Mock.Of<IPublishedContent>(x => x.Id == intId));
            contentCache = contentCacheMock.Object;
        }

        mediaCache ??= Mock.Of<IPublishedMediaCache>();
        documentNavigationQueryService ??= Mock.Of<IDocumentNavigationQueryService>();
        mediaNavigationQueryService ??= Mock.Of<IMediaNavigationQueryService>();

        return new PublishedContentQuery(
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
            Assert.That(query.Content(contentId), Is.SameAs(content));
            Assert.That(query.Content(contentKey), Is.SameAs(content));
            Assert.That(query.Content(contentUdi), Is.SameAs(content));
            Assert.That(query.Content((object)contentId), Is.SameAs(content));
            Assert.That(query.Content((object)contentId.ToString(CultureInfo.InvariantCulture)), Is.SameAs(content));
            Assert.That(query.Content((object)contentKey), Is.SameAs(content));
            Assert.That(query.Content((object)contentKey.ToString()), Is.SameAs(content));
            Assert.That(query.Content((object)contentUdi), Is.SameAs(content));
            Assert.That(query.Content((object)contentUdi.ToString()), Is.SameAs(content));

            Assert.That(query.Content((Udi?)null), Is.Null);
            Assert.That(query.Content(new StringUdi(Constants.UdiEntityType.Member, "member-a")), Is.Null);
            Assert.That(query.Content((object)"umb://member/member-a"), Is.Null);
            Assert.That(query.Content((object)new object()), Is.Null);
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
            Assert.That(query.Content([contentId, 99]).Select(x => x.Id).ToArray(), Is.EqualTo(new[] { contentId }).AsCollection);
            Assert.That(query.Content([contentKey, Guid.NewGuid()]).Select(x => x.Id).ToArray(), Is.EqualTo(new[] { contentId }).AsCollection);

            var objectResults = query.Content(
                [contentId, contentKey.ToString(), contentUdi.ToString(), "not-a-guid", new object()]).ToArray();
            Assert.That(objectResults, Has.Length.EqualTo(3));
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

        Assert.That(query.ContentAtRoot().Select(x => x.Id).ToArray(), Is.EqualTo(new[] { 1, 2 }).AsCollection);
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

        Assert.That(query.ContentAtRoot(), Is.Empty);
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
            Assert.That(query.Media(mediaId), Is.SameAs(media));
            Assert.That(query.Media(mediaKey), Is.SameAs(media));
            Assert.That(query.Media(mediaUdi), Is.SameAs(media));
            Assert.That(query.Media((object)mediaId), Is.SameAs(media));
            Assert.That(query.Media((object)mediaId.ToString(CultureInfo.InvariantCulture)), Is.SameAs(media));
            Assert.That(query.Media((object)mediaKey), Is.SameAs(media));
            Assert.That(query.Media((object)mediaKey.ToString()), Is.SameAs(media));
            Assert.That(query.Media((object)mediaUdi), Is.SameAs(media));
            Assert.That(query.Media((object)mediaUdi.ToString()), Is.SameAs(media));

            Assert.That(query.Media((Udi?)null), Is.Null);
            Assert.That(query.Media(new StringUdi(Constants.UdiEntityType.Member, "member-a")), Is.Null);
            Assert.That(query.Media((object)"umb://member/member-a"), Is.Null);
            Assert.That(query.Media((object)new object()), Is.Null);
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
            Assert.That(query.Media([mediaId, 99]).Select(x => x.Id).ToArray(), Is.EqualTo(new[] { mediaId }).AsCollection);
            Assert.That(query.Media([mediaKey, Guid.NewGuid()]).Select(x => x.Id).ToArray(), Is.EqualTo(new[] { mediaId }).AsCollection);

            var objectResults = query.Media(
                [mediaId, mediaKey.ToString(), mediaUdi.ToString(), "not-a-guid", new object()]).ToArray();
            Assert.That(objectResults, Has.Length.EqualTo(3));
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

        Assert.That(query.MediaAtRoot().Select(x => x.Id).ToArray(), Is.EqualTo(new[] { 7 }).AsCollection);
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

        Assert.That(query.MediaAtRoot(), Is.Empty);
    }
}
