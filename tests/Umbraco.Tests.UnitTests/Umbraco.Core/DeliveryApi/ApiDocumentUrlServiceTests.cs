using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ApiDocumentUrlServiceTests
{
    [Test]
    public void Can_Resolve_Document_Key_With_Start_Node()
    {
        var documentKey = Guid.NewGuid();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();
        documentUrlServiceMock
            .Setup(m => m.GetDocumentKeyByRoute(
                "/some/where",
                It.IsAny<string?>(),
                1234,
                false))
            .Returns(documentKey);

        var apiDocumentUrlService = new ApiDocumentUrlService(documentUrlServiceMock.Object);
        var result = apiDocumentUrlService.GetDocumentKeyByRoute("1234/some/where", null, false);
        Assert.AreEqual(documentKey, result);
    }

    [Test]
    public void Can_Resolve_Document_Key_Without_Start_Node()
    {
        var documentKey = Guid.NewGuid();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();
        documentUrlServiceMock
            .Setup(m => m.GetDocumentKeyByRoute(
                "/some/where",
                It.IsAny<string?>(),
                null,
                false))
            .Returns(documentKey);

        var apiDocumentUrlService = new ApiDocumentUrlService(documentUrlServiceMock.Object);
        var result = apiDocumentUrlService.GetDocumentKeyByRoute("/some/where", null, false);
        Assert.AreEqual(documentKey, result);
    }
}
