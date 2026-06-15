using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

[TestFixture]
public class PackageManifestServiceTests
{
    private IPackageManifestService _service;
    private Mock<IPackageManifestReader> _readerMock;
    private IAppPolicyCache _runtimeCache;

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

    [Test]
    public async Task Caches_PackageManifests()
    {
        var result = await _service.GetAllPackageManifestsAsync();
        Assert.That(result.Count(), Is.EqualTo(2));

        var result2 = await _service.GetAllPackageManifestsAsync();
        Assert.That(result2.Count(), Is.EqualTo(2));

        var result3 = await _service.GetAllPackageManifestsAsync();
        Assert.That(result3.Count(), Is.EqualTo(2));

        _readerMock.Verify(r => r.ReadPackageManifestsAsync(), Times.Exactly(1));
    }

    [Test]
    public async Task Reloads_PackageManifest_After_Cache_Clear()
    {
        var result = await _service.GetAllPackageManifestsAsync();
        Assert.That(result.Count(), Is.EqualTo(2));
        _runtimeCache.Clear();

        var result2 = await _service.GetAllPackageManifestsAsync();
        Assert.That(result2.Count(), Is.EqualTo(2));
        _runtimeCache.Clear();

        var result3 = await _service.GetAllPackageManifestsAsync();
        Assert.That(result3.Count(), Is.EqualTo(2));
        _runtimeCache.Clear();

        _readerMock.Verify(r => r.ReadPackageManifestsAsync(), Times.Exactly(3));
    }

    [Test]
    public async Task Supports_Public_PackageManifests()
    {
        var result = await _service.GetPublicPackageManifestsAsync();
        Assert.That(result.Count(), Is.EqualTo(1));

        result = await _service.GetAllPackageManifestsAsync();
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Supports_Private_PackageManifests()
    {
        var result = await _service.GetPrivatePackageManifestsAsync();
        Assert.That(result.Count(), Is.EqualTo(1));

        result = await _service.GetAllPackageManifestsAsync();
        Assert.That(result.Count(), Is.EqualTo(2));
    }
}
