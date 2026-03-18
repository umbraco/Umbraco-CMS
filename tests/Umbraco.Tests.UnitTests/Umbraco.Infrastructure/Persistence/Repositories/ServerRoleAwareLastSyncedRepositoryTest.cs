using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Factories;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
public class ServerRoleAwareLastSyncedRepositoryTest
{
    private string _tempPath = null!;
    private Mock<IServerRoleAccessor> _serverRoleAccessor = null!;
    private Mock<IDatabaseReadOnlyAccessor> _databaseReadOnlyAccessor = null!;
    private FileSystemLastSyncedRepository _fileSystemRepository = null!;

    [SetUp]
    public void SetUp()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "UmbracoTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempPath);

        _serverRoleAccessor = new Mock<IServerRoleAccessor>();
        _databaseReadOnlyAccessor = new Mock<IDatabaseReadOnlyAccessor>();

        var hostingEnvironment = new Mock<IHostingEnvironment>();
        hostingEnvironment.Setup(x => x.LocalTempPath).Returns(_tempPath);
        hostingEnvironment.Setup(x => x.ApplicationId).Returns("TestApplication");
        _fileSystemRepository = new FileSystemLastSyncedRepository(hostingEnvironment.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
    }

    [Test]
    public async Task Subscriber_With_ReadOnly_Database_Delegates_To_FileSystemRepository()
    {
        _serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(ServerRole.Subscriber);
        _databaseReadOnlyAccessor.Setup(x => x.IsReadOnly()).Returns(true);

        await _fileSystemRepository.SaveInternalIdAsync(99);

        var sut = CreateSut();
        var result = await sut.GetInternalIdAsync();

        Assert.AreEqual(99, result);
    }

    [Test]
    public void Subscriber_With_Writable_Database_Delegates_To_DatabaseRepository()
    {
        _serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(ServerRole.Subscriber);
        _databaseReadOnlyAccessor.Setup(x => x.IsReadOnly()).Returns(false);

        // The database repository has no ambient scope, so it throws InvalidOperationException.
        // This proves that the call did NOT go to the file system repository.
        var sut = CreateSut();
        Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetInternalIdAsync());
    }

    [TestCase(ServerRole.Single)]
    [TestCase(ServerRole.SchedulingPublisher)]
    [TestCase(ServerRole.Unknown)]
    public void NonSubscriber_Delegates_To_DatabaseRepository(ServerRole role)
    {
        _serverRoleAccessor.Setup(x => x.CurrentServerRole).Returns(role);

        // The database repository has no ambient scope, so it throws InvalidOperationException.
        // This proves that the call did NOT go to the file system repository.
        var sut = CreateSut();
        Assert.ThrowsAsync<InvalidOperationException>(() => sut.GetInternalIdAsync());
    }

    private ServerRoleAwareLastSyncedRepository CreateSut()
        => new(
            new Lazy<IServerRoleAccessor>(() => _serverRoleAccessor.Object),
            new LastSyncedRepository(
                Mock.Of<IScopeAccessor>(),
                AppCaches.NoCache,
                Mock.Of<IMachineInfoFactory>()),
            _fileSystemRepository,
            _databaseReadOnlyAccessor.Object);
}
