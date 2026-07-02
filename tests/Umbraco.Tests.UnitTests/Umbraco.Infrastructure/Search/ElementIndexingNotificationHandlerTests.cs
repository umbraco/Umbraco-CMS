using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Search;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Search;

[TestFixture]
public class ElementIndexingNotificationHandlerTests
{
    private Mock<IDeferredSearchReindexService> _mockReindexService = null!;
    private ElementIndexingNotificationHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockReindexService = new Mock<IDeferredSearchReindexService>();
        _sut = new ElementIndexingNotificationHandler(_mockReindexService.Object);
    }

    [Test]
    public void Handle_ElementPublishedNotification_QueuesElementIds()
    {
        var element1 = ElementWithId(30);
        var element2 = ElementWithId(40);
        var notification = new ElementPublishedNotification([element1, element2], new EventMessages());

        _sut.Handle(notification);

        _mockReindexService.Verify(
            s => s.QueueElementReindex(It.Is<IReadOnlyCollection<int>>(ids => ids.SequenceEqual(new[] { 30, 40 }))),
            Times.Once);
    }

    [Test]
    public void Handle_ElementPublishedNotification_WhenEmpty_DoesNotQueue()
    {
        var notification = new ElementPublishedNotification([], new EventMessages());

        _sut.Handle(notification);

        _mockReindexService.Verify(s => s.QueueElementReindex(It.IsAny<IReadOnlyCollection<int>>()), Times.Never);
    }

    private static IElement ElementWithId(int id)
    {
        var mock = new Mock<IElement>();
        mock.Setup(e => e.Id).Returns(id);
        return mock.Object;
    }
}
