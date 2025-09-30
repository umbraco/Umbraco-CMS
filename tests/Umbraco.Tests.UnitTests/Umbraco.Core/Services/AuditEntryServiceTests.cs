using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class AuditEntryServiceTests
{
    private static readonly Guid _testUserKey = Guid.NewGuid();
    private IAuditEntryService _auditEntryService;
    private Mock<ICoreScopeProvider> _scopeProviderMock;
    private Mock<IAuditEntryRepository> _auditEntryRepositoryMock;
    private Mock<IUserIdKeyResolver> _userIdKeyResolverMock;

    [SetUp]
    public void Setup()
    {
        _scopeProviderMock = new Mock<ICoreScopeProvider>(MockBehavior.Strict);
        _auditEntryRepositoryMock = new Mock<IAuditEntryRepository>(MockBehavior.Strict);
        _userIdKeyResolverMock = new Mock<IUserIdKeyResolver>(MockBehavior.Strict);

        _auditEntryService = new AuditEntryService(
            _auditEntryRepositoryMock.Object,
            _userIdKeyResolverMock.Object,
            _scopeProviderMock.Object,
            Mock.Of<ILoggerFactory>(MockBehavior.Strict),
            Mock.Of<IEventMessagesFactory>(MockBehavior.Strict));
    }

    [Test]
    public async Task WriteAsync_Calls_Repository_With_Correct_Values()
    {
        SetupScopeProviderMock();

        var date = DateTime.UtcNow;
        _auditEntryRepositoryMock.Setup(x => x.Save(It.IsAny<IAuditEntry>()))
            .Callback<IAuditEntry>(item =>
            {
                Assert.AreEqual(Constants.Security.SuperUserId, item.PerformingUserId);
                Assert.AreEqual(Constants.Security.SuperUserKey, item.PerformingUserKey);
                Assert.AreEqual("performingDetails", item.PerformingDetails);
                Assert.AreEqual("performingIp", item.PerformingIp);
                Assert.AreEqual(date, item.EventDate);
                Assert.AreEqual(Constants.Security.UnknownUserId, item.AffectedUserId);
                Assert.AreEqual(null, item.AffectedUserKey);
                Assert.AreEqual("affectedDetails", item.AffectedDetails);
                Assert.AreEqual("umbraco/test", item.EventType);
                Assert.AreEqual("eventDetails", item.EventDetails);
            });
        _userIdKeyResolverMock.Setup(x => x.TryGetAsync(Constants.Security.SuperUserKey))
            .ReturnsAsync(Attempt.Succeed(Constants.Security.SuperUserId));

        var result = await _auditEntryService.WriteAsync(
            Constants.Security.SuperUserKey,
            "performingDetails",
            "performingIp",
            date,
            null,
            "affectedDetails",
            "umbraco/test",
            "eventDetails");

        _auditEntryRepositoryMock.Verify(x => x.Save(It.IsAny<IAuditEntry>()), Times.Once);

        Assert.NotNull(result);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(Constants.Security.SuperUserId, result.PerformingUserId);
            Assert.AreEqual("performingDetails", result.PerformingDetails);
            Assert.AreEqual("performingIp", result.PerformingIp);
            Assert.AreEqual(date, result.EventDate);
            Assert.AreEqual(Constants.Security.UnknownUserId, result.AffectedUserId);
            Assert.AreEqual("affectedDetails", result.AffectedDetails);
            Assert.AreEqual("umbraco/test", result.EventType);
            Assert.AreEqual("eventDetails", result.EventDetails);
        });
    }

    [Test]
    public async Task GetUserId_UsingKey_Returns_Correct_Id()
    {
        SetupScopeProviderMock();

        int userId = 12;
        _userIdKeyResolverMock.Setup(x => x.TryGetAsync(_testUserKey))
            .ReturnsAsync(Attempt.Succeed(userId));

        var actualUserId = await ((AuditEntryService)_auditEntryService).GetUserId(_testUserKey);

        Assert.AreEqual(actualUserId, userId);
    }

    [Test]
    public async Task GetUserId_UsingNonExistingKey_Returns_Null()
    {
        SetupScopeProviderMock();

        _userIdKeyResolverMock.Setup(x => x.TryGetAsync(_testUserKey))
            .ReturnsAsync(Attempt.Fail<int>());

        var actualUserId = await ((AuditEntryService)_auditEntryService).GetUserId(_testUserKey);

        Assert.AreEqual(null, actualUserId);
    }

    [Test]
    public async Task GetUserKey_UsingKey_Returns_Correct_Id()
    {
        SetupScopeProviderMock();

        int userId = 12;
        _userIdKeyResolverMock.Setup(x => x.TryGetAsync(userId))
            .ReturnsAsync(Attempt.Succeed(_testUserKey));

        var actualUserKey = await ((AuditEntryService)_auditEntryService).GetUserKey(userId);

        Assert.AreEqual(actualUserKey, _testUserKey);
    }

    [Test]
    public async Task GetUserKey_UsingNonExistingId_Returns_Null()
    {
        SetupScopeProviderMock();

        int userId = 12;
        _userIdKeyResolverMock.Setup(x => x.TryGetAsync(userId))
            .ReturnsAsync(Attempt.Fail<Guid>());

        var userKey = await ((AuditEntryService)_auditEntryService).GetUserKey(userId);

        Assert.AreEqual(null, userKey);
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
