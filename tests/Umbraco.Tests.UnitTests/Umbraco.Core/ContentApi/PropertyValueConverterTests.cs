using System;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

public class PropertyValueConverterTests : ContentApiTests
{
    protected IPublishedSnapshotAccessor PublishedSnapshotAccessor { get; private set; }

    protected IPublishedUrlProvider PublishedUrlProvider { get; private set; }

    protected IPublishedContent PublishedContent { get; private set; }

    protected IPublishedContent PublishedMedia { get; private set; }

    protected IPublishedContentType PublishedContentType { get; private set; }

    protected IPublishedContentType PublishedMediaType { get; private set; }

    protected Mock<IPublishedContentCache> PublishedContentCacheMock { get; private set; }

    protected Mock<IPublishedMediaCache> PublishedMediaCacheMock { get; private set; }

    protected Mock<IPublishedUrlProvider> PublishedUrlProviderMock { get; private set; }

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        var publishedContentType = new Mock<IPublishedContentType>();
        publishedContentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);
        publishedContentType.SetupGet(c => c.Alias).Returns("TheContentType");
        PublishedContentType = publishedContentType.Object;
        var publishedMediaType = new Mock<IPublishedContentType>();
        publishedMediaType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Media);
        publishedMediaType.SetupGet(c => c.Alias).Returns("TheMediaType");
        PublishedMediaType = publishedMediaType.Object;

        var contentKey = Guid.NewGuid();
        var publishedContent = SetupPublishedContent("The page", contentKey, PublishedItemType.Content, publishedContentType.Object);
        PublishedContent = publishedContent.Object;

        var mediaKey = Guid.NewGuid();
        var publishedMedia = SetupPublishedContent("The media", mediaKey, PublishedItemType.Media, publishedMediaType.Object);
        PublishedMedia = publishedMedia.Object;

        PublishedContentCacheMock = new Mock<IPublishedContentCache>();
        PublishedContentCacheMock
            .Setup(pcc => pcc.GetById(contentKey))
            .Returns(publishedContent.Object);
        PublishedMediaCacheMock = new Mock<IPublishedMediaCache>();
        PublishedMediaCacheMock
            .Setup(pcc => pcc.GetById(mediaKey))
            .Returns(publishedMedia.Object);

        var publishedSnapshot = new Mock<IPublishedSnapshot>();
        publishedSnapshot.SetupGet(ps => ps.Content).Returns(PublishedContentCacheMock.Object);
        publishedSnapshot.SetupGet(ps => ps.Media).Returns(PublishedMediaCacheMock.Object);

        PublishedUrlProviderMock = new Mock<IPublishedUrlProvider>();
        PublishedUrlProviderMock
            .Setup(p => p.GetUrl(publishedContent.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns("the-page-url");
        PublishedUrlProviderMock
            .Setup(p => p.GetMediaUrl(publishedMedia.Object, It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns("the-media-url");
        PublishedUrlProvider = PublishedUrlProviderMock.Object;

        var publishedSnapshotAccessor = new Mock<IPublishedSnapshotAccessor>();
        var publishedSnapshotObject = publishedSnapshot.Object;
        publishedSnapshotAccessor
            .Setup(psa => psa.TryGetPublishedSnapshot(out publishedSnapshotObject))
            .Returns(true);
        PublishedSnapshotAccessor = publishedSnapshotAccessor.Object;
    }

    protected Mock<IPublishedContent> SetupPublishedContent(string name, Guid key, PublishedItemType itemType, IPublishedContentType contentType)
    {
        var content = new Mock<IPublishedContent>();
        content.SetupGet(c => c.Properties).Returns(Array.Empty<PublishedElementPropertyBase>());
        content.SetupGet(c => c.Key).Returns(key);
        content.SetupGet(c => c.Name).Returns(name);
        content.SetupGet(c => c.ItemType).Returns(itemType);
        content.SetupGet(c => c.ContentType).Returns(contentType);
        return content;
    }
}
