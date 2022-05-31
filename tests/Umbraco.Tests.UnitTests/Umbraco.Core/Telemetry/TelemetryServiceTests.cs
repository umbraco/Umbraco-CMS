using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Telemetry;

[TestFixture]
public class TelemetryServiceTests
{
    [Test]
    public void UsesGetOrCreateSiteId()
    {
        var version = CreateUmbracoVersion(9, 3, 1);
        var siteIdentifierServiceMock = new Mock<ISiteIdentifierService>();
        var usageInformationServiceMock = new Mock<IUsageInformationService>();
        var sut = new TelemetryService(
            Mock.Of<IManifestParser>(),
            version,
            siteIdentifierServiceMock.Object,
            usageInformationServiceMock.Object,
            Mock.Of<IMetricsConsentService>());
        Guid guid;

        sut.TryGetTelemetryReportData(out _);
        siteIdentifierServiceMock.Verify(x => x.TryGetOrCreateSiteIdentifier(out guid), Times.Once);
    }

    [Test]
    public void SkipsIfCantGetOrCreateId()
    {
        var version = CreateUmbracoVersion(9, 3, 1);
        var sut = new TelemetryService(
            Mock.Of<IManifestParser>(),
            version,
            CreateSiteIdentifierService(false),
            Mock.Of<IUsageInformationService>(),
            Mock.Of<IMetricsConsentService>());

        var result = sut.TryGetTelemetryReportData(out var telemetry);

        Assert.IsFalse(result);
        Assert.IsNull(telemetry);
    }

    [Test]
    public void ReturnsSemanticVersionWithoutBuild()
    {
        var version = CreateUmbracoVersion(9, 1, 1, "-rc", "-ad2f4k2d");

        var metricsConsentService = new Mock<IMetricsConsentService>();
        metricsConsentService.Setup(x => x.GetConsentLevel()).Returns(TelemetryLevel.Detailed);
        var sut = new TelemetryService(
            Mock.Of<IManifestParser>(),
            version,
            CreateSiteIdentifierService(),
            Mock.Of<IUsageInformationService>(),
            metricsConsentService.Object);

        var result = sut.TryGetTelemetryReportData(out var telemetry);

        Assert.IsTrue(result);
        Assert.AreEqual("9.1.1-rc", telemetry.Version);
    }

    [Test]
    public void CanGatherPackageTelemetry()
    {
        var version = CreateUmbracoVersion(9, 1, 1);
        var versionPackageName = "VersionPackage";
        var packageVersion = "1.0.0";
        var noVersionPackageName = "NoVersionPackage";
        PackageManifest[] manifests =
        {
            new() { PackageName = versionPackageName, Version = packageVersion },
            new() { PackageName = noVersionPackageName },
        };
        var manifestParser = CreateManifestParser(manifests);
        var metricsConsentService = new Mock<IMetricsConsentService>();
        metricsConsentService.Setup(x => x.GetConsentLevel()).Returns(TelemetryLevel.Basic);
        var sut = new TelemetryService(
            manifestParser,
            version,
            CreateSiteIdentifierService(),
            Mock.Of<IUsageInformationService>(),
            metricsConsentService.Object);

        var success = sut.TryGetTelemetryReportData(out var telemetry);

        Assert.IsTrue(success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, telemetry.Packages.Count());
            var versionPackage = telemetry.Packages.FirstOrDefault(x => x.Name == versionPackageName);
            Assert.AreEqual(versionPackageName, versionPackage.Name);
            Assert.AreEqual(packageVersion, versionPackage.Version);

            var noVersionPackage = telemetry.Packages.FirstOrDefault(x => x.Name == noVersionPackageName);
            Assert.AreEqual(noVersionPackageName, noVersionPackage.Name);
            Assert.AreEqual(string.Empty, noVersionPackage.Version);
        });
    }

    [Test]
    public void RespectsAllowPackageTelemetry()
    {
        var version = CreateUmbracoVersion(9, 1, 1);
        PackageManifest[] manifests =
        {
            new() { PackageName = "DoNotTrack", AllowPackageTelemetry = false },
            new() { PackageName = "TrackingAllowed", AllowPackageTelemetry = true },
        };
        var manifestParser = CreateManifestParser(manifests);
        var metricsConsentService = new Mock<IMetricsConsentService>();
        metricsConsentService.Setup(x => x.GetConsentLevel()).Returns(TelemetryLevel.Basic);
        var sut = new TelemetryService(
            manifestParser,
            version,
            CreateSiteIdentifierService(),
            Mock.Of<IUsageInformationService>(),
            metricsConsentService.Object);

        var success = sut.TryGetTelemetryReportData(out var telemetry);

        Assert.IsTrue(success);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, telemetry.Packages.Count());
            Assert.AreEqual("TrackingAllowed", telemetry.Packages.First().Name);
        });
    }

    private IManifestParser CreateManifestParser(IEnumerable<PackageManifest> manifests)
    {
        var manifestParserMock = new Mock<IManifestParser>();
        manifestParserMock.Setup(x => x.GetManifests()).Returns(manifests);
        return manifestParserMock.Object;
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
