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
        Mock<IDocumentUrlService> UrlService,
        Mock<IDocumentUrlAliasService> AliasService) CreateHandler(bool isInitialized = true)
    {
        var urlServiceMock = new Mock<IDocumentUrlService>();
        urlServiceMock.Setup(x => x.IsInitialized).Returns(isInitialized);
        urlServiceMock.Setup(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()))
            .Returns(Task.CompletedTask);
        urlServiceMock.Setup(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var aliasServiceMock = new Mock<IDocumentUrlAliasService>();
        aliasServiceMock.Setup(x => x.CreateOrUpdateAliasesAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);
        aliasServiceMock.Setup(x => x.CreateOrUpdateAliasesWithDescendantsAsync(It.IsAny<Guid>()))
            .Returns(Task.CompletedTask);

        var handler = new DocumentUrlServiceContentTreeChangeNotificationHandler(
            urlServiceMock.Object, aliasServiceMock.Object);

        return (handler, urlServiceMock, aliasServiceMock);
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
        var (handler, urlService, aliasService) = CreateHandler(isInitialized: false);
        var content = MakeContent();
        var notification = SingleChange(content.Object, TreeChangeTypes.RefreshNode);

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()), Times.Never);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesAsync(It.IsAny<Guid>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithSingleRefreshNode_CallsUrlSegmentsWithItemAndAliasesWithKey()
    {
        var (handler, urlService, aliasService) = CreateHandler();
        var content = MakeContent();
        var key = content.Object.Key;
        var notification = SingleChange(content.Object, TreeChangeTypes.RefreshNode);

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(
            x => x.CreateOrUpdateUrlSegmentsAsync(It.Is<IEnumerable<IContent>>(items => items.Any(i => i.Key == key))),
            Times.Once);
        aliasService.Verify(x => x.CreateOrUpdateAliasesAsync(key), Times.Once);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithMultipleRefreshNodes_BatchesUrlSegmentsAndCallsAliasesPerItem()
    {
        var (handler, urlService, aliasService) = CreateHandler();
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
        aliasService.Verify(x => x.CreateOrUpdateAliasesAsync(content1.Object.Key), Times.Once);
        aliasService.Verify(x => x.CreateOrUpdateAliasesAsync(content2.Object.Key), Times.Once);
    }

    [Test]
    public async Task HandleAsync_WithRefreshBranch_CallsWithDescendantsMethods()
    {
        var (handler, urlService, aliasService) = CreateHandler();
        var content = MakeContent();
        var key = content.Object.Key;
        var notification = SingleChange(content.Object, TreeChangeTypes.RefreshBranch);

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(key), Times.Once);
        aliasService.Verify(x => x.CreateOrUpdateAliasesWithDescendantsAsync(key), Times.Once);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithRemoveChange_DoesNotCallAnyCreateOrUpdateMethods()
    {
        var (handler, urlService, aliasService) = CreateHandler();
        var content = MakeContent();
        var notification = SingleChange(content.Object, TreeChangeTypes.Remove);

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()), Times.Never);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesAsync(It.IsAny<Guid>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithRefreshAll_DoesNotCallCreateOrUpdateMethods()
    {
        var (handler, urlService, aliasService) = CreateHandler();
        var content = MakeContent();
        var notification = SingleChange(content.Object, TreeChangeTypes.RefreshAll);

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()), Times.Never);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesAsync(It.IsAny<Guid>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithEmptyChanges_DoesNotCallAnyMethods()
    {
        var (handler, urlService, aliasService) = CreateHandler();
        var notification = new ContentTreeChangeNotification(
            Array.Empty<TreeChange<IContent>>(),
            new EventMessages());

        await handler.HandleAsync(notification, CancellationToken.None);

        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsAsync(It.IsAny<IEnumerable<IContent>>()), Times.Never);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesAsync(It.IsAny<Guid>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WithBothRefreshNodeAndBranch_CallsCorrectMethodsForEach()
    {
        var (handler, urlService, aliasService) = CreateHandler();
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
        aliasService.Verify(x => x.CreateOrUpdateAliasesAsync(nodeKey), Times.Once);
        urlService.Verify(x => x.CreateOrUpdateUrlSegmentsWithDescendantsAsync(branchKey), Times.Once);
        aliasService.Verify(x => x.CreateOrUpdateAliasesWithDescendantsAsync(branchKey), Times.Once);
    }
}
