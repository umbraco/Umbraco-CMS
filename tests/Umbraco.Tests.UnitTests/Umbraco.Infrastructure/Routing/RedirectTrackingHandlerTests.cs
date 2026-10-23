using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Routing;

[TestFixture]
public class RedirectTrackingHandlerTests
{
    private WebRoutingSettings _webRoutingSettings = null!;
    private Mock<IRedirectTracker> _redirectTracker = null!;
    private List<Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>> _storedOldRoutes = null!;
    private List<Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>> _createdRedirects = null!;
    private Func<IContent, string> _getOldRoute = null!;
    private RedirectTrackingHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _webRoutingSettings = new WebRoutingSettings();
        _storedOldRoutes = [];
        _createdRedirects = [];
        _getOldRoute = content => $"/{content.Name}";

        _redirectTracker = new Mock<IRedirectTracker>();
        _redirectTracker
            .Setup(x => x.StoreOldRoute(It.IsAny<IContent>(), It.IsAny<Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>>(), It.IsAny<bool>()))
            .Callback((IContent content, Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes, bool _) =>
            {
                _storedOldRoutes.Add(oldRoutes);
                oldRoutes[(content.Id, string.Empty)] = (content.Key, _getOldRoute(content));
            });
        _redirectTracker
            .Setup(x => x.CreateRedirects(It.IsAny<IDictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>>()))
            .Callback((IDictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes) => _createdRedirects.Add(new(oldRoutes)));

        _sut = new RedirectTrackingHandler(Mock.Of<IOptionsMonitor<WebRoutingSettings>>(x => x.CurrentValue == _webRoutingSettings), _redirectTracker.Object);
    }

    [Test]
    public void Handle_Moving_And_Moved_Creates_Redirects()
    {
        IContent content = CreateContent(1, "page");
        var moving = CreateMovingNotification(content);

        _sut.Handle(moving);
        _sut.Handle(new ContentMovedNotification(moving.MoveInfoCollection, new EventMessages()).WithStateFrom(moving));

        Assert.AreEqual(1, _createdRedirects.Count);
        Assert.AreEqual((content.Key, "/page"), _createdRedirects[0][(1, string.Empty)]);
    }

    [Test]
    public void Handle_Moving_Batch_Shares_Old_Routes_And_Creates_Redirects_Once()
    {
        IContent content1 = CreateContent(1, "first");
        IContent content2 = CreateContent(2, "second");
        ContentMovingNotification[] moving = [CreateMovingNotification(content1), CreateMovingNotification(content2)];

        HandleBatch(moving);
        HandleBatch(moving.Select(x => new ContentMovedNotification(x.MoveInfoCollection, new EventMessages()).WithStateFrom(x)));

        Assert.AreEqual(2, _storedOldRoutes.Count);
        Assert.AreSame(_storedOldRoutes[0], _storedOldRoutes[1], "The old routes should be shared within the batch.");
        Assert.AreEqual(1, _createdRedirects.Count, "Shared old routes should only create redirects once.");
        Assert.AreEqual(2, _createdRedirects[0].Count);
    }

    [Test]
    public void Handle_Moved_Batch_Merges_Separately_Stored_Old_Routes_Keeping_The_First()
    {
        IContent content = CreateContent(1, "page");
        var firstMove = CreateMovingNotification(content);
        var secondMove = CreateMovingNotification(content);

        // Moving notifications are published immediately (one by one), while moved notifications are published as a batch on scope exit.
        _sut.Handle(firstMove);
        _getOldRoute = _ => "/moved-once";
        _sut.Handle(secondMove);
        HandleBatch(new[] { firstMove, secondMove }.Select(x => new ContentMovedNotification(x.MoveInfoCollection, new EventMessages()).WithStateFrom(x)));

        Assert.AreEqual(1, _createdRedirects.Count);
        Assert.AreEqual((content.Key, "/page"), _createdRedirects[0].Single().Value);
    }

    [Test]
    public void Handle_Publishing_Batch_Shares_Old_Routes_And_Creates_Redirects_Once()
    {
        ContentPublishingNotification[] publishing =
        [
            new(CreateContent(1, "first"), new EventMessages()),
            new(CreateContent(2, "second"), new EventMessages()),
        ];

        HandleBatch(publishing);
        HandleBatch(publishing.Select(x => new ContentPublishedNotification(x.PublishedEntities, new EventMessages()).WithStateFrom(x)));

        Assert.AreSame(_storedOldRoutes[0], _storedOldRoutes[1], "The old routes should be shared within the batch.");
        _redirectTracker.Verify(x => x.StoreOldRoute(It.IsAny<IContent>(), It.IsAny<Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>>(), false), Times.Exactly(2));
        Assert.AreEqual(1, _createdRedirects.Count);
        Assert.AreEqual(2, _createdRedirects[0].Count);
    }

    [Test]
    public void Handle_Does_Nothing_When_Redirect_Tracking_Is_Disabled()
    {
        _webRoutingSettings.DisableRedirectUrlTracking = true;
        var moving = CreateMovingNotification(CreateContent(1, "page"));

        _sut.Handle(moving);
        _sut.Handle(new ContentMovedNotification(moving.MoveInfoCollection, new EventMessages()).WithStateFrom(moving));

        _redirectTracker.VerifyNoOtherCalls();
    }

    // Batches are dispatched through the interface, like the event aggregator does.
    private void HandleBatch<TNotification>(IEnumerable<TNotification> notifications)
        where TNotification : INotification
        => ((INotificationHandler<TNotification>)(object)_sut).Handle(notifications);

    private static IContent CreateContent(int id, string name)
        => Mock.Of<IContent>(x => x.Id == id && x.Key == Guid.NewGuid() && x.Name == name);

    private static ContentMovingNotification CreateMovingNotification(IContent content)
        => new(new MoveEventInfo<IContent>(content, "-1," + content.Id, -1), new EventMessages());
}
