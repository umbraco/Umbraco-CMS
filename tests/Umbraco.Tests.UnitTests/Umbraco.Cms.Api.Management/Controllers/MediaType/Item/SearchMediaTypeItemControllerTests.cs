using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MediaType.Item;
using Umbraco.Cms.Api.Management.ViewModels.MediaType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.MediaType.Item;

[TestFixture]
public class SearchMediaTypeItemControllerTests
{
    private Mock<IEntitySearchService> _entitySearchService = null!;
    private Mock<IMediaTypeService> _mediaTypeService = null!;
    private Mock<IUmbracoMapper> _mapper = null!;
    private SearchMediaTypeItemController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _entitySearchService = new Mock<IEntitySearchService>();
        _mediaTypeService = new Mock<IMediaTypeService>();
        _mapper = new Mock<IUmbracoMapper>();
        _controller = new SearchMediaTypeItemController(
            _entitySearchService.Object,
            _mediaTypeService.Object,
            _mapper.Object);
    }

    [Test]
    public async Task Search_Media_Type_Returns_Items_Ordered_By_Search_Result_Order()
    {
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var keyC = Guid.NewGuid();

        // Search returns keys in order [A, B, C]
        _entitySearchService
            .Setup(x => x.Search(UmbracoObjectTypes.MediaType, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new PagedModel<IEntitySlim>
            {
                Items = [
                    new EntitySlim { Key = keyA },
                    new EntitySlim { Key = keyB },
                    new EntitySlim { Key = keyC },
                ],
                Total = 3,
            });

        // Service returns media types in scrambled order [C, A, B]
        IMediaType mediaTypeC = Mock.Of<IMediaType>(x => x.Key == keyC);
        IMediaType mediaTypeA = Mock.Of<IMediaType>(x => x.Key == keyA);
        IMediaType mediaTypeB = Mock.Of<IMediaType>(x => x.Key == keyB);
        _mediaTypeService
            .Setup(x => x.GetMany(It.IsAny<IEnumerable<Guid>>()))
            .Returns(new[] { mediaTypeC, mediaTypeA, mediaTypeB });

        _mapper
            .Setup(x => x.MapEnumerable<IMediaType, MediaTypeItemResponseModel>(It.IsAny<IEnumerable<IMediaType>>()))
            .Returns<IEnumerable<IMediaType>>(entities =>
                entities.Select(e => new MediaTypeItemResponseModel { Id = e.Key }).ToList());

        IActionResult result = await _controller.Search(CancellationToken.None, "test");

        OkObjectResult? okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var pagedModel = okResult!.Value as PagedModel<MediaTypeItemResponseModel>;
        Assert.That(pagedModel, Is.Not.Null);
        List<MediaTypeItemResponseModel> items = pagedModel!.Items.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Id, Is.EqualTo(keyA));
            Assert.That(items[1].Id, Is.EqualTo(keyB));
            Assert.That(items[2].Id, Is.EqualTo(keyC));
        });
    }
}
