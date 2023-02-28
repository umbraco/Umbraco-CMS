using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

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
                new PackageManifest { Name = "Test", Extensions = Array.Empty<object>() }
            });

        _runtimeCache = new ObjectCacheAppCache();
        AppCaches appCaches = new AppCaches(
                _runtimeCache,
                NoAppCache.Instance,
                new IsolatedCaches(type => NoAppCache.Instance));

        _service = new PackageManifestService(new[] { _readerMock.Object }, appCaches, new OptionsWrapper<PackageManifestSettings>(new PackageManifestSettings()));
    }

    [Test]
    public async Task Caches_PackageManifests()
    {
        var result = await _service.GetPackageManifestsAsync();
        Assert.AreEqual(1, result.Count());

        var result2 = await _service.GetPackageManifestsAsync();
        Assert.AreEqual(1, result2.Count());

        var result3 = await _service.GetPackageManifestsAsync();
        Assert.AreEqual(1, result3.Count());

        _readerMock.Verify(r => r.ReadPackageManifestsAsync(), Times.Exactly(1));
    }

    [Test]
    public async Task Reloads_PackageManifest_After_Cache_Clear()
    {
        var result = await _service.GetPackageManifestsAsync();
        Assert.AreEqual(1, result.Count());
        _runtimeCache.Clear();

        var result2 = await _service.GetPackageManifestsAsync();
        Assert.AreEqual(1, result2.Count());
        _runtimeCache.Clear();

        var result3 = await _service.GetPackageManifestsAsync();
        Assert.AreEqual(1, result3.Count());
        _runtimeCache.Clear();

        _readerMock.Verify(r => r.ReadPackageManifestsAsync(), Times.Exactly(3));
    }
}
