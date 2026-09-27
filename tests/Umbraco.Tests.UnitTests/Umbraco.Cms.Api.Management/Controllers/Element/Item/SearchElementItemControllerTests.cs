using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Element.Item;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Element.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.Element.Item;

[TestFixture]
public class SearchElementItemControllerTests
{
    private Mock<IIndexedEntitySearchService> _indexedEntitySearchService = null!;
    private Mock<IElementPresentationFactory> _elementPresentationFactory = null!;
    private SearchElementItemController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _indexedEntitySearchService = new Mock<IIndexedEntitySearchService>();
        _elementPresentationFactory = new Mock<IElementPresentationFactory>();
        _controller = new SearchElementItemController(
            _indexedEntitySearchService.Object,
            _elementPresentationFactory.Object);
    }

    [Test]
    public async Task Search_Element_Returns_Items_In_Search_Result_Order()
    {
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var keyC = Guid.NewGuid();

        // Deliberately not in key or creation order, so the controller must preserve the search result order
        IElementEntitySlim elementC = Mock.Of<IElementEntitySlim>(x => x.Key == keyC);
        IElementEntitySlim elementA = Mock.Of<IElementEntitySlim>(x => x.Key == keyA);
        IElementEntitySlim elementB = Mock.Of<IElementEntitySlim>(x => x.Key == keyB);
        _indexedEntitySearchService
            .Setup(x => x.SearchAsync(
                UmbracoObjectTypes.Element,
                "test",
                It.IsAny<Guid?>(),
                It.IsAny<IEnumerable<Guid>?>(),
                It.IsAny<bool?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new PagedModel<IEntitySlim>
            {
                Items = [elementC, elementA, elementB],
                Total = 42,
            });

        _elementPresentationFactory
            .Setup(x => x.CreateItemResponseModelAsync(It.IsAny<IElementEntitySlim>()))
            .ReturnsAsync((IElementEntitySlim entity) => new ElementItemResponseModel { Id = entity.Key });

        IActionResult result = await _controller.Search(CancellationToken.None, "test");

        OkObjectResult? okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var pagedModel = okResult!.Value as PagedModel<ElementItemResponseModel>;
        Assert.That(pagedModel, Is.Not.Null);
        List<ElementItemResponseModel> items = pagedModel!.Items.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(pagedModel.Total, Is.EqualTo(42));
            Assert.That(items.Select(item => item.Id), Is.EqualTo(new[] { keyC, keyA, keyB }));
        });
    }
}
