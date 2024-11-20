using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Telemetry;

[TestFixture]
public class TelemetryServiceTests
{
    [Test]
    public async Task UsesGetOrCreateSiteId()
    {
        var version = CreateUmbracoVersion(9, 3, 1);
        var siteIdentifierServiceMock = new Mock<ISiteIdentifierService>();
        var usageInformationServiceMock = new Mock<IUsageInformationService>();
        var sut = new TelemetryService(
            Mock.Of<IPackagingService>(),
            version,
            siteIdentifierServiceMock.Object,
            usageInformationServiceMock.Object,
            Mock.Of<IMetricsConsentService>());
        Guid guid;

        await sut.GetTelemetryReportDataAsync();
        siteIdentifierServiceMock.Verify(x => x.TryGetOrCreateSiteIdentifier(out guid), Times.Once);
    }

    [Test]
    public async Task SkipsIfCantGetOrCreateId()
    {
        var version = CreateUmbracoVersion(9, 3, 1);
        var sut = new TelemetryService(
            Mock.Of<IPackagingService>(),
            version,
            CreateSiteIdentifierService(false),
            Mock.Of<IUsageInformationService>(),
            Mock.Of<IMetricsConsentService>());

        var result = await sut.GetTelemetryReportDataAsync();
        Assert.IsNull(result);
    }

    [Test]
    public async Task ReturnsSemanticVersionWithoutBuild()
    {
        var version = CreateUmbracoVersion(9, 1, 1, "-rc", "-ad2f4k2d");

        var metricsConsentService = new Mock<IMetricsConsentService>();
        metricsConsentService.Setup(x => x.GetConsentLevel()).Returns(TelemetryLevel.Detailed);
        var sut = new TelemetryService(
            Mock.Of<IPackagingService>(),
            version,
            CreateSiteIdentifierService(),
            Mock.Of<IUsageInformationService>(),
            metricsConsentService.Object);

        var result = await sut.GetTelemetryReportDataAsync();

        Assert.IsNotNull(result);
        Assert.AreEqual("9.1.1-rc", result.Version);
    }

    [Test]
    public async Task CanGatherPackageTelemetry()
    {
        var version = CreateUmbracoVersion(9, 1, 1);
        var versionPackageId = "VersionPackageId";
        var versionPackageName = "VersionPackageName";
        var packageVersion = "1.0.0";
        var noVersionPackageName = "NoVersionPackageName";
        InstalledPackage[] installedPackages =
        {
            new() { PackageId = versionPackageId, PackageName = versionPackageName, Version = packageVersion },
            new() { PackageName = noVersionPackageName },
        };
        var packagingService = CreatePackagingService(installedPackages);
        var metricsConsentService = new Mock<IMetricsConsentService>();
        metricsConsentService.Setup(x => x.GetConsentLevel()).Returns(TelemetryLevel.Detailed);
        var sut = new TelemetryService(
            packagingService,
            version,
            CreateSiteIdentifierService(),
            Mock.Of<IUsageInformationService>(),
            metricsConsentService.Object);

        var result = await sut.GetTelemetryReportDataAsync();

        Assert.IsNotNull(result);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, result.Packages.Count());
            var versionPackage = result.Packages.FirstOrDefault(x => x.Name == versionPackageName);
            Assert.AreEqual(versionPackageId, versionPackage.Id);
            Assert.AreEqual(versionPackageName, versionPackage.Name);
            Assert.AreEqual(packageVersion, versionPackage.Version);

            var noVersionPackage = result.Packages.FirstOrDefault(x => x.Name == noVersionPackageName);
            Assert.AreEqual(null, noVersionPackage.Id);
            Assert.AreEqual(noVersionPackageName, noVersionPackage.Name);
            Assert.AreEqual(null, noVersionPackage.Version);
        });
    }

    [Test]
    public async Task RespectsAllowPackageTelemetry()
    {
        var version = CreateUmbracoVersion(9, 1, 1);
        InstalledPackage[] installedPackages =
        {
            new() { PackageName = "DoNotTrack", AllowPackageTelemetry = false},
            new() { PackageName = "TrackingAllowed", AllowPackageTelemetry = true },
        };
        var packagingService = CreatePackagingService(installedPackages);
        var metricsConsentService = new Mock<IMetricsConsentService>();
        metricsConsentService.Setup(x => x.GetConsentLevel()).Returns(TelemetryLevel.Detailed);
        var sut = new TelemetryService(
            packagingService,
            version,
            CreateSiteIdentifierService(),
            Mock.Of<IUsageInformationService>(),
            metricsConsentService.Object);

        var result = await sut.GetTelemetryReportDataAsync();

        Assert.IsNotNull(result);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.Packages.Count());
            Assert.AreEqual("TrackingAllowed", result.Packages.First().Name);
        });
    }

    private IPackagingService CreatePackagingService(IEnumerable<InstalledPackage> installedPackages)
    {
        var packagingServiceMock = new Mock<IPackagingService>();
        packagingServiceMock.Setup(x => x.GetAllInstalledPackagesAsync()).ReturnsAsync(installedPackages);
        return packagingServiceMock.Object;
    }

    private IUmbracoVersion CreateUmbracoVersion(int major, int minor, int patch, string prerelease = "", string build = "")
    {
        var version = new SemVersion(major, minor, patch, prerelease, build);
        return Mock.Of<IUmbracoVersion>(x => x.SemanticVersion == version);
    }

    private ISiteIdentifierService CreateSiteIdentifierService(bool shouldSucceed = true)
    {
        var mock = new Mock<ISiteIdentifierService>();
        var siteIdentifier = shouldSucceed ? Guid.NewGuid() : Guid.Empty;
        mock.Setup(x => x.TryGetOrCreateSiteIdentifier(out siteIdentifier)).Returns(shouldSucceed);
        return mock.Object;
    }
}
