using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.DataType.Item;
using Umbraco.Cms.Api.Management.ViewModels.DataType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.DataType.Item;

[TestFixture]
public class SearchDataTypeItemControllerTests
{
    private Mock<IEntitySearchService> _entitySearchService = null!;
    private Mock<IDataTypeService> _dataTypeService = null!;
    private Mock<IUmbracoMapper> _mapper = null!;
    private SearchDataTypeItemController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _entitySearchService = new Mock<IEntitySearchService>();
        _dataTypeService = new Mock<IDataTypeService>();
        _mapper = new Mock<IUmbracoMapper>();
        _controller = new SearchDataTypeItemController(
            _entitySearchService.Object,
            _dataTypeService.Object,
            _mapper.Object);
    }

    [Test]
    public async Task Search_Data_Type_Returns_Items_Ordered_By_Search_Result_Order()
    {
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var keyC = Guid.NewGuid();

        // Search returns keys in order [A, B, C]
        _entitySearchService
            .Setup(x => x.Search(UmbracoObjectTypes.DataType, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new PagedModel<IEntitySlim>
            {
                Items = [
                    new EntitySlim { Key = keyA },
                    new EntitySlim { Key = keyB },
                    new EntitySlim { Key = keyC },
                ],
                Total = 3,
            });

        // Service returns data types in scrambled order [C, A, B]
        IDataType dataTypeC = Mock.Of<IDataType>(x => x.Key == keyC);
        IDataType dataTypeA = Mock.Of<IDataType>(x => x.Key == keyA);
        IDataType dataTypeB = Mock.Of<IDataType>(x => x.Key == keyB);
        _dataTypeService
            .Setup(x => x.GetAllAsync(It.IsAny<Guid[]>()))
            .ReturnsAsync(new[] { dataTypeC, dataTypeA, dataTypeB });

        // Mapper propagates each entity's key into the response model Id so the result reflects ordering
        _mapper
            .Setup(x => x.MapEnumerable<IDataType, DataTypeItemResponseModel>(It.IsAny<IEnumerable<IDataType>>()))
            .Returns<IEnumerable<IDataType>>(entities =>
                entities.Select(e => new DataTypeItemResponseModel { Id = e.Key }).ToList());

        IActionResult result = await _controller.Search(CancellationToken.None, "test");

        OkObjectResult? okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var pagedModel = okResult!.Value as PagedModel<DataTypeItemResponseModel>;
        Assert.That(pagedModel, Is.Not.Null);
        List<DataTypeItemResponseModel> items = pagedModel!.Items.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Id, Is.EqualTo(keyA));
            Assert.That(items[1].Id, Is.EqualTo(keyB));
            Assert.That(items[2].Id, Is.EqualTo(keyC));
        });
    }
}
