using System.Data;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

/// <summary>
/// Provides unit tests to verify the behavior and functionality of the <see cref="AuditService"/> class.
/// </summary>
[TestFixture]
public class AuditServiceTests
{
    private IAuditService _auditService;
    private Mock<ICoreScopeProvider> _scopeProviderMock;
    private Mock<IAuditRepository> _auditRepositoryMock;
    private Mock<IEntityService> _entityServiceMock;
    private Mock<IUserIdKeyResolver> _userIdKeyResolverMock;

    /// <summary>
    /// Sets up the necessary mocks and initializes the AuditService before each test.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _scopeProviderMock = new Mock<ICoreScopeProvider>(MockBehavior.Strict);
        _auditRepositoryMock = new Mock<IAuditRepository>(MockBehavior.Strict);
        _entityServiceMock = new Mock<IEntityService>(MockBehavior.Strict);
        _userIdKeyResolverMock = new Mock<IUserIdKeyResolver>(MockBehavior.Strict);

        _auditService = new AuditService(
            _scopeProviderMock.Object,
            Mock.Of<ILoggerFactory>(MockBehavior.Strict),
            Mock.Of<IEventMessagesFactory>(MockBehavior.Strict),
            _auditRepositoryMock.Object,
            _userIdKeyResolverMock.Object,
            _entityServiceMock.Object);
    }

    /// <summary>
    /// Verifies that <see cref="AuditService.AddAsync"/> invokes the audit repository with the expected values for each parameter.
    /// </summary>
    /// <param name="type">The <see cref="AuditType"/> to be audited.</param>
    /// <param name="objectId">The identifier of the object being audited.</param>
    /// <param name="entityType">The type of the entity being audited, or <c>null</c> if not specified.</param>
    /// <param name="comment">An optional comment for the audit entry, or <c>null</c> if not specified.</param>
    /// <param name="parameters">Optional parameters for the audit entry, or <c>null</c> if not specified.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [TestCase(AuditType.Publish, 33, null, null, null)]
    [TestCase(AuditType.Copy, 1, "entityType", "comment", "parameters")]
    public async Task AddAsync_Calls_Repository_With_Correct_Values(AuditType type, int objectId, string? entityType, string? comment, string? parameters = null)
    {
        SetupScopeProviderMock();

        _auditRepositoryMock.Setup(x => x.Save(It.IsAny<IAuditItem>()))
            .Callback<IAuditItem>(item =>
            {
                Assert.AreEqual(type, item.AuditType);
                Assert.AreEqual(Constants.Security.SuperUserId, item.UserId);
                Assert.AreEqual(objectId, item.Id);
                Assert.AreEqual(entityType, item.EntityType);
                Assert.AreEqual(comment, item.Comment);
                Assert.AreEqual(parameters, item.Parameters);
            });

        _userIdKeyResolverMock.Setup(x => x.TryGetAsync(Constants.Security.SuperUserKey))
            .ReturnsAsync(Attempt.Succeed(Constants.Security.SuperUserId));

        var result = await _auditService.AddAsync(
            type,
            Constants.Security.SuperUserKey,
            objectId,
            entityType,
            comment,
            parameters);
        _auditRepositoryMock.Verify(x => x.Save(It.IsAny<IAuditItem>()), Times.Once);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(AuditLogOperationStatus.Success, result.Result);
    }

    /// <summary>
    /// Tests that AddAsync does not succeed when a non-existing user is provided.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task AddAsync_Does_Not_Succeed_When_Non_Existing_User_Is_Provided()
    {
        _userIdKeyResolverMock.Setup(x => x.TryGetAsync(It.IsAny<Guid>())).ReturnsAsync(Attempt.Fail<int>());

        var result = await _auditService.AddAsync(
            AuditType.Publish,
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            1,
            "entityType",
            "comment",
            "parameters");
        Assert.IsFalse(result.Success);
        Assert.AreEqual(AuditLogOperationStatus.UserNotFound, result.Result);
    }

    /// <summary>
    /// Tests that GetItemsAsync throws an ArgumentOutOfRangeException when invalid pagination arguments are provided.
    /// </summary>
    [Test]
    public void GetItemsAsync_Throws_When_Invalid_Pagination_Arguments_Are_Provided()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsAsync(-1, 10), "Skip must be greater than or equal to 0.");
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsAsync(0, -1), "Take must be greater than 0.");
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsAsync(0, 0), "Take must be greater than 0.");
    }

    /// <summary>
    /// Tests that GetItemsAsync returns the correct total number of records and the correct count of items.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetItemsAsync_Returns_Correct_Total_And_Item_Count()
    {
        SetupScopeProviderMock();

        Fixture fixture = new Fixture();
        long totalRecords = 12;
        _auditRepositoryMock.Setup(x => x.GetPagedResultsByQuery(
                It.IsAny<IQuery<IAuditItem>>(),
                2,
                5,
                out totalRecords,
                Direction.Descending,
                null,
                null))
            .Returns(fixture.CreateMany<AuditItem>(count: 2));

        _scopeProviderMock.Setup(x => x.CreateQuery<IAuditItem>()).Returns(Mock.Of<IQuery<IAuditItem>>());

        var result = await _auditService.GetItemsAsync(10, 5);
        Assert.AreEqual(totalRecords, result.Total);
        Assert.AreEqual(2, result.Items.Count());
    }

    /// <summary>
    /// Tests that GetItemsByKeyAsync throws an ArgumentOutOfRangeException when invalid pagination arguments are provided.
    /// </summary>
    [Test]
    public void GetItemsByKeyAsync_Throws_When_Invalid_Pagination_Arguments_Are_Provided()
    {
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsByKeyAsync(Guid.Empty, UmbracoObjectTypes.Document, -1, 10), "Skip must be greater than or equal to 0.");
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsByKeyAsync(Guid.Empty, UmbracoObjectTypes.Document, 0, -1), "Take must be greater than 0.");
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsByKeyAsync(Guid.Empty, UmbracoObjectTypes.Document, 0, 0), "Take must be greater than 0.");
    }

    /// <summary>
    /// Tests that GetItemsByKeyAsync returns no results when the specified key is not found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetItemsByKeyAsync_Returns_No_Results_When_Key_Is_Not_Found()
    {
        SetupScopeProviderMock();

        _entityServiceMock.Setup(x => x.GetId(Guid.Empty, UmbracoObjectTypes.Document)).Returns(Attempt<int>.Fail());

        var result = await _auditService.GetItemsByKeyAsync(Guid.Empty, UmbracoObjectTypes.Document, 10, 10);
        Assert.AreEqual(0, result.Total);
        Assert.AreEqual(0, result.Items.Count());
    }

    /// <summary>
    /// Verifies that <c>GetItemsByKeyAsync</c> returns the expected total record count and correct number of items for a given key and object type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetItemsByKeyAsync_Returns_Correct_Total_And_Item_Count()
    {
        SetupScopeProviderMock();

        _entityServiceMock.Setup(x => x.GetId(Guid.Empty, UmbracoObjectTypes.Document)).Returns(Attempt.Succeed(2));

        Fixture fixture = new Fixture();
        long totalRecords = 12;

        _auditRepositoryMock.Setup(x => x.GetPagedResultsByQuery(
                It.IsAny<IQuery<IAuditItem>>(),
                2,
                5,
                out totalRecords,
                Direction.Descending,
                null,
                null))
            .Returns(fixture.CreateMany<AuditItem>(count: 2));

        _scopeProviderMock.Setup(x => x.CreateQuery<IAuditItem>())
            .Returns(Mock.Of<IQuery<IAuditItem>>());

        var result = await _auditService.GetItemsByKeyAsync(Guid.Empty, UmbracoObjectTypes.Document, 10, 5);
        Assert.AreEqual(totalRecords, result.Total);
        Assert.AreEqual(2, result.Items.Count());
    }

    /// <summary>
    /// Verifies that <c>GetItemsByEntityAsync</c> returns no audit log entries when called with an entity ID that is either the system root or a value less than root.
    /// </summary>
    /// <param name="userId">The entity ID to test, representing either the root or a value lower than root.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [TestCase(Constants.System.Root)]
    [TestCase(-100)]
    public async Task GetItemsByEntityAsync_Returns_No_Results_When_Id_Is_Root_Or_Lower(int userId)
    {
        var result = await _auditService.GetItemsByEntityAsync(userId, 10, 10);
        Assert.AreEqual(0, result.Total);
        Assert.AreEqual(0, result.Items.Count());
    }

    /// <summary>
    /// Tests that GetItemsByEntityAsync returns the correct total number of records and the correct count of items.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task GetItemsByEntityAsync_Returns_Correct_Total_And_Item_Count()
    {
        SetupScopeProviderMock();

        _entityServiceMock.Setup(x => x.GetId(Guid.Empty, UmbracoObjectTypes.Document)).Returns(Attempt.Succeed(2));

        Fixture fixture = new Fixture();
        long totalRecords = 12;

        _auditRepositoryMock.Setup(x => x.GetPagedResultsByQuery(
                It.IsAny<IQuery<IAuditItem>>(),
                2,
                5,
                out totalRecords,
                Direction.Descending,
                null,
                null))
            .Returns(fixture.CreateMany<AuditItem>(count: 2));

        _scopeProviderMock.Setup(x => x.CreateQuery<IAuditItem>())
            .Returns(Mock.Of<IQuery<IAuditItem>>());

        var result = await _auditService.GetItemsByEntityAsync(1, 10, 5);
        Assert.AreEqual(totalRecords, result.Total);
        Assert.AreEqual(2, result.Items.Count());
    }

    /// <summary>
    /// Verifies that <see cref="AuditService.CleanLogsAsync(int)"/> calls the audit repository's <c>CleanLogs</c> method with the correct parameter value.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task CleanLogsAsync_Calls_Repository_With_Correct_Values()
    {
        SetupScopeProviderMock();
        _auditRepositoryMock.Setup(x => x.CleanLogs(100));
        await _auditService.CleanLogsAsync(100);
        _auditRepositoryMock.Verify(x => x.CleanLogs(It.IsAny<int>()), Times.Once);
    }

    private void SetupScopeProviderMock() =>
        _scopeProviderMock
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<IScope>());
}
