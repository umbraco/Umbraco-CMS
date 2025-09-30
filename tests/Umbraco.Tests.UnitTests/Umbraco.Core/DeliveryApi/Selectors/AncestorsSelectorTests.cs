using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Delivery.Querying.Selectors;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi.Selectors;

[TestFixture]
public class AncestorsSelectorTests
{
    private readonly Guid _documentKey = Guid.NewGuid();
    private IDocumentNavigationQueryService _documentNavigationQueryService;

    [SetUp]
    public void SetUp()
    {
        IEnumerable<Guid> ancestorKeys =
        [
            new("863e10d5-b0f8-421d-902d-5e4d1bd8e780"),
            new("11fc9bdc-8366-4a6b-a9c2-6b8b2717c4b8")
        ];
        var documentNavigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        documentNavigationQueryServiceMock
            .Setup(m => m.TryGetAncestorsKeys(_documentKey, out ancestorKeys))
            .Returns(true);
        _documentNavigationQueryService = documentNavigationQueryServiceMock.Object;
    }

    [TestCase(null)]
    [TestCase(1234)]
    public void Can_Build_Selector_Option_For_Path(int? documentStartNodeId)
    {
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();
        documentUrlServiceMock
            .Setup(m => m.GetDocumentKeyByRoute(
                "/some/where",
                It.IsAny<string?>(),
                documentStartNodeId,
                false))
            .Returns(_documentKey);

        var requestRoutingServiceMock = new Mock<IRequestRoutingService>();
        requestRoutingServiceMock.Setup(m => m.GetContentRoute("/some/where")).Returns($"{documentStartNodeId}/some/where");

        var subject = new AncestorsSelector(
            requestRoutingServiceMock.Object,
            Mock.Of<IRequestPreviewService>(),
            new ApiDocumentUrlService(documentUrlServiceMock.Object),
            Mock.Of<IVariationContextAccessor>(),
            _documentNavigationQueryService);

        var result = subject.BuildSelectorOption("ancestors:/some/where");
        Assert.AreEqual(2, result.Values.Length);
        Assert.AreEqual("863e10d5-b0f8-421d-902d-5e4d1bd8e780", result.Values[0]);
        Assert.AreEqual("11fc9bdc-8366-4a6b-a9c2-6b8b2717c4b8", result.Values[1]);
    }

    [Test]
    public void Can_Build_Selector_Option_For_Id()
    {
        var subject = new AncestorsSelector(
            Mock.Of<IRequestRoutingService>(),
            Mock.Of<IRequestPreviewService>(),
            Mock.Of<IApiDocumentUrlService>(),
            Mock.Of<IVariationContextAccessor>(),
            _documentNavigationQueryService);

        var result = subject.BuildSelectorOption($"ancestors:{_documentKey:D}");
        Assert.AreEqual(2, result.Values.Length);
        Assert.AreEqual("863e10d5-b0f8-421d-902d-5e4d1bd8e780", result.Values[0]);
        Assert.AreEqual("11fc9bdc-8366-4a6b-a9c2-6b8b2717c4b8", result.Values[1]);
    }
}
