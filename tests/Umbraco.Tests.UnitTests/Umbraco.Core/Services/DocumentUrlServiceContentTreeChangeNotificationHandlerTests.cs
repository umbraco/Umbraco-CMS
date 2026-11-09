using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class DocumentUrlServiceContentTreeChangeNotificationHandlerTests
{
    private static (DocumentUrlServiceContentTreeChangeNotificationHandler Handler,
        Mock<IDocumentUrlService> UrlService) CreateHandler(bool isInitialized = true)
    {
        var urlServiceMock = new Mock<IDocumentUrlService>();
        urlServiceMock.Setup(x => x.IsInitialized).Returns(isInitialized);
        urlServiceMock.Setup(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()))
            .Returns(Task.CompletedTask);
        urlServiceMock.Setup(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var handler = new DocumentUrlServiceContentTreeChangeNotificationHandler(urlServiceMock.Object);

        return (handler, urlServiceMock);
    }

    private static Mock<IContent> MakeContent()
    {
        var mock = new Mock<IContent>();
        mock.Setup(x => x.Key).Returns(Guid.NewGuid());
        return mock;
    }

    private static ContentTreeChangeNotification SingleChange(IContent content, TreeChangeTypes changeType)
        => new(
            [new TreeChange<IContent>(content, changeType)],
            new EventMessages());

    [Test]
    public async Task HandleAsync_WhenNotInitialized_DoesNotCallAnyServiceMethods()
    {
        var (handler, urlService) = CreateHandler(isInitialized: false);
        var content = MakeContent();
        var notification = SingleChange(content.Object, TreeChangeTypes.RefreshNode);

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()), Times.Never);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithSingleRefreshNode_CallsUrlSegmentsWithItem()
    {
        var (handler, urlService) = CreateHandler();
        var content = MakeContent();
        var key = content.Object.Key;
        var notification = SingleChange(content.Object, TreeChangeTypes.RefreshNode);

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(
            x => x.CreateOrUpdateUrlSegmentsAsync(It.Is<IEnumerable<IContent>>(items => items.Any(i => i.Key == key))),
            Times.Once);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithMultipleRefreshNodes_BatchesUrlSegments()
    {
        var (handler, urlService) = CreateHandler();
        var content1 = MakeContent();
        var content2 = MakeContent();
        var notification = new ContentTreeChangeNotification(
            new[]
            {
                new TreeChange<IContent>(content1.Object, TreeChangeTypes.RefreshNode),
                new TreeChange<IContent>(content2.Object, TreeChangeTypes.RefreshNode),
            },
            new EventMessages());

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()), Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithRefreshBranch_CallsUrlSegmentsWithDescendants()
    {
        var (handler, urlService) = CreateHandler();
        var content = MakeContent();
        var key = content.Object.Key;
        var notification = SingleChange(content.Object, TreeChangeTypes.RefreshBranch);

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(key), Times.Once);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithRemoveChange_DoesNotCallAnyCreateOrUpdateMethods()
    {
        var (handler, urlService) = CreateHandler();
        var content = MakeContent();
        var notification = SingleChange(content.Object, TreeChangeTypes.Remove);

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()), Times.Never);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithRefreshAll_DoesNotCallCreateOrUpdateMethods()
    {
        var (handler, urlService) = CreateHandler();
        var content = MakeContent();
        var notification = SingleChange(content.Object, TreeChangeTypes.RefreshAll);

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()), Times.Never);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithEmptyChanges_DoesNotCallAnyMethods()
    {
        var (handler, urlService) = CreateHandler();
        var notification = new ContentTreeChangeNotification(
            Array.Empty<TreeChange<IContent>>(),
            new EventMessages());

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()), Times.Never);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithBothRefreshNodeAndBranch_CallsTheUrlSegmentMethodForEach()
    {
        var (handler, urlService) = CreateHandler();
        var nodeContent = MakeContent();
        var branchContent = MakeContent();
        var nodeKey = nodeContent.Object.Key;
        var branchKey = branchContent.Object.Key;

        var notification = new ContentTreeChangeNotification(
            new[]
            {
                new TreeChange<IContent>(nodeContent.Object, TreeChangeTypes.RefreshNode),
                new TreeChange<IContent>(branchContent.Object, TreeChangeTypes.RefreshBranch),
            },
            new EventMessages());

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(
            x => x.CreateOrUpdateUrlSegmentsAsync(It.Is<IEnumerable<IContent>>(items => items.Any(i => i.Key == nodeKey))),
            Times.Once);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(branchKey), Times.Once);
    }
}
