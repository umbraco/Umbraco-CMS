using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

[TestFixture]
public class ExtensionManifestServiceTests
{
    private IExtensionManifestService _service;
    private Mock<IExtensionManifestReader> _readerMock;
    private IAppPolicyCache _runtimeCache;

    [SetUp]
    public void SetUp()
    {
        _readerMock = new Mock<IExtensionManifestReader>();
        _readerMock.Setup(r => r.ReadManifestsAsync()).ReturnsAsync(
            new[]
            {
                new ExtensionManifest { Name = "Test", Extensions = Array.Empty<object>() }
            });

        _runtimeCache = new ObjectCacheAppCache();
        AppCaches appCaches = new AppCaches(
                _runtimeCache,
                NoAppCache.Instance,
                new IsolatedCaches(type => NoAppCache.Instance));

        _service = new ExtensionManifestService(_readerMock.Object, appCaches);
    }

    [Test]
    public async Task CachesManifests()
    {
        var result = await _service.GetManifestsAsync();
        Assert.AreEqual(1, result.Count());

        var result2 = await _service.GetManifestsAsync();
        Assert.AreEqual(1, result2.Count());

        var result3 = await _service.GetManifestsAsync();
        Assert.AreEqual(1, result3.Count());

        _readerMock.Verify(r => r.ReadManifestsAsync(), Times.Exactly(1));
    }

    [Test]
    public async Task ReloadsManifestsAfterCacheClear()
    {
        var result = await _service.GetManifestsAsync();
        Assert.AreEqual(1, result.Count());
        _runtimeCache.Clear();

        var result2 = await _service.GetManifestsAsync();
        Assert.AreEqual(1, result2.Count());
        _runtimeCache.Clear();

        var result3 = await _service.GetManifestsAsync();
        Assert.AreEqual(1, result3.Count());
        _runtimeCache.Clear();

        _readerMock.Verify(r => r.ReadManifestsAsync(), Times.Exactly(3));
    }
}
