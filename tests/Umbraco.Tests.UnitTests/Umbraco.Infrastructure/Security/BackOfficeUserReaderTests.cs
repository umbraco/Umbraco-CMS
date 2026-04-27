using System.Data;
using System.Data.Common;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Security;

[TestFixture]
public class BackOfficeUserReaderTests
{
    private const int UserId = 1;

    private Mock<ICoreScopeProvider> _scopeProviderMock = null!;
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<IRuntimeState> _runtimeStateMock = null!;
    private BackOfficeUserReader _sut = null!;

    [SetUp]
    public void Setup()
    {
        _scopeProviderMock = new Mock<ICoreScopeProvider>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _runtimeStateMock = new Mock<IRuntimeState>();

        _scopeProviderMock
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());

        _sut = new BackOfficeUserReader(
            _scopeProviderMock.Object,
            _userRepositoryMock.Object,
            _runtimeStateMock.Object);
    }

    [Test]
    public void GetById_ReturnsUser_WhenRepositoryReturnsUser()
    {
        IUser expected = Mock.Of<IUser>();
        _userRepositoryMock.Setup(x => x.Get(UserId)).Returns(expected);

        IUser? result = _sut.GetById(UserId);

        Assert.That(result, Is.SameAs(expected));
    }

    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.Upgrading)]
    public void GetById_FallsBackToGetForUpgrade_WhenDbExceptionAndUpgrading(RuntimeLevel level)
    {
        IUser upgradeUser = Mock.Of<IUser>();
        _userRepositoryMock.Setup(x => x.Get(UserId)).Throws(new TestDbException());
        _userRepositoryMock.Setup(x => x.GetForUpgrade(UserId)).Returns(upgradeUser);
        _runtimeStateMock.SetupGet(x => x.Level).Returns(level);

        IUser? result = _sut.GetById(UserId);

        Assert.That(result, Is.SameAs(upgradeUser));
    }

    [TestCase(RuntimeLevel.Run)]
    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.BootFailed)]
    public void GetById_RethrowsDbException_WhenNotUpgrading(RuntimeLevel level)
    {
        _userRepositoryMock.Setup(x => x.Get(UserId)).Throws(new TestDbException());
        _runtimeStateMock.SetupGet(x => x.Level).Returns(level);

        Assert.Throws<TestDbException>(() => _sut.GetById(UserId));
    }

    private sealed class TestDbException : DbException
    {
        public TestDbException()
            : base("Simulated database exception")
        {
        }
    }
}
