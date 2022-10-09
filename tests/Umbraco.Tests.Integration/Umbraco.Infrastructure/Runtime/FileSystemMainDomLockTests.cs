using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure.Runtime;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Runtime;

[TestFixture]
internal class FileSystemMainDomLockTests : UmbracoIntegrationTest
{
    [SetUp]
    public void SetUp()
    {
        MainDomKeyGenerator = GetRequiredService<IMainDomKeyGenerator>();
        HostingEnvironment = GetRequiredService<IHostingEnvironment>();

        var lockFileName = $"MainDom_{MainDomKeyGenerator.GenerateKey()}.lock";
        LockFilePath = Path.Combine(HostingEnvironment.LocalTempPath, lockFileName);
        LockReleaseFilePath = LockFilePath + "_release";

        var globalSettings = Mock.Of<IOptionsMonitor<GlobalSettings>>();
        Mock.Get(globalSettings).Setup(x => x.CurrentValue).Returns(new GlobalSettings());

        var log = GetRequiredService<ILogger<FileSystemMainDomLock>>();
        FileSystemMainDomLock = new FileSystemMainDomLock(log, MainDomKeyGenerator, HostingEnvironment, globalSettings);
    }

    [TearDown]
    public void TearDown()
    {
        CleanupTestFile(LockFilePath);
        CleanupTestFile(LockReleaseFilePath);
    }

    private IMainDomKeyGenerator MainDomKeyGenerator { get; set; }

    private IHostingEnvironment HostingEnvironment { get; set; }

    private FileSystemMainDomLock FileSystemMainDomLock { get; set; }

    private string LockFilePath { get; set; }
    private string LockReleaseFilePath { get; set; }

    private static void CleanupTestFile(string path)
    {
        for (var i = 0; i < 3; i++)
        {
            try
            {
                File.Delete(path);
                return;
            }
            catch
            {
                Thread.Sleep(500 * (i + 1));
            }
        }
    }

    [Test]
    public async Task AcquireLockAsync_WhenNoOtherHoldsLockFileHandle_ReturnsTrue()
    {
        using var sut = FileSystemMainDomLock;

        var result = await sut.AcquireLockAsync(1000);

        Assert.True(result);
    }

    [Test]
    public async Task AcquireLockAsync_WhenTimeoutExceeded_ReturnsFalse()
    {
        await using var lockFile = File.Open(LockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

        using var sut = FileSystemMainDomLock;

        var result = await sut.AcquireLockAsync(1000);

        Assert.False(result);
    }

    [Test]
    public async Task ListenAsync_WhenLockReleaseSignalFileFound_DropsLockFileHandle()
    {
        using var sut = FileSystemMainDomLock;

        await sut.AcquireLockAsync(1000);

        var before = await sut.AcquireLockAsync(1000);

        sut.CreateLockReleaseSignalFile();
        await sut.ListenAsync();

        var after = await sut.AcquireLockAsync(1000);

        Assert.Multiple(() =>
        {
            Assert.False(before);
            Assert.True(after);
        });
    }
}
