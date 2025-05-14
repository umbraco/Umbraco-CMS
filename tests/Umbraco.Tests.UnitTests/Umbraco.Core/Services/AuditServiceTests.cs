using System.Data;
using AutoFixture;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class AuditServiceTests
{
    private IAuditService _auditService;
    private Mock<ICoreScopeProvider> _scopeProviderMock;
    private Mock<IAuditRepository> _auditRepositoryMock;
    private Mock<IEntityService> _entityServiceMock;

    [SetUp]
    public void Setup()
    {
        _scopeProviderMock = new Mock<ICoreScopeProvider>(MockBehavior.Strict);
        _auditRepositoryMock = new Mock<IAuditRepository>(MockBehavior.Strict);
        _entityServiceMock = new Mock<IEntityService>(MockBehavior.Strict);

        _auditService = new AuditService(
            _scopeProviderMock.Object,
            Mock.Of<ILoggerFactory>(MockBehavior.Strict),
            Mock.Of<IEventMessagesFactory>(MockBehavior.Strict),
            _auditRepositoryMock.Object,
            Mock.Of<IUserService>(MockBehavior.Strict),
            _entityServiceMock.Object);
    }

    [TestCase(AuditType.Publish, -1, 33, null, null, null)]
    [TestCase(AuditType.Copy, -1, 1, "entityType", "comment", "parameters")]
    public async Task AddAsync_Calls_Repository_With_Correct_Values(AuditType type, int userId, int objectId, string? entityType, string? comment, string? parameters = null)
    {
        SetupScopeProviderMock();

        _auditRepositoryMock.Setup(x => x.Save(It.IsAny<IAuditItem>()))
            .Callback<IAuditItem>(item =>
            {
                Assert.AreEqual(type, item.AuditType);
                Assert.AreEqual(userId, item.UserId);
                Assert.AreEqual(objectId, item.Id);
                Assert.AreEqual(entityType, item.EntityType);
                Assert.AreEqual(comment, item.Comment);
                Assert.AreEqual(parameters, item.Parameters);
            });

        var result = await _auditService.AddAsync(type, userId, objectId, entityType, comment, parameters);
        _auditRepositoryMock.Verify(x => x.Save(It.IsAny<IAuditItem>()), Times.Once);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(AuditLogOperationStatus.Success, result.Result);
    }

    [Test]
    public void GetItemsAsync_Throws_When_Invalid_Pagination_Arguments_Are_Provided()
    {
        SetupScopeProviderMock();

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsAsync(-1, 10), "Skip must be greater than or equal to 0.");
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsAsync(0, -1), "Take must be greater than 0.");
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsAsync(0, 0), "Take must be greater than 0.");
    }

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

    [Test]
    public void GetItemsByKeyAsync_Throws_When_Invalid_Pagination_Arguments_Are_Provided()
    {
        SetupScopeProviderMock();

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsByKeyAsync(Guid.Empty, UmbracoObjectTypes.Document, -1, 10), "Skip must be greater than or equal to 0.");
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsByKeyAsync(Guid.Empty, UmbracoObjectTypes.Document, 0, -1), "Take must be greater than 0.");
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await _auditService.GetItemsByKeyAsync(Guid.Empty, UmbracoObjectTypes.Document, 0, 0), "Take must be greater than 0.");
    }

    [Test]
    public async Task GetItemsByKeyAsync_Returns_No_Results_When_Key_Is_Not_Found()
    {
        SetupScopeProviderMock();

        _entityServiceMock.Setup(x => x.GetId(Guid.Empty, UmbracoObjectTypes.Document)).Returns(Attempt<int>.Fail());

        var result = await _auditService.GetItemsByKeyAsync(Guid.Empty, UmbracoObjectTypes.Document, 10, 10);
        Assert.AreEqual(0, result.Total);
        Assert.AreEqual(0, result.Items.Count());
    }

    [Test]
    public async Task GetItemsByKeyAsync_Returns_Correct_Total_And_Item_Count()
    {
        SetupScopeProviderMock();

        _entityServiceMock.Setup(x => x.GetId(Guid.Empty, UmbracoObjectTypes.Document)).Returns(Attempt.Succeed(2));

        Fixture fixture = new Fixture();
        long totalRecords = 12;

        // TODO: Test whether the provided query has a filter on id
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

    [Test]
    public async Task GetItemsByEntityAsync_Returns_No_Results_When_Key_Is_Not_Found()
    {
        SetupScopeProviderMock();

        _entityServiceMock.Setup(x => x.GetId(Guid.Empty, UmbracoObjectTypes.Document)).Returns(Attempt<int>.Fail());

        var result = await _auditService.GetItemsByEntityAsync(-1, 10, 10);
        Assert.AreEqual(0, result.Total);
        Assert.AreEqual(0, result.Items.Count());
    }

    [TestCase(Constants.System.Root)]
    [TestCase(-100)]
    public async Task GetItemsByEntityAsync_Returns_No_Results_When_Id_Is_Root_Or_Lower(int userId)
    {
        SetupScopeProviderMock();

        _entityServiceMock.Setup(x => x.GetId(Guid.Empty, UmbracoObjectTypes.Document)).Returns(Attempt<int>.Fail());

        var result = await _auditService.GetItemsByEntityAsync(userId, 10, 10);
        Assert.AreEqual(0, result.Total);
        Assert.AreEqual(0, result.Items.Count());
    }

    [Test]
    public async Task GetItemsByEntityAsync_Returns_Correct_Total_And_Item_Count()
    {
        SetupScopeProviderMock();

        _entityServiceMock.Setup(x => x.GetId(Guid.Empty, UmbracoObjectTypes.Document)).Returns(Attempt.Succeed(2));

        Fixture fixture = new Fixture();
        long totalRecords = 12;

        // TODO: Test whether the provided query has a filter on id
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
