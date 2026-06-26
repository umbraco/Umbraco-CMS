using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Infrastructure.Manifest;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Manifest;

[TestFixture]
public class PackageManifestServiceTests
{
    private static PackageManifestService CreateService(params PackageManifest[] manifests)
        => CreateService(string.Empty, manifests);

    private static PackageManifestService CreateService(string cacheBuster, params PackageManifest[] manifests)
    {
        var reader = new Mock<IPackageManifestReader>();
        reader.Setup(x => x.ReadPackageManifestsAsync()).ReturnsAsync(manifests);

        var runtimeSettings = new Mock<IOptionsMonitor<RuntimeSettings>>();
        runtimeSettings.Setup(x => x.CurrentValue).Returns(new RuntimeSettings { Mode = RuntimeMode.Production });

        var pluginSettings = new Mock<IOptionsMonitor<UmbracoPluginSettings>>();
        pluginSettings.Setup(x => x.CurrentValue).Returns(new UmbracoPluginSettings { Cachebuster = cacheBuster });

        return new PackageManifestService(
            new[] { reader.Object },
            AppCaches.Disabled,
            runtimeSettings.Object,
            pluginSettings.Object);
    }

    private static PackageManifest Manifest(string name, string? version, bool allowCacheBusting, Dictionary<string, string> imports)
        => new()
        {
            Name = name,
            Version = version,
            AllowCacheBusting = allowCacheBusting,
            Extensions = Array.Empty<object>(),
            Importmap = new PackageManifestImportmap { Imports = imports },
        };

    private static IEnumerable<TestCaseData> SingleImportCases()
    {
        yield return new TestCaseData("2.0.0", true, "/App_Plugins/Pkg/index.js", "/App_Plugins/Pkg/index.js?v=2.0.0")
            .SetName("Stamps the version on a clean /App_Plugins import");
        yield return new TestCaseData("2.0.0", false, "/App_Plugins/Pkg/index.js", "/App_Plugins/Pkg/index.js")
            .SetName("Does not stamp when busting is disabled");
        yield return new TestCaseData((string?)null, true, "/App_Plugins/Pkg/index.js", "/App_Plugins/Pkg/index.js")
            .SetName("Leaves a clean import untouched when there is no version and no cache-buster");
        yield return new TestCaseData("2.0.0", true, "/App_Plugins/Pkg/index.js?v=%CACHE_BUSTER%", "/App_Plugins/Pkg/index.js?v=2.0.0")
            .SetName("Resolves an explicit %CACHE_BUSTER% token to the version");
    }

    [TestCaseSource(nameof(SingleImportCases))]
    public async Task GetPackageManifestImportmapAsync_StampsSingleImport(string? version, bool allowCacheBusting, string importUrl, string expected)
    {
        var service = CreateService(Manifest("Pkg", version, allowCacheBusting, new Dictionary<string, string> { ["pkg"] = importUrl }));

        var result = await service.GetPackageManifestImportmapAsync();

        Assert.That(result.Imports["pkg"], Is.EqualTo(expected));
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_LeavesNonAppPluginsAndBareSpecifiersUnchanged()
    {
        var service = CreateService(Manifest(
            "Pkg",
            "2.0.0",
            allowCacheBusting: true,
            new Dictionary<string, string>
            {
                ["bare"] = "@scope/pkg",
                ["cdn"] = "https://cdn.example.com/x.js",
            }));

        var result = await service.GetPackageManifestImportmapAsync();

        Assert.That(result.Imports["bare"], Is.EqualTo("@scope/pkg"));
        Assert.That(result.Imports["cdn"], Is.EqualTo("https://cdn.example.com/x.js"));
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_StampsScopeImports()
    {
        var manifest = new PackageManifest
        {
            Name = "Pkg",
            Version = "2.0.0",
            AllowCacheBusting = true,
            Extensions = Array.Empty<object>(),
            Importmap = new PackageManifestImportmap
            {
                Imports = new Dictionary<string, string>(),
                Scopes = new Dictionary<string, Dictionary<string, string>>
                {
                    ["/App_Plugins/Pkg/"] = new() { ["pkg"] = "/App_Plugins/Pkg/scoped.js" },
                },
            },
        };
        var service = CreateService(manifest);

        var result = await service.GetPackageManifestImportmapAsync();

        Assert.That(result.Scopes!["/App_Plugins/Pkg/"]["pkg"], Is.EqualTo("/App_Plugins/Pkg/scoped.js?v=2.0.0"));
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_WhenBustingDisabled_ResolvesTokenToVersionButDoesNotStamp()
    {
        var service = CreateService(Manifest(
            "Pkg",
            "2.0.0",
            allowCacheBusting: false,
            new Dictionary<string, string>
            {
                ["tokenised"] = "/App_Plugins/Pkg/index.js?v=%CACHE_BUSTER%",
                ["clean"] = "/App_Plugins/Pkg/clean.js",
            }));

        var result = await service.GetPackageManifestImportmapAsync();

        Assert.Multiple(() =>
        {
            // Disabling busting turns off auto-stamping only; an explicit token still resolves to the package version.
            Assert.That(result.Imports["tokenised"], Is.EqualTo("/App_Plugins/Pkg/index.js?v=2.0.0"));
            // The clean path is left untouched because automatic stamping is off for this package.
            Assert.That(result.Imports["clean"], Is.EqualTo("/App_Plugins/Pkg/clean.js"));
        });
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_AppendsHostCacheBuster()
    {
        var service = CreateService(
            "deploy-1",
            Manifest("Pkg", "2.0.0", allowCacheBusting: true, new Dictionary<string, string> { ["pkg"] = "/App_Plugins/Pkg/index.js" }));

        var result = await service.GetPackageManifestImportmapAsync();

        Assert.That(result.Imports["pkg"], Is.EqualTo("/App_Plugins/Pkg/index.js?v=2.0.0&umb__rnd=deploy-1"));
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_AppendsHostCacheBuster_WhenPackageHasNoVersion()
    {
        var service = CreateService(
            "deploy-1",
            Manifest("Pkg", null, allowCacheBusting: true, new Dictionary<string, string> { ["pkg"] = "/App_Plugins/Pkg/index.js" }));

        var result = await service.GetPackageManifestImportmapAsync();

        Assert.That(result.Imports["pkg"], Is.EqualTo("/App_Plugins/Pkg/index.js?umb__rnd=deploy-1"));
    }
}
