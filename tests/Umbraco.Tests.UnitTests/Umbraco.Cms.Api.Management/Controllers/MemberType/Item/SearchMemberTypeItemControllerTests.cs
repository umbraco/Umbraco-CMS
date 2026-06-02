using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.MemberType.Item;
using Umbraco.Cms.Api.Management.ViewModels.MemberType.Item;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.MemberType.Item;

[TestFixture]
public class SearchMemberTypeItemControllerTests
{
    private Mock<IEntitySearchService> _entitySearchService = null!;
    private Mock<IMemberTypeService> _memberTypeService = null!;
    private Mock<IUmbracoMapper> _mapper = null!;
    private SearchMemberTypeItemController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _entitySearchService = new Mock<IEntitySearchService>();
        _memberTypeService = new Mock<IMemberTypeService>();
        _mapper = new Mock<IUmbracoMapper>();
        _controller = new SearchMemberTypeItemController(
            _entitySearchService.Object,
            _memberTypeService.Object,
            _mapper.Object);
    }

    [Test]
    public async Task Search_Member_Type_Returns_Items_Ordered_By_Search_Result_Order()
    {
        var keyA = Guid.NewGuid();
        var keyB = Guid.NewGuid();
        var keyC = Guid.NewGuid();

        // Search returns keys in order [A, B, C]
        _entitySearchService
            .Setup(x => x.Search(UmbracoObjectTypes.MemberType, It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(new PagedModel<IEntitySlim>
            {
                Items = [
                    new EntitySlim { Key = keyA },
                    new EntitySlim { Key = keyB },
                    new EntitySlim { Key = keyC },
                ],
                Total = 3,
            });

        // Service returns member types in scrambled order [C, A, B]
        IMemberType memberTypeC = Mock.Of<IMemberType>(x => x.Key == keyC);
        IMemberType memberTypeA = Mock.Of<IMemberType>(x => x.Key == keyA);
        IMemberType memberTypeB = Mock.Of<IMemberType>(x => x.Key == keyB);
        _memberTypeService
            .Setup(x => x.GetMany(It.IsAny<IEnumerable<Guid>>()))
            .Returns(new[] { memberTypeC, memberTypeA, memberTypeB });

        _mapper
            .Setup(x => x.MapEnumerable<IMemberType, MemberTypeItemResponseModel>(It.IsAny<IEnumerable<IMemberType>>()))
            .Returns<IEnumerable<IMemberType>>(entities =>
                entities.Select(e => new MemberTypeItemResponseModel { Id = e.Key }).ToList());

        IActionResult result = await _controller.Search(CancellationToken.None, "test");

        OkObjectResult? okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var pagedModel = okResult!.Value as PagedModel<MemberTypeItemResponseModel>;
        Assert.That(pagedModel, Is.Not.Null);
        List<MemberTypeItemResponseModel> items = pagedModel!.Items.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Id, Is.EqualTo(keyA));
            Assert.That(items[1].Id, Is.EqualTo(keyB));
            Assert.That(items[2].Id, Is.EqualTo(keyC));
        });
    }
}
