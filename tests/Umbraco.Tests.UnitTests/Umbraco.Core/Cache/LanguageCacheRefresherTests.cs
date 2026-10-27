// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services.Changes;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class LanguageCacheRefresherTests
{
    private Mock<IDomainCacheService> _domainCacheService = null!;
    private Mock<IDocumentPublishStatusManagementService> _documentPublishStatusManagementService = null!;
    private Mock<IElementPublishStatusManagementService> _elementPublishStatusManagementService = null!;

    [SetUp]
    public void SetUp()
    {
        _domainCacheService = new Mock<IDomainCacheService>();
        _documentPublishStatusManagementService = new Mock<IDocumentPublishStatusManagementService>();
        _elementPublishStatusManagementService = new Mock<IElementPublishStatusManagementService>();
    }

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

    [Test]
    public void RefreshInternal_Clears_Dictionary_Items_When_A_Language_Is_Added()
    {
        var appCaches = new AppCaches(
            NoAppCache.Instance,
            NoAppCache.Instance,
            new IsolatedCaches(_ => new ObjectCacheAppCache()));
        appCaches.IsolatedCaches.GetOrCreate<IDictionaryItem>().Insert("dictionary-item", () => new object());
        LanguageCacheRefresher refresher = CreateRefresher(appCaches);

        refresher.RefreshInternal([Payload(LanguageCacheRefresher.JsonPayload.LanguageChangeType.Add)]);

        Assert.IsNull(appCaches.IsolatedCaches.GetOrCreate<IDictionaryItem>().Get("dictionary-item"));
    }

    [TestCase(LanguageCacheRefresher.JsonPayload.LanguageChangeType.Add)]
    [TestCase(LanguageCacheRefresher.JsonPayload.LanguageChangeType.Remove)]
    [TestCase(LanguageCacheRefresher.JsonPayload.LanguageChangeType.ChangeCulture)]
    public void Refresh_Refreshes_The_Publish_Status_Of_The_Culture_When_The_Set_Of_IsoCodes_Changes(LanguageCacheRefresher.JsonPayload.LanguageChangeType changeType)
    {
        LanguageCacheRefresher refresher = CreateRefresher();

        refresher.Refresh([Payload(changeType)]);

        _documentPublishStatusManagementService.Verify(x => x.RefreshCultureAsync("en-US", It.IsAny<CancellationToken>()), Times.Once);
        _elementPublishStatusManagementService.Verify(x => x.RefreshCultureAsync("en-US", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void Refresh_Does_Not_Refresh_The_Publish_Status_Of_The_Culture_When_The_IsoCode_Is_Unchanged()
    {
        LanguageCacheRefresher refresher = CreateRefresher();

        refresher.Refresh([Payload(LanguageCacheRefresher.JsonPayload.LanguageChangeType.Update)]);

        _documentPublishStatusManagementService.Verify(x => x.RefreshCultureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _elementPublishStatusManagementService.Verify(x => x.RefreshCultureAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestCase(LanguageCacheRefresher.JsonPayload.LanguageChangeType.Add)]
    [TestCase(LanguageCacheRefresher.JsonPayload.LanguageChangeType.Update)]
    [TestCase(LanguageCacheRefresher.JsonPayload.LanguageChangeType.ChangeCulture)]
    [TestCase(LanguageCacheRefresher.JsonPayload.LanguageChangeType.Remove)]
    public void Refresh_Refreshes_The_Default_Culture_Of_The_Publish_Status(LanguageCacheRefresher.JsonPayload.LanguageChangeType changeType)
    {
        LanguageCacheRefresher refresher = CreateRefresher();

        refresher.Refresh([Payload(changeType), Payload(changeType)]);

        _documentPublishStatusManagementService.Verify(x => x.RefreshDefaultCultureAsync(It.IsAny<CancellationToken>()), Times.Once);
        _elementPublishStatusManagementService.Verify(x => x.RefreshDefaultCultureAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static LanguageCacheRefresher.JsonPayload Payload(LanguageCacheRefresher.JsonPayload.LanguageChangeType changeType)
        => new(1, "en-US", changeType);

    private LanguageCacheRefresher CreateRefresher(AppCaches? appCaches = null)
    {
        var notificationFactory = new Mock<ICacheRefresherNotificationFactory>();
        notificationFactory
            .Setup(x => x.Create<LanguageCacheRefresherNotification>(It.IsAny<object>(), It.IsAny<MessageType>()))
            .Returns(new LanguageCacheRefresherNotification(new object(), MessageType.RefreshByPayload));

        return new LanguageCacheRefresher(
            appCaches ?? AppCaches.NoCache,
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IEventAggregator>(),
            _domainCacheService.Object,
            notificationFactory.Object,
            _documentPublishStatusManagementService.Object,
            _elementPublishStatusManagementService.Object);
    }
}
