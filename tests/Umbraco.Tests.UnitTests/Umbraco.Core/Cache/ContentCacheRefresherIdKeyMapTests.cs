// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class ContentCacheRefresherIdKeyMapTests
{
    private Mock<IIdKeyMap> _idKeyMap = null!;

    [SetUp]
    public void SetUp() => _idKeyMap = new Mock<IIdKeyMap>();

    [Test]
    public void RefreshInternal_Clears_The_Id_Key_Map_For_Removed_Content()
    {
        ContentCacheRefresher refresher = CreateRefresher();
        var key = Guid.NewGuid();

        refresher.RefreshInternal([new ContentCacheRefresher.JsonPayload { Id = 1234, Key = key, ChangeTypes = TreeChangeTypes.Remove }]);

        _idKeyMap.Verify(x => x.ClearCache(1234), Times.Once);
        _idKeyMap.Verify(x => x.ClearCache(key), Times.Once);
    }

    [Test]
    public void RefreshInternal_Keeps_The_Id_Key_Map_For_Refreshed_Content()
    {
        ContentCacheRefresher refresher = CreateRefresher();

        refresher.RefreshInternal([new ContentCacheRefresher.JsonPayload { Id = 1234, Key = Guid.NewGuid(), ChangeTypes = TreeChangeTypes.RefreshNode }]);

        _idKeyMap.Verify(x => x.ClearCache(It.IsAny<int>()), Times.Never);
        _idKeyMap.Verify(x => x.ClearCache(It.IsAny<Guid>()), Times.Never);
    }

    private ContentCacheRefresher CreateRefresher()
    {
        var notificationFactory = new Mock<ICacheRefresherNotificationFactory>();
        notificationFactory
            .Setup(x => x.Create<ContentCacheRefresherNotification>(It.IsAny<object>(), It.IsAny<MessageType>()))
            .Returns(new ContentCacheRefresherNotification(new object(), MessageType.RefreshByPayload));

        return new ContentCacheRefresher(
            AppCaches.NoCache,
            Mock.Of<IJsonSerializer>(),
            _idKeyMap.Object,
            Mock.Of<IDomainService>(),
            Mock.Of<IEventAggregator>(),
            notificationFactory.Object,
            Mock.Of<IDocumentUrlService>(),
            Mock.Of<IDocumentUrlAliasService>(),
            Mock.Of<IDomainCacheService>(),
            Mock.Of<IDocumentNavigationQueryService>(),
            Mock.Of<IDocumentNavigationManagementService>(),
            Mock.Of<IContentService>(),
            Mock.Of<IPublishStatusManagementService>(),
            Mock.Of<IDocumentCacheService>(),
            Mock.Of<ICacheManager>(x => x.ElementsCache == Mock.Of<IAppCache>()));
    }
}
