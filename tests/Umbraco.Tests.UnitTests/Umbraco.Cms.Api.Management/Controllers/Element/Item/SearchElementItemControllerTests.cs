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
    private Mock<IEntitySearchService> _entitySearchService = null!;
    private Mock<IEntityService> _entityService = null!;
    private Mock<IElementPresentationFactory> _elementPresentationFactory = null!;
    private SearchElementItemController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _entitySearchService = new Mock<IEntitySearchService>();
        _entityService = new Mock<IEntityService>();
        _elementPresentationFactory = new Mock<IElementPresentationFactory>();
        _controller = new SearchElementItemController(
            _entitySearchService.Object,
            _entityService.Object,
            _elementPresentationFactory.Object);
    }

    [Test]
    public async Task Search_Element_Returns_Items_Ordered_By_Search_Result_Order()
    {
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var keyC = Guid.NewGuid();

        // Search returns keys in order [A, B, C]
        _entitySearchService
            .Setup(x => x.Search(UmbracoObjectTypes.Element, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new PagedModel<IEntitySlim>
            {
                Items = [
                    new EntitySlim { Key = keyA },
                    new EntitySlim { Key = keyB },
                    new EntitySlim { Key = keyC },
                ],
                Total = 3,
            });

        // Entity service returns elements in scrambled order [C, A, B]
        IElementEntitySlim elementC = Mock.Of<IElementEntitySlim>(x => x.Key == keyC);
        IElementEntitySlim elementA = Mock.Of<IElementEntitySlim>(x => x.Key == keyA);
        IElementEntitySlim elementB = Mock.Of<IElementEntitySlim>(x => x.Key == keyB);
        _entityService
            .Setup(x => x.GetAll(UmbracoObjectTypes.Element, It.IsAny<Guid[]>()))
            .Returns(new IEntitySlim[] { elementC, elementA, elementB });

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
            Assert.That(items[0].Id, Is.EqualTo(keyA));
            Assert.That(items[1].Id, Is.EqualTo(keyB));
            Assert.That(items[2].Id, Is.EqualTo(keyC));
        });
    }
}
