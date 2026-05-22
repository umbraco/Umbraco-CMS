using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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
            new TestOptionsMonitor<RuntimeSettings>(new RuntimeSettings { Mode = RuntimeMode.Production }),
            NullLogger<PackageManifestService>.Instance);
    }

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

    [Test]
    public async Task Supports_Public_PackageManifests()
    {
        var result = await _service.GetPublicPackageManifestsAsync();
        Assert.AreEqual(1, result.Count());

        result = await _service.GetAllPackageManifestsAsync();
        Assert.AreEqual(2, result.Count());
    }

    [Test]
    public async Task Supports_Private_PackageManifests()
    {
        var result = await _service.GetPrivatePackageManifestsAsync();
        Assert.AreEqual(1, result.Count());

        result = await _service.GetAllPackageManifestsAsync();
        Assert.AreEqual(2, result.Count());
    }

    [Test]
    public async Task Preload_Empty_When_No_Manifest_Declares_Preload()
    {
        IPackageManifestService service = CreateService(
            new PackageManifest
            {
                Name = "Core",
                Extensions = Array.Empty<object>(),
                Importmap = new PackageManifestImportmap
                {
                    Imports = new Dictionary<string, string> { ["@umb/foo"] = "/foo.js" },
                },
            });

        IReadOnlyList<string> result = await service.GetPackageManifestPreloadAsync();

        Assert.IsEmpty(result);
    }

    [Test]
    public async Task Preload_Returns_Resolved_Urls_In_Manifest_Order()
    {
        IPackageManifestService service = CreateService(
            new PackageManifest
            {
                Name = "Core",
                Extensions = Array.Empty<object>(),
                Importmap = new PackageManifestImportmap
                {
                    Imports = new Dictionary<string, string>
                    {
                        ["@umb/foo"] = "/foo.js",
                        ["@umb/bar"] = "/bar.js",
                    },
                    Preload = new[] { "@umb/foo", "@umb/bar" },
                },
            },
            new PackageManifest
            {
                Name = "Plugin",
                Extensions = Array.Empty<object>(),
                Importmap = new PackageManifestImportmap
                {
                    Imports = new Dictionary<string, string> { ["@plugin/x"] = "/x.js" },
                    Preload = new[] { "@plugin/x" },
                },
            });

        IReadOnlyList<string> result = await service.GetPackageManifestPreloadAsync();

        CollectionAssert.AreEqual(new[] { "/foo.js", "/bar.js", "/x.js" }, result);
    }

    [Test]
    public async Task Preload_Deduplicates_By_Resolved_Url()
    {
        // Two manifests preload aliases that resolve to the same URL — the URL must appear once.
        IPackageManifestService service = CreateService(
            new PackageManifest
            {
                Name = "Core",
                Extensions = Array.Empty<object>(),
                Importmap = new PackageManifestImportmap
                {
                    Imports = new Dictionary<string, string> { ["@umb/foo"] = "/foo.js" },
                    Preload = new[] { "@umb/foo" },
                },
            },
            new PackageManifest
            {
                Name = "Plugin",
                Extensions = Array.Empty<object>(),
                Importmap = new PackageManifestImportmap
                {
                    Imports = new Dictionary<string, string> { ["@plugin/foo-alias"] = "/foo.js" },
                    Preload = new[] { "@plugin/foo-alias" },
                },
            });

        IReadOnlyList<string> result = await service.GetPackageManifestPreloadAsync();

        CollectionAssert.AreEqual(new[] { "/foo.js" }, result);
    }

    [Test]
    public async Task Preload_Resolves_Against_Merged_Importmap_Across_Manifests()
    {
        // A plugin preloads an alias it did NOT declare itself — the alias lives in core's imports.
        IPackageManifestService service = CreateService(
            new PackageManifest
            {
                Name = "Core",
                Extensions = Array.Empty<object>(),
                Importmap = new PackageManifestImportmap
                {
                    Imports = new Dictionary<string, string> { ["@umb/heavy"] = "/heavy.js" },
                },
            },
            new PackageManifest
            {
                Name = "Plugin",
                Extensions = Array.Empty<object>(),
                Importmap = new PackageManifestImportmap
                {
                    Imports = new Dictionary<string, string> { ["@plugin/own"] = "/own.js" },
                    Preload = new[] { "@umb/heavy" },
                },
            });

        IReadOnlyList<string> result = await service.GetPackageManifestPreloadAsync();

        CollectionAssert.AreEqual(new[] { "/heavy.js" }, result);
    }

    [Test]
    public async Task Preload_Skips_Unresolved_Alias_And_Logs_Warning()
    {
        var logger = new Mock<ILogger<PackageManifestService>>();
        logger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        IPackageManifestService service = CreateService(
            logger.Object,
            new PackageManifest
            {
                Name = "Plugin",
                Extensions = Array.Empty<object>(),
                Importmap = new PackageManifestImportmap
                {
                    Imports = new Dictionary<string, string> { ["@plugin/known"] = "/known.js" },
                    Preload = new[] { "@plugin/known", "@plugin/typo" },
                },
            });

        IReadOnlyList<string> result = await service.GetPackageManifestPreloadAsync();

        CollectionAssert.AreEqual(new[] { "/known.js" }, result);
        logger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("@plugin/typo")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private IPackageManifestService CreateService(params PackageManifest[] manifests)
        => CreateService(NullLogger<PackageManifestService>.Instance, manifests);

    private IPackageManifestService CreateService(ILogger<PackageManifestService> logger, params PackageManifest[] manifests)
    {
        var reader = new Mock<IPackageManifestReader>();
        reader.Setup(r => r.ReadPackageManifestsAsync()).ReturnsAsync(manifests);

        var appCaches = new AppCaches(
            new ObjectCacheAppCache(),
            NoAppCache.Instance,
            new IsolatedCaches(type => NoAppCache.Instance));

        return new PackageManifestService(
            new[] { reader.Object },
            appCaches,
            new TestOptionsMonitor<RuntimeSettings>(new RuntimeSettings { Mode = RuntimeMode.Production }),
            logger);
    }
}
