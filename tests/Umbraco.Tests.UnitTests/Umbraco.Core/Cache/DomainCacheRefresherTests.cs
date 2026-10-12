// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class DomainCacheRefresherTests
{
    private Mock<IDomainCacheService> _domainCacheService = null!;

    [SetUp]
    public void SetUp() => _domainCacheService = new Mock<IDomainCacheService>();

    [Test]
    public void RefreshInternal_Does_Not_Refresh_The_Domain_Cache()
    {
        DomainCacheRefresher refresher = CreateRefresher();

        refresher.RefreshInternal([new DomainCacheRefresher.JsonPayload(0, DomainChangeTypes.RefreshAll)]);

        _domainCacheService.Verify(x => x.Refresh(It.IsAny<DomainCacheRefresher.JsonPayload[]>()), Times.Never);
    }

    [Test]
    public void Refresh_Refreshes_The_Domain_Cache()
    {
        DomainCacheRefresher refresher = CreateRefresher();
        DomainCacheRefresher.JsonPayload[] payloads = [new DomainCacheRefresher.JsonPayload(1234, DomainChangeTypes.Refresh)];

        refresher.Refresh(payloads);

        _domainCacheService.Verify(x => x.Refresh(payloads), Times.Once);
    }

    private DomainCacheRefresher CreateRefresher()
    {
        var notificationFactory = new Mock<ICacheRefresherNotificationFactory>();
        notificationFactory
            .Setup(x => x.Create<DomainCacheRefresherNotification>(It.IsAny<object>(), It.IsAny<MessageType>()))
            .Returns(new DomainCacheRefresherNotification(new object(), MessageType.RefreshByPayload));

        return new DomainCacheRefresher(
            AppCaches.NoCache,
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IEventAggregator>(),
            notificationFactory.Object,
            _domainCacheService.Object);
    }
}
