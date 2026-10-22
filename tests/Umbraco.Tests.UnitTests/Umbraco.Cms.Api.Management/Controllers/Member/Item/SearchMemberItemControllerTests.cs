using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Member.Item;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.Member.Item;

[TestFixture]
public class SearchMemberItemControllerTests
{
    private Mock<IIndexedEntitySearchService> _indexedEntitySearchService = null!;
    private Mock<IMemberPresentationService> _memberPresentationService = null!;
    private SearchMemberItemController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _indexedEntitySearchService = new Mock<IIndexedEntitySearchService>();
        _memberPresentationService = new Mock<IMemberPresentationService>();
        _controller = new SearchMemberItemController(
            _indexedEntitySearchService.Object,
            _memberPresentationService.Object);
    }

    [Test]
    public async Task Search_Member_Returns_Every_Resolvable_Member_In_Search_Result_Order()
    {
        var contentMemberKey = Guid.NewGuid();
        var externalMemberKey = Guid.NewGuid();
        var otherContentMemberKey = Guid.NewGuid();
        Guid[] searchResultKeys = [externalMemberKey, contentMemberKey, otherContentMemberKey];

        _indexedEntitySearchService
            .Setup(x => x.SearchKeysAsync(
                UmbracoObjectTypes.Member,
                "test",
                It.IsAny<Guid?>(),
                It.IsAny<IEnumerable<Guid>?>(),
                It.IsAny<bool?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new PagedModel<Guid> { Items = searchResultKeys, Total = 42 });

        // the presentation service resolves content-based members before external members
        _memberPresentationService
            .Setup(x => x.CreateItemResponseModelsAsync(It.Is<HashSet<Guid>>(keys => keys.SetEquals(searchResultKeys))))
            .ReturnsAsync(
            [
                new MemberItemResponseModel { Id = contentMemberKey, Kind = MemberKind.Default },
                new MemberItemResponseModel { Id = otherContentMemberKey, Kind = MemberKind.Default },
                new MemberItemResponseModel { Id = externalMemberKey, Kind = MemberKind.ExternalOnly },
            ]);

        IActionResult actionResult = await _controller.SearchWithAllowedTypes(CancellationToken.None, "test");

        var okResult = actionResult as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var result = okResult!.Value as PagedModel<MemberItemResponseModel>;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Items.Select(item => item.Id), Is.EqualTo(searchResultKeys));
            Assert.That(result.Total, Is.EqualTo(42));
        });
    }

    [TestCase(0)]
    [TestCase(42)]
    public async Task Search_Member_Without_Search_Result_Keys_Returns_No_Members(long total)
    {
        _indexedEntitySearchService
            .Setup(x => x.SearchKeysAsync(
                UmbracoObjectTypes.Member,
                "test",
                It.IsAny<Guid?>(),
                It.IsAny<IEnumerable<Guid>?>(),
                It.IsAny<bool?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new PagedModel<Guid> { Items = [], Total = total });

        // resolving an empty set of keys yields every member, as the underlying entity lookup treats no keys as "all"
        _memberPresentationService
            .Setup(x => x.CreateItemResponseModelsAsync(It.Is<HashSet<Guid>>(keys => keys.Count == 0)))
            .ReturnsAsync([new MemberItemResponseModel { Id = Guid.NewGuid(), Kind = MemberKind.Default }]);

        IActionResult actionResult = await _controller.SearchWithAllowedTypes(CancellationToken.None, "test", skip: 100);

        var okResult = actionResult as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var result = okResult!.Value as PagedModel<MemberItemResponseModel>;
        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result!.Items, Is.Empty);
            Assert.That(result.Total, Is.EqualTo(total));
        });
    }
}
