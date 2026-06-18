using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Infrastructure.Manifest;
using Umbraco.Extensions;

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
        runtimeSettings.Setup(x => x.CurrentValue).Returns(new RuntimeSettings { Mode = RuntimeMode.Production, CacheBuster = cacheBuster });

        var hostingEnvironment = new Mock<IHostingEnvironment>();
        hostingEnvironment.Setup(x => x.IsDebugMode).Returns(false);

        var umbracoVersion = new Mock<IUmbracoVersion>();
        umbracoVersion.Setup(x => x.SemanticVersion).Returns(new SemVersion(17, 0, 0));

        return new PackageManifestService(
            new[] { reader.Object },
            AppCaches.Disabled,
            runtimeSettings.Object,
            hostingEnvironment.Object,
            umbracoVersion.Object);
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

    private static readonly string _globalHash = new SemVersion(17, 0, 0).ToSemanticString().GenerateHash();

    // The per-package hash for version "2.0.0": the version salted with the global hash (Umbraco version + seed).
    private static readonly string _packageHash = $"2.0.0|{_globalHash}".GenerateHash();

    private static IEnumerable<TestCaseData> SingleImportCases()
    {
        yield return new TestCaseData("2.0.0", true, "/App_Plugins/Pkg/index.js", $"/App_Plugins/Pkg/index.js?umb__rnd={_packageHash}")
            .SetName("Stamps the version hash on a clean /App_Plugins import");
        yield return new TestCaseData("2.0.0", false, "/App_Plugins/Pkg/index.js", "/App_Plugins/Pkg/index.js")
            .SetName("Does not stamp when busting is disabled");
        yield return new TestCaseData((string?)null, true, "/App_Plugins/Pkg/index.js", $"/App_Plugins/Pkg/index.js?umb__rnd={_globalHash}")
            .SetName("Falls back to the global hash when the package has no version");
        yield return new TestCaseData("2.0.0", true, "/App_Plugins/Pkg/index.js?v=%CACHE_BUSTER%", $"/App_Plugins/Pkg/index.js?v={_packageHash}")
            .SetName("Resolves an explicit %CACHE_BUSTER% token to the version hash");
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

        Assert.That(result.Scopes!["/App_Plugins/Pkg/"]["pkg"], Is.EqualTo($"/App_Plugins/Pkg/scoped.js?umb__rnd={_packageHash}"));
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_WhenBustingDisabled_ResolvesTokenToVersionHashButDoesNotStamp()
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
            // Disabling busting turns off auto-stamping only; an explicit token still resolves to the package version hash.
            Assert.That(result.Imports["tokenised"], Is.EqualTo($"/App_Plugins/Pkg/index.js?v={_packageHash}"));
            // The clean path is left untouched because automatic stamping is off for this package.
            Assert.That(result.Imports["clean"], Is.EqualTo("/App_Plugins/Pkg/clean.js"));
        });
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_SeedFlowsThroughGlobalHashIntoPackageHash()
    {
        var service = CreateService(
            "deploy-1",
            Manifest("Pkg", "2.0.0", allowCacheBusting: true, new Dictionary<string, string> { ["pkg"] = "/App_Plugins/Pkg/index.js" }));

        var result = await service.GetPackageManifestImportmapAsync();

        var seededGlobalHash = $"{_globalHash}|deploy-1".GenerateHash();
        var expected = $"2.0.0|{seededGlobalHash}".GenerateHash();
        Assert.Multiple(() =>
        {
            // The host seed is folded into the global hash, which is salted into the package hash — so the stamp moves.
            Assert.That(result.Imports["pkg"], Is.EqualTo($"/App_Plugins/Pkg/index.js?umb__rnd={expected}"));
            Assert.That(result.Imports["pkg"], Does.Not.Contain($"umb__rnd={_packageHash}"));
        });
    }
}
