using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
public class FileSystemLastSyncedRepositoryTest
{
    private string _tempPath = null!;
    private Mock<IHostingEnvironment> _hostingEnvironment = null!;

    [SetUp]
    public void SetUp()
    {
        _tempPath = Path.Combine(Path.GetTempPath(), "UmbracoTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempPath);

        _hostingEnvironment = new Mock<IHostingEnvironment>();
        _hostingEnvironment.Setup(x => x.LocalTempPath).Returns(_tempPath);
        _hostingEnvironment.Setup(x => x.ApplicationId).Returns("TestApplication");
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
    public async Task Internal_Id_Is_Initially_Null()
    {
        var repo = CreateRepository();
        var value = await repo.GetInternalIdAsync();
        Assert.IsNull(value);
    }

    [Test]
    public async Task External_Id_Is_Initially_Null()
    {
        var repo = CreateRepository();
        var value = await repo.GetExternalIdAsync();
        Assert.IsNull(value);
    }

    [Test]
    public async Task Save_And_Get_Internal_Id()
    {
        var repo = CreateRepository();

        // Read first to initialize the lazy lock.
        await repo.GetInternalIdAsync();

        var testId = new Random().Next(1, int.MaxValue);
        await repo.SaveInternalIdAsync(testId);

        var value = await repo.GetInternalIdAsync();
        Assert.AreEqual(testId, value);
    }

    [Test]
    public async Task Save_And_Get_External_Id()
    {
        var repo = CreateRepository();

        // Read first to initialize the lazy lock.
        await repo.GetExternalIdAsync();

        var testId = new Random().Next(1, int.MaxValue);
        await repo.SaveExternalIdAsync(testId);

        var value = await repo.GetExternalIdAsync();
        Assert.AreEqual(testId, value);
    }

    [Test]
    public async Task Internal_And_External_Ids_Are_Independent()
    {
        var repo = CreateRepository();

        await repo.GetInternalIdAsync();
        await repo.GetExternalIdAsync();

        await repo.SaveInternalIdAsync(100);
        await repo.SaveExternalIdAsync(200);

        Assert.AreEqual(100, await repo.GetInternalIdAsync());
        Assert.AreEqual(200, await repo.GetExternalIdAsync());
    }

    [Test]
    public async Task Persists_Internal_Id_To_Disk()
    {
        var testId = new Random().Next(1, int.MaxValue);

        // Save with one instance.
        var repo1 = CreateRepository();
        await repo1.GetInternalIdAsync();
        await repo1.SaveInternalIdAsync(testId);

        // Read with a fresh instance to verify file persistence.
        var repo2 = CreateRepository();
        var value = await repo2.GetInternalIdAsync();

        Assert.AreEqual(testId, value);
    }

    [Test]
    public async Task Persists_External_Id_To_Disk()
    {
        var testId = new Random().Next(1, int.MaxValue);

        // Save with one instance.
        var repo1 = CreateRepository();
        await repo1.GetExternalIdAsync();
        await repo1.SaveExternalIdAsync(testId);

        // Read with a fresh instance to verify file persistence.
        var repo2 = CreateRepository();
        var value = await repo2.GetExternalIdAsync();

        Assert.AreEqual(testId, value);
    }

    [Test]
    public async Task Creates_DistCache_Folder()
    {
        var repo = CreateRepository();
        await repo.GetInternalIdAsync();

        var distCacheFolder = Path.Combine(_tempPath, "DistCache");
        Assert.IsTrue(Directory.Exists(distCacheFolder));
    }

    [Test]
    public async Task Delete_Entries_Is_NoOp()
    {
        var repo = CreateRepository();

        await repo.GetExternalIdAsync();
        await repo.SaveExternalIdAsync(42);

        // Delete should not affect file-system entries.
        await repo.DeleteEntriesOlderThanAsync(DateTime.Now + TimeSpan.FromDays(1));

        var value = await repo.GetExternalIdAsync();
        Assert.AreEqual(42, value);
    }

    private FileSystemLastSyncedRepository CreateRepository()
        => new(_hostingEnvironment.Object);
}
