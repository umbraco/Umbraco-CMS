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
public class LanguageCacheRefresherTests
{
    private Mock<IDomainCacheService> _domainCacheService = null!;

    [SetUp]
    public void SetUp() => _domainCacheService = new Mock<IDomainCacheService>();

    [Test]
    public void RefreshInternal_Does_Not_Refresh_The_Domain_Cache()
    {
        LanguageCacheRefresher refresher = CreateRefresher();

        refresher.RefreshInternal([Payload(LanguageCacheRefresher.JsonPayload.LanguageChangeType.Remove)]);

        _domainCacheService.Verify(x => x.Refresh(It.IsAny<DomainCacheRefresher.JsonPayload[]>()), Times.Never);
    }

    [TestCase(LanguageCacheRefresher.JsonPayload.LanguageChangeType.Remove)]
    [TestCase(LanguageCacheRefresher.JsonPayload.LanguageChangeType.ChangeCulture)]
    public void Refresh_Refreshes_All_Domains_When_Content_Is_Affected(LanguageCacheRefresher.JsonPayload.LanguageChangeType changeType)
    {
        LanguageCacheRefresher refresher = CreateRefresher();

        refresher.Refresh([Payload(changeType)]);

        _domainCacheService.Verify(
            x => x.Refresh(It.Is<DomainCacheRefresher.JsonPayload[]>(p => p.Length == 1 && p[0].ChangeType == DomainChangeTypes.RefreshAll)),
            Times.Once);
    }

    [TestCase(LanguageCacheRefresher.JsonPayload.LanguageChangeType.Add)]
    [TestCase(LanguageCacheRefresher.JsonPayload.LanguageChangeType.Update)]
    public void Refresh_Does_Not_Refresh_Domains_When_Content_Is_Not_Affected(LanguageCacheRefresher.JsonPayload.LanguageChangeType changeType)
    {
        LanguageCacheRefresher refresher = CreateRefresher();

        refresher.Refresh([Payload(changeType)]);

        _domainCacheService.Verify(x => x.Refresh(It.IsAny<DomainCacheRefresher.JsonPayload[]>()), Times.Never);
    }

    private static LanguageCacheRefresher.JsonPayload Payload(LanguageCacheRefresher.JsonPayload.LanguageChangeType changeType)
        => new(1, "en-US", changeType);

    private LanguageCacheRefresher CreateRefresher()
    {
        var notificationFactory = new Mock<ICacheRefresherNotificationFactory>();
        notificationFactory
            .Setup(x => x.Create<LanguageCacheRefresherNotification>(It.IsAny<object>(), It.IsAny<MessageType>()))
            .Returns(new LanguageCacheRefresherNotification(new object(), MessageType.RefreshByPayload));

        return new LanguageCacheRefresher(
            AppCaches.NoCache,
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IEventAggregator>(),
            _domainCacheService.Object,
            notificationFactory.Object);
    }
}
