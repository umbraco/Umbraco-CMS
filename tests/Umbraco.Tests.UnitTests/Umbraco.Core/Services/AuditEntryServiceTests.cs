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
    public async Task WriteAsync_UsingIds_Calls_Repository_With_Correct_Values()
    {
        SetupScopeProviderMock();

        var date = DateTime.UtcNow;
        _auditEntryRepositoryMock.Setup(x => x.IsAvailable()).Returns(true);
        _auditEntryRepositoryMock.Setup(x => x.Save(It.IsAny<IAuditEntry>()))
            .Callback<IAuditEntry>(item =>
            {
                Assert.AreEqual(Constants.Security.SuperUserId, item.PerformingUserId);
                Assert.AreEqual("performingDetails", item.PerformingDetails);
                Assert.AreEqual("performingIp", item.PerformingIp);
                Assert.AreEqual(date, item.EventDateUtc);
                Assert.AreEqual(Constants.Security.UnknownUserId, item.AffectedUserId);
                Assert.AreEqual("affectedDetails", item.AffectedDetails);
                Assert.AreEqual("umbraco/test", item.EventType);
                Assert.AreEqual("eventDetails", item.EventDetails);
            });

        var result = await _auditEntryService.WriteAsync(
            Constants.Security.SuperUserId,
            "performingDetails",
            "performingIp",
            date,
            Constants.Security.UnknownUserId,
            "affectedDetails",
            "umbraco/test",
            "eventDetails");
        _auditEntryRepositoryMock.Verify(x => x.IsAvailable(), Times.AtLeastOnce);
        _auditEntryRepositoryMock.Verify(x => x.Save(It.IsAny<IAuditEntry>()), Times.Once);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(AuditEntryOperationStatus.Success, result.Status);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(Constants.Security.SuperUserId, result.Result.PerformingUserId);
            Assert.AreEqual("performingDetails", result.Result.PerformingDetails);
            Assert.AreEqual("performingIp", result.Result.PerformingIp);
            Assert.AreEqual(date, result.Result.EventDateUtc);
            Assert.AreEqual(Constants.Security.UnknownUserId, result.Result.AffectedUserId);
            Assert.AreEqual("affectedDetails", result.Result.AffectedDetails);
            Assert.AreEqual("umbraco/test", result.Result.EventType);
            Assert.AreEqual("eventDetails", result.Result.EventDetails);
        });
    }

    [Test]
    public async Task WriteAsync_UsingKeys_Calls_Repository_With_Correct_Values()
    {
        SetupScopeProviderMock();

        var date = DateTime.UtcNow;
        _auditEntryRepositoryMock.Setup(x => x.IsAvailable()).Returns(true);
        _auditEntryRepositoryMock.Setup(x => x.Save(It.IsAny<IAuditEntry>()))
            .Callback<IAuditEntry>(item =>
            {
                Assert.AreEqual(Constants.Security.SuperUserId, item.PerformingUserId);
                Assert.AreEqual("performingDetails", item.PerformingDetails);
                Assert.AreEqual("performingIp", item.PerformingIp);
                Assert.AreEqual(date, item.EventDateUtc);
                Assert.AreEqual(Constants.Security.UnknownUserId, item.AffectedUserId);
                Assert.AreEqual("affectedDetails", item.AffectedDetails);
                Assert.AreEqual("umbraco/test", item.EventType);
                Assert.AreEqual("eventDetails", item.EventDetails);
            });
        _userIdKeyResolverMock.Setup(x => x.GetAsync(Constants.Security.SuperUserKey))
            .Returns(Task.FromResult(Constants.Security.SuperUserId));

        var result = await _auditEntryService.WriteAsync(
            Constants.Security.SuperUserKey,
            "performingDetails",
            "performingIp",
            date,
            Constants.Security.UnknownUserKey,
            "affectedDetails",
            "umbraco/test",
            "eventDetails");

        _auditEntryRepositoryMock.Verify(x => x.IsAvailable(), Times.AtLeastOnce);
        _auditEntryRepositoryMock.Verify(x => x.Save(It.IsAny<IAuditEntry>()), Times.Once);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(AuditEntryOperationStatus.Success, result.Status);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(Constants.Security.SuperUserId, result.Result.PerformingUserId);
            Assert.AreEqual("performingDetails", result.Result.PerformingDetails);
            Assert.AreEqual("performingIp", result.Result.PerformingIp);
            Assert.AreEqual(date, result.Result.EventDateUtc);
            Assert.AreEqual(Constants.Security.UnknownUserId, result.Result.AffectedUserId);
            Assert.AreEqual("affectedDetails", result.Result.AffectedDetails);
            Assert.AreEqual("umbraco/test", result.Result.EventType);
            Assert.AreEqual("eventDetails", result.Result.EventDetails);
        });
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
