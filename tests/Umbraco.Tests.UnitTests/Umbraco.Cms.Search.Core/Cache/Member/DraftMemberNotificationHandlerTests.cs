using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Search.Core.Cache.Member;
using Umbraco.Cms.Search.Core.Services.ContentIndexing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Search.Core.Cache.Member;

[TestFixture]
internal sealed class DraftMemberNotificationHandlerTests
{
    private Mock<IIndexDocumentService> _indexDocumentServiceMock = null!;
    private DraftMemberNotificationHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _indexDocumentServiceMock = new Mock<IIndexDocumentService>();

        var cacheRefreshers = new CacheRefresherCollection(() => new[]
        {
            Mock.Of<ICacheRefresher>(x => x.RefresherUniqueId == DraftMemberCacheRefresher.UniqueId),
        });
        var distributedCache = new DistributedCache(Mock.Of<IServerMessenger>(), cacheRefreshers);

        _handler = new DraftMemberNotificationHandler(
            distributedCache,
            Mock.Of<IOriginProvider>(x => x.GetCurrent() == "test-origin"),
            _indexDocumentServiceMock.Object);
    }

    [Test]
    public void Handle_WhenIndexableFieldsChangedIsExplicitlyFalse_SkipsReindex()
    {
        IMember member = Mock.Of<IMember>(x => x.Key == Guid.NewGuid());
        var notification = new MemberSavedNotification(member, new EventMessages());
        notification.State[Constants.Conventions.Member.IndexableFieldsChangedStateKey] = false;

        _handler.Handle(notification);

        _indexDocumentServiceMock.Verify(x => x.DeleteAsync(It.IsAny<Guid[]>(), It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public void Handle_WhenIndexableFieldsChangedIsExplicitlyTrue_Reindexes()
    {
        var memberKey = Guid.NewGuid();
        IMember member = Mock.Of<IMember>(x => x.Key == memberKey);
        var notification = new MemberSavedNotification(member, new EventMessages());
        notification.State[Constants.Conventions.Member.IndexableFieldsChangedStateKey] = true;

        _handler.Handle(notification);

        _indexDocumentServiceMock.Verify(x => x.DeleteAsync(It.Is<Guid[]>(ids => ids.Length == 1 && ids[0] == memberKey), false), Times.Once);
    }

    [Test]
    public void Handle_WhenIndexableFieldsChangedStateIsNotSet_ReindexesByDefault()
    {
        var memberKey = Guid.NewGuid();
        IMember member = Mock.Of<IMember>(x => x.Key == memberKey);
        var notification = new MemberSavedNotification(member, new EventMessages());

        _handler.Handle(notification);

        _indexDocumentServiceMock.Verify(x => x.DeleteAsync(It.Is<Guid[]>(ids => ids.Length == 1 && ids[0] == memberKey), false), Times.Once);
    }
}
