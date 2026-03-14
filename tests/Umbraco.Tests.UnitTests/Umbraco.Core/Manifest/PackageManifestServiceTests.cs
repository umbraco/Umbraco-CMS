using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

/// <summary>
/// Contains unit tests for the <see cref="PackageManifestService"/> class.
/// </summary>
[TestFixture]
public class PackageManifestServiceTests
{
    private IPackageManifestService _service;
    private Mock<IPackageManifestReader> _readerMock;
    private IAppPolicyCache _runtimeCache;

    /// <summary>
    /// Initializes mocks and dependencies required for each test in <see cref="PackageManifestServiceTests"/>.
    /// Sets up the <see cref="PackageManifestService"/> instance and configures the mock <see cref="IPackageManifestReader"/>.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        _readerMock = new Mock<IPackageManifestReader>();
        _readerMock.Setup(r => r.ReadPackageManifestsAsync()).ReturnsAsync(
            new[]
            {
                new PackageManifest { Name = "Test", Extensions = Array.Empty<object>(), AllowPublicAccess = false },
                new PackageManifest { Name = "Test Public", Extensions = Array.Empty<object>(), AllowPublicAccess = true },
            });

        _runtimeCache = new ObjectCacheAppCache();
        AppCaches appCaches = new AppCaches(
                _runtimeCache,
                NoAppCache.Instance,
                new IsolatedCaches(type => NoAppCache.Instance));

        _service = new PackageManifestService(
            new[] { _readerMock.Object },
            appCaches,
            new TestOptionsMonitor<RuntimeSettings>(new RuntimeSettings { Mode = RuntimeMode.Production }));
    }

    /// <summary>
    /// Tests that package manifests are cached and the underlying reader is called only once.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Caches_PackageManifests()
    {
        var result = await _service.GetAllPackageManifestsAsync();
        Assert.AreEqual(2, result.Count());

        var result2 = await _service.GetAllPackageManifestsAsync();
        Assert.AreEqual(2, result2.Count());

        var result3 = await _service.GetAllPackageManifestsAsync();
        Assert.AreEqual(2, result3.Count());

        _readerMock.Verify(r => r.ReadPackageManifestsAsync(), Times.Exactly(1));
    }

    /// <summary>
    /// Tests that the package manifest is reloaded after the cache is cleared.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Reloads_PackageManifest_After_Cache_Clear()
    {
        var result = await _service.GetAllPackageManifestsAsync();
        Assert.AreEqual(2, result.Count());
        _runtimeCache.Clear();

        var result2 = await _service.GetAllPackageManifestsAsync();
        Assert.AreEqual(2, result2.Count());
        _runtimeCache.Clear();

        var result3 = await _service.GetAllPackageManifestsAsync();
        Assert.AreEqual(2, result3.Count());
        _runtimeCache.Clear();

        _readerMock.Verify(r => r.ReadPackageManifestsAsync(), Times.Exactly(3));
    }

    /// <summary>
    /// Verifies that the <see cref="PackageManifestService"/> correctly retrieves public package manifests
    /// and that the count of public and all manifests matches expectations.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task Supports_Public_PackageManifests()
    {
        var result = await _service.GetPublicPackageManifestsAsync();
        Assert.AreEqual(1, result.Count());

        result = await _service.GetAllPackageManifestsAsync();
        Assert.AreEqual(2, result.Count());
    }

    /// <summary>
    /// Verifies that the package manifest service correctly retrieves private package manifests
    /// and distinguishes them from all available package manifests.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Supports_Private_PackageManifests()
    {
        var result = await _service.GetPrivatePackageManifestsAsync();
        Assert.AreEqual(1, result.Count());

        result = await _service.GetAllPackageManifestsAsync();
        Assert.AreEqual(2, result.Count());
    }
}
