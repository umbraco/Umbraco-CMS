using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Querying.Selectors;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi.Selectors;

[TestFixture]
public class ChildrenSelectorTests
{
    [TestCase(null)]
    [TestCase(1234)]
    public void Can_Build_Selector_Option_For_Path(int? documentStartNodeId)
    {
        var documentKey = Guid.NewGuid();

        var documentUrlServiceMock = new Mock<IDocumentUrlService>();
        documentUrlServiceMock
            .Setup(m => m.GetDocumentKeyByRoute(
                "/some/where",
                It.IsAny<string?>(),
                documentStartNodeId,
                false))
            .Returns(documentKey);

        var requestRoutingServiceMock = new Mock<IRequestRoutingService>();
        requestRoutingServiceMock.Setup(m => m.GetContentRoute("/some/where")).Returns($"{documentStartNodeId}/some/where");

        var subject = new ChildrenSelector(
            requestRoutingServiceMock.Object,
            Mock.Of<IRequestPreviewService>(),
            new ApiDocumentUrlService(documentUrlServiceMock.Object),
            Mock.Of<IVariationContextAccessor>());

        var result = subject.BuildSelectorOption("children:/some/where");
        Assert.AreEqual(1, result.Values.Length);
        Assert.AreEqual(documentKey.ToString("D"), result.Values[0]);
    }

    [Test]
    public void Can_Build_Selector_Option_For_Id()
    {
        var documentKey = Guid.NewGuid();

        var subject = new ChildrenSelector(
            Mock.Of<IRequestRoutingService>(),
            Mock.Of<IRequestPreviewService>(),
            Mock.Of<IApiDocumentUrlService>(),
            Mock.Of<IVariationContextAccessor>());

        var result = subject.BuildSelectorOption($"children:{documentKey:D}");
        Assert.AreEqual(1, result.Values.Length);
        Assert.AreEqual(documentKey.ToString("D"), result.Values[0]);
    }
}
