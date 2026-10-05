using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class DocumentUrlAliasContentRefreshNotificationHandlerTests
{
    private static (DocumentUrlAliasContentRefreshNotificationHandler Handler, Mock<IDocumentUrlAliasService> AliasService) CreateHandler(bool isInitialized)
    {
        var aliasServiceMock = new Mock<IDocumentUrlAliasService>();
        aliasServiceMock.Setup(x => x.IsInitialized).Returns(isInitialized);
        aliasServiceMock.Setup(x => x.PersistAliasesAsync(It.IsAny<IContent>())).Returns(Task.CompletedTask);

        return (new DocumentUrlAliasContentRefreshNotificationHandler(aliasServiceMock.Object), aliasServiceMock);
    }

    [Test]
    public async Task HandleAsync_WhenInitialized_PersistsTheAliasesOfTheRefreshedDocument()
    {
        var (handler, aliasService) = CreateHandler(isInitialized: true);
        IContent content = Mock.Of<IContent>();

        await handler.HandleAsync(new ContentRefreshNotification(content, new EventMessages()), CancellationToken.None);

        aliasService.Verify(x => x.PersistAliasesAsync(content), Times.Once);
        aliasService.Verify(x => x.CreateOrUpdateAliasesAsync(It.IsAny<Guid>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesWithDescendantsAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task HandleAsync_WhenNotInitialized_DoesNotTouchTheAliasService()
    {
        var (handler, aliasService) = CreateHandler(isInitialized: false);

        await handler.HandleAsync(new ContentRefreshNotification(Mock.Of<IContent>(), new EventMessages()), CancellationToken.None);

        aliasService.Verify(x => x.PersistAliasesAsync(It.IsAny<IContent>()), Times.Never);
        aliasService.Verify(x => x.CreateOrUpdateAliasesAsync(It.IsAny<Guid>()), Times.Never);
    }
}
