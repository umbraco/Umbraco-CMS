using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Template.Item;
using Umbraco.Cms.Api.Management.ViewModels.Template.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.Template.Item;

[TestFixture]
public class SearchTemplateItemControllerTests
{
    private Mock<IEntitySearchService> _entitySearchService = null!;
    private Mock<ITemplateService> _templateService = null!;
    private Mock<IUmbracoMapper> _mapper = null!;
    private SearchTemplateItemController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _entitySearchService = new Mock<IEntitySearchService>();
        _templateService = new Mock<ITemplateService>();
        _mapper = new Mock<IUmbracoMapper>();
        _controller = new SearchTemplateItemController(
            _entitySearchService.Object,
            _templateService.Object,
            _mapper.Object);
    }

    [Test]
    public async Task Search_Template_Item_Returns_Items_Ordered_By_Search_Result_Order()
    {
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var keyC = Guid.NewGuid();

        // Search returns keys in order [A, B, C]
        _entitySearchService
            .Setup(x => x.Search(UmbracoObjectTypes.Template, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new PagedModel<IEntitySlim>
            {
                Items = [
                    new EntitySlim { Key = keyA },
                    new EntitySlim { Key = keyB },
                    new EntitySlim { Key = keyC },
                ],
                Total = 3,
            });

        // Service returns templates in scrambled order [C, A, B]
        ITemplate templateC = Mock.Of<ITemplate>(x => x.Key == keyC);
        ITemplate templateA = Mock.Of<ITemplate>(x => x.Key == keyA);
        ITemplate templateB = Mock.Of<ITemplate>(x => x.Key == keyB);
        _templateService
            .Setup(x => x.GetAllAsync(It.IsAny<Guid[]>()))
            .ReturnsAsync(new[] { templateC, templateA, templateB });

        _mapper
            .Setup(x => x.MapEnumerable<ITemplate, TemplateItemResponseModel>(It.IsAny<IEnumerable<ITemplate>>()))
            .Returns<IEnumerable<ITemplate>>(entities =>
                entities.Select(e => new TemplateItemResponseModel { Alias = e.Alias, Id = e.Key }).ToList());

        IActionResult result = await _controller.Search(CancellationToken.None, "test");

        OkObjectResult? okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var pagedModel = okResult!.Value as PagedModel<TemplateItemResponseModel>;
        Assert.That(pagedModel, Is.Not.Null);
        List<TemplateItemResponseModel> items = pagedModel!.Items.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Id, Is.EqualTo(keyA));
            Assert.That(items[1].Id, Is.EqualTo(keyB));
            Assert.That(items[2].Id, Is.EqualTo(keyC));
        });
    }
}
