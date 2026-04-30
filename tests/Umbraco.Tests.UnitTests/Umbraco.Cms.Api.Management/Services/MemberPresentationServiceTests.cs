using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Member;
using Umbraco.Cms.Api.Management.ViewModels.Member.Item;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

[TestFixture]
public class MemberPresentationServiceTests
{
    private Mock<IEntityService> _mockEntityService = null!;
    private Mock<IMemberEditingService> _mockMemberEditingService = null!;
    private Mock<IMemberPresentationFactory> _mockFactory = null!;
    private MemberPresentationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockEntityService = new Mock<IEntityService>();
        _mockMemberEditingService = new Mock<IMemberEditingService>();
        _mockFactory = new Mock<IMemberPresentationFactory>();

        _sut = new MemberPresentationService(
            _mockEntityService.Object,
            _mockMemberEditingService.Object,
            _mockFactory.Object);
    }

    [Test]
    public async Task CreateResponseModelByKeyAsync_Returns_Content_Member_When_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var member = new Mock<IMember>();
        var user = new Mock<IUser>();
        var expected = new MemberResponseModel { Id = id };

        _mockMemberEditingService.Setup(x => x.GetAsync(id)).ReturnsAsync(member.Object);
        _mockFactory.Setup(x => x.CreateResponseModelAsync(member.Object, user.Object)).ReturnsAsync(expected);

        // Act
        MemberResponseModel? result = await _sut.CreateResponseModelByKeyAsync(id, user.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(id, result!.Id);
        _mockMemberEditingService.Verify(x => x.GetExternalMemberAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task CreateResponseModelByKeyAsync_Falls_Back_To_External_Member_When_Content_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var externalMember = new ExternalMemberIdentity { Key = id, Email = "ext@test.com" };
        var user = new Mock<IUser>();
        var expected = new MemberResponseModel { Id = id, Kind = MemberKind.ExternalOnly };

        _mockMemberEditingService.Setup(x => x.GetAsync(id)).ReturnsAsync((IMember?)null);
        _mockMemberEditingService.Setup(x => x.GetExternalMemberAsync(id)).ReturnsAsync(externalMember);
        _mockFactory.Setup(x => x.CreateExternalMemberResponseModelAsync(externalMember)).ReturnsAsync(expected);

        // Act
        MemberResponseModel? result = await _sut.CreateResponseModelByKeyAsync(id, user.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(MemberKind.ExternalOnly, result!.Kind);
    }

    [Test]
    public async Task CreateResponseModelByKeyAsync_Returns_Null_When_Not_Found_In_Either_Store()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new Mock<IUser>();

        _mockMemberEditingService.Setup(x => x.GetAsync(id)).ReturnsAsync((IMember?)null);
        _mockMemberEditingService.Setup(x => x.GetExternalMemberAsync(id)).ReturnsAsync((ExternalMemberIdentity?)null);

        // Act
        MemberResponseModel? result = await _sut.CreateResponseModelByKeyAsync(id, user.Object);

        // Assert
        Assert.IsNull(result);
    }

    [Test]
    public async Task CreateItemResponseModelsAsync_Returns_Content_Members()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = CreateMockMemberEntitySlim(id);
        var expected = new MemberItemResponseModel { Id = id };

        _mockEntityService
            .Setup(x => x.GetAll(UmbracoObjectTypes.Member, It.Is<Guid[]>(a => a.Contains(id))))
            .Returns([entity.Object]);
        _mockFactory.Setup(x => x.CreateItemResponseModel(entity.Object)).Returns(expected);

        // Act
        IEnumerable<MemberItemResponseModel> results = await _sut.CreateItemResponseModelsAsync([id]);

        // Assert
        Assert.AreEqual(1, results.Count());
        Assert.AreEqual(id, results.First().Id);
    }

    [Test]
    public async Task CreateItemResponseModelsAsync_Resolves_External_Members_For_Unresolved_Ids()
    {
        // Arrange
        var contentId = Guid.NewGuid();
        var externalId = Guid.NewGuid();
        var contentEntity = CreateMockMemberEntitySlim(contentId);
        var externalMember = new ExternalMemberIdentity { Key = externalId };
        var contentItem = new MemberItemResponseModel { Id = contentId };
        var externalItem = new MemberItemResponseModel { Id = externalId, Kind = MemberKind.ExternalOnly };

        _mockEntityService
            .Setup(x => x.GetAll(UmbracoObjectTypes.Member, It.IsAny<Guid[]>()))
            .Returns(new[] { contentEntity.Object });
        _mockFactory.Setup(x => x.CreateItemResponseModel(contentEntity.Object)).Returns(contentItem);
        _mockMemberEditingService.Setup(x => x.GetExternalMemberAsync(externalId)).ReturnsAsync(externalMember);
        _mockFactory.Setup(x => x.CreateExternalMemberItemResponseModel(externalMember)).Returns(externalItem);

        // Act
        IEnumerable<MemberItemResponseModel> results =
            await _sut.CreateItemResponseModelsAsync(new HashSet<Guid> { contentId, externalId });

        // Assert
        Assert.AreEqual(2, results.Count());
        Assert.IsTrue(results.Any(r => r.Id == contentId));
        Assert.IsTrue(results.Any(r => r.Id == externalId && r.Kind == MemberKind.ExternalOnly));
    }

    [Test]
    public async Task CreateItemResponseModelsAsync_Skips_Unresolved_Ids_Not_In_External_Store()
    {
        // Arrange
        var unknownId = Guid.NewGuid();

        _mockEntityService
            .Setup(x => x.GetAll(UmbracoObjectTypes.Member, It.IsAny<Guid[]>()))
            .Returns(Enumerable.Empty<IEntitySlim>());
        _mockMemberEditingService.Setup(x => x.GetExternalMemberAsync(unknownId)).ReturnsAsync((ExternalMemberIdentity?)null);

        // Act
        IEnumerable<MemberItemResponseModel> results =
            await _sut.CreateItemResponseModelsAsync(new HashSet<Guid> { unknownId });

        // Assert
        Assert.IsEmpty(results);
    }

    [Test]
    public async Task CreateItemResponseModelsAsync_Returns_Empty_For_Empty_Ids()
    {
        // Act
        IEnumerable<MemberItemResponseModel> results =
            await _sut.CreateItemResponseModelsAsync(new HashSet<Guid>());

        // Assert
        Assert.IsEmpty(results);
        _mockEntityService.Verify(
            x => x.GetAll(UmbracoObjectTypes.Member, It.IsAny<Guid[]>()), Times.Once);
    }

    private static Mock<IMemberEntitySlim> CreateMockMemberEntitySlim(Guid key)
    {
        var entity = new Mock<IMemberEntitySlim>();
        entity.Setup(x => x.Key).Returns(key);
        return entity;
    }
}
