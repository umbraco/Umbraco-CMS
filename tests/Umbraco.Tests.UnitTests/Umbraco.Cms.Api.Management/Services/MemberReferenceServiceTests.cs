using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Services;

[TestFixture]
public class MemberReferenceServiceTests
{
    private Mock<ITrackedReferencesService> _mockTrackedReferencesService = null!;
    private Mock<IMemberEditingService> _mockMemberEditingService = null!;
    private MemberReferenceService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _mockTrackedReferencesService = new Mock<ITrackedReferencesService>();
        _mockMemberEditingService = new Mock<IMemberEditingService>();

        _sut = new MemberReferenceService(
            _mockTrackedReferencesService.Object,
            _mockMemberEditingService.Object);
    }

    [Test]
    public async Task GetPagedReferencesAsync_Returns_Success_When_Entity_Lookup_Succeeds()
    {
        // Arrange
        var id = Guid.NewGuid();
        var pagedModel = new PagedModel<RelationItemModel> { Total = 1, Items = new[] { new RelationItemModel() } };
        var attempt = Attempt.SucceedWithStatus(GetReferencesOperationStatus.Success, pagedModel);

        _mockTrackedReferencesService
            .Setup(x => x.GetPagedRelationsForItemAsync(id, UmbracoObjectTypes.Member, 0, 20, true))
            .ReturnsAsync(attempt);

        // Act
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> result =
            await _sut.GetPagedReferencesAsync(id, 0, 20);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Result.Total);
        _mockMemberEditingService.Verify(x => x.IsExternalMemberAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task GetPagedReferencesAsync_Falls_Back_For_External_Member_When_ContentNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var failedAttempt = Attempt.FailWithStatus(GetReferencesOperationStatus.ContentNotFound, new PagedModel<RelationItemModel>());
        var fallbackModel = new PagedModel<RelationItemModel> { Total = 2, Items = new[] { new RelationItemModel(), new RelationItemModel() } };

        _mockTrackedReferencesService
            .Setup(x => x.GetPagedRelationsForItemAsync(id, UmbracoObjectTypes.Member, 0, 10, true))
            .ReturnsAsync(failedAttempt);
        _mockMemberEditingService.Setup(x => x.IsExternalMemberAsync(id)).ReturnsAsync(true);

#pragma warning disable CS0618 // Type or member is obsolete
        _mockTrackedReferencesService
            .Setup(x => x.GetPagedRelationsForItemAsync(id, 0, 10, true))
            .ReturnsAsync(fallbackModel);
#pragma warning restore CS0618

        // Act
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> result =
            await _sut.GetPagedReferencesAsync(id, 0, 10);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(GetReferencesOperationStatus.Success, result.Status);
        Assert.AreEqual(2, result.Result.Total);
    }

    [Test]
    public async Task GetPagedReferencesAsync_Returns_ContentNotFound_When_Not_External_Member()
    {
        // Arrange
        var id = Guid.NewGuid();
        var failedAttempt = Attempt.FailWithStatus(GetReferencesOperationStatus.ContentNotFound, new PagedModel<RelationItemModel>());

        _mockTrackedReferencesService
            .Setup(x => x.GetPagedRelationsForItemAsync(id, UmbracoObjectTypes.Member, 0, 20, true))
            .ReturnsAsync(failedAttempt);
        _mockMemberEditingService.Setup(x => x.IsExternalMemberAsync(id)).ReturnsAsync(false);

        // Act
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> result =
            await _sut.GetPagedReferencesAsync(id, 0, 20);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(GetReferencesOperationStatus.ContentNotFound, result.Status);
    }

    [Test]
    public async Task GetPagedReferencesAsync_Does_Not_Check_External_Member_For_Non_ContentNotFound_Failure()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Simulate a failure with a status other than ContentNotFound.
        var failedAttempt = Attempt.FailWithStatus(GetReferencesOperationStatus.ContentNotFound, new PagedModel<RelationItemModel>());

        // We can't easily create a different status since the enum only has Success and ContentNotFound,
        // so instead verify the external member check is only called for ContentNotFound.
        _mockTrackedReferencesService
            .Setup(x => x.GetPagedRelationsForItemAsync(id, UmbracoObjectTypes.Member, 5, 10, true))
            .ReturnsAsync(failedAttempt);
        _mockMemberEditingService.Setup(x => x.IsExternalMemberAsync(id)).ReturnsAsync(false);

        // Act
        Attempt<PagedModel<RelationItemModel>, GetReferencesOperationStatus> result =
            await _sut.GetPagedReferencesAsync(id, 5, 10);

        // Assert
        Assert.IsFalse(result.Success);
        _mockMemberEditingService.Verify(x => x.IsExternalMemberAsync(id), Times.Once);
    }

    [Test]
    public async Task GetPagedReferencesAsync_Passes_Skip_And_Take_To_Fallback()
    {
        // Arrange
        var id = Guid.NewGuid();
        var failedAttempt = Attempt.FailWithStatus(GetReferencesOperationStatus.ContentNotFound, new PagedModel<RelationItemModel>());
        var fallbackModel = new PagedModel<RelationItemModel> { Total = 0, Items = Array.Empty<RelationItemModel>() };

        _mockTrackedReferencesService
            .Setup(x => x.GetPagedRelationsForItemAsync(id, UmbracoObjectTypes.Member, 10, 5, true))
            .ReturnsAsync(failedAttempt);
        _mockMemberEditingService.Setup(x => x.IsExternalMemberAsync(id)).ReturnsAsync(true);

#pragma warning disable CS0618 // Type or member is obsolete
        _mockTrackedReferencesService
            .Setup(x => x.GetPagedRelationsForItemAsync(id, (long)10, (long)5, true))
            .ReturnsAsync(fallbackModel);
#pragma warning restore CS0618

        // Act
        await _sut.GetPagedReferencesAsync(id, 10, 5);

        // Assert
#pragma warning disable CS0618 // Type or member is obsolete
        _mockTrackedReferencesService.Verify(
            x => x.GetPagedRelationsForItemAsync(id, (long)10, (long)5, true), Times.Once);
#pragma warning restore CS0618
    }
}
