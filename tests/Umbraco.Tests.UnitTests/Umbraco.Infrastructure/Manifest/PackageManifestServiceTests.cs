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
    {
        var reader = new Mock<IPackageManifestReader>();
        reader.Setup(x => x.ReadPackageManifestsAsync()).ReturnsAsync(manifests);

        var runtimeSettings = new Mock<IOptionsMonitor<RuntimeSettings>>();
        runtimeSettings.Setup(x => x.CurrentValue).Returns(new RuntimeSettings { Mode = RuntimeMode.Production });

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

    [Test]
    public async Task GetPackageManifestImportmapAsync_StampsVersionHashOnAppPluginsImport()
    {
        var service = CreateService(Manifest(
            "Pkg",
            "2.0.0",
            allowCacheBusting: true,
            new Dictionary<string, string> { ["pkg"] = "/App_Plugins/Pkg/index.js" }));

        var result = await service.GetPackageManifestImportmapAsync();

        Assert.That(result.Imports["pkg"], Is.EqualTo($"/App_Plugins/Pkg/index.js?umb__rnd={"2.0.0".GenerateHash()}"));
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_DoesNotStampWhenDisabled()
    {
        var service = CreateService(Manifest(
            "Pkg",
            "2.0.0",
            allowCacheBusting: false,
            new Dictionary<string, string> { ["pkg"] = "/App_Plugins/Pkg/index.js" }));

        var result = await service.GetPackageManifestImportmapAsync();

        Assert.That(result.Imports["pkg"], Is.EqualTo("/App_Plugins/Pkg/index.js"));
    }

    [Test]
    public async Task GetPackageManifestImportmapAsync_FallsBackToGlobalHashWhenNoVersion()
    {
        var service = CreateService(Manifest(
            "Pkg",
            version: null,
            allowCacheBusting: true,
            new Dictionary<string, string> { ["pkg"] = "/App_Plugins/Pkg/index.js" }));

        var result = await service.GetPackageManifestImportmapAsync();

        var expectedGlobal = new SemVersion(17, 0, 0).ToSemanticString().GenerateHash();
        Assert.That(result.Imports["pkg"], Is.EqualTo($"/App_Plugins/Pkg/index.js?umb__rnd={expectedGlobal}"));
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

        Assert.That(result.Scopes!["/App_Plugins/Pkg/"]["pkg"], Is.EqualTo($"/App_Plugins/Pkg/scoped.js?umb__rnd={"2.0.0".GenerateHash()}"));
    }
}
