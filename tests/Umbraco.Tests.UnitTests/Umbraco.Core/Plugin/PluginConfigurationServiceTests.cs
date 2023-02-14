using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Plugin;
using Umbraco.Cms.Infrastructure.Plugin;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Plugin;

[TestFixture]
public class PluginConfigurationServiceTests
{
    private IPluginConfigurationService _service;
    private Mock<IPluginConfigurationReader> _readerMock;
    private IAppPolicyCache _runtimeCache;

    [SetUp]
    public void SetUp()
    {
        _readerMock = new Mock<IPluginConfigurationReader>();
        _readerMock.Setup(r => r.ReadPluginConfigurationsAsync()).ReturnsAsync(
            new[]
            {
                new PluginConfiguration { Name = "Test", Extensions = Array.Empty<object>() }
            });

        _runtimeCache = new ObjectCacheAppCache();
        AppCaches appCaches = new AppCaches(
                _runtimeCache,
                NoAppCache.Instance,
                new IsolatedCaches(type => NoAppCache.Instance));

        _service = new PluginConfigurationService(_readerMock.Object, appCaches);
    }

    [Test]
    public async Task CachesExtensionPackageConfigurations()
    {
        var result = await _service.GetPluginConfigurationsAsync();
        Assert.AreEqual(1, result.Count());

        var result2 = await _service.GetPluginConfigurationsAsync();
        Assert.AreEqual(1, result2.Count());

        var result3 = await _service.GetPluginConfigurationsAsync();
        Assert.AreEqual(1, result3.Count());

        _readerMock.Verify(r => r.ReadPluginConfigurationsAsync(), Times.Exactly(1));
    }

    [Test]
    public async Task ReloadsExtensionPackageConfigurationsAfterCacheClear()
    {
        var result = await _service.GetPluginConfigurationsAsync();
        Assert.AreEqual(1, result.Count());
        _runtimeCache.Clear();

        var result2 = await _service.GetPluginConfigurationsAsync();
        Assert.AreEqual(1, result2.Count());
        _runtimeCache.Clear();

        var result3 = await _service.GetPluginConfigurationsAsync();
        Assert.AreEqual(1, result3.Count());
        _runtimeCache.Clear();

        _readerMock.Verify(r => r.ReadPluginConfigurationsAsync(), Times.Exactly(3));
    }
}
