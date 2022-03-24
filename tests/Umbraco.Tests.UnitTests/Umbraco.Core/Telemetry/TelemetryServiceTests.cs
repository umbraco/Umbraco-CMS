using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Telemetry.DataCollectors;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Telemetry
{
    [TestFixture]
    public class TelemetryServiceTests
    {
        [Test]
        public void CanCollectMetadataTelemetryData()
        {
            var telemetryOptions = CreateTelemetrySettings(TelemetryLevel.Basic);

            var siteIdentifier = Guid.NewGuid();
            var siteIdentifierServiceMock = new Mock<ISiteIdentifierService>();
            siteIdentifierServiceMock.Setup(x => x.TryGetSiteIdentifier(out siteIdentifier)).Returns(true);

            var telemetryDataCollectors = new[]
            {
                new MetadataTelemetryDataCollector(siteIdentifierServiceMock.Object)
            };

            var telemetryService = new TelemetryService(telemetryOptions, telemetryDataCollectors);

            var result = telemetryService.TryGetTelemetryReportData(out var telemetry);

            Assert.IsTrue(result);

            Assert.Contains(TelemetryData.TelemetryId, telemetry.Keys);
            Assert.AreEqual(siteIdentifier, telemetry[TelemetryData.TelemetryId]);

            Assert.Contains(TelemetryData.Network, telemetry.Keys);
            Assert.AreEqual(true, telemetry[TelemetryData.Network]);
        }

        [Test]
        public void CanCollectUmbracoVersionTelemetryData()
        {
            var telemetryOptions = CreateTelemetrySettings(TelemetryLevel.Basic);
            var umbracoVersion = Mock.Of<IUmbracoVersion>(x => x.SemanticVersion == new SemVersion(9, 1, 1, "-rc", "-ad2f4k2d"));
            var telemetryDataCollectors = new[]
            {
                new UmbracoVersionTelemetryDataCollector(umbracoVersion)
            };

            var telemetryService = new TelemetryService(telemetryOptions, telemetryDataCollectors);

            var result = telemetryService.TryGetTelemetryReportData(out var telemetry);

            Assert.IsTrue(result);

            Assert.Contains(TelemetryData.UmbracoVersion, telemetry.Keys);
            Assert.AreEqual("9.1.1-rc", telemetry[TelemetryData.UmbracoVersion]);
        }

        [Test]
        public void CanCollectPackageVersionsTelemetryData()
        {
            var telemetryOptions = CreateTelemetrySettings(TelemetryLevel.Basic);
            var currentAssembly = GetType().Assembly;

            var versionPackageName = "VersionPackage";
            var packageVersion = "1.0.0";
            var noVersionPackageName = "NoVersionPackage";
            var doNotTrackPackageName = "DoNotTrack";
            var trackingAllowedPackageName = "TrackingAllowed";
            var assemblyVersionPackageName = currentAssembly.GetName().Name;
            var manifestParserMock = new Mock<IManifestParser>();
            manifestParserMock.Setup(x => x.GetManifests()).Returns(new[]
            {
                new PackageManifest() { PackageName = versionPackageName, Version = packageVersion },
                new PackageManifest() { PackageName = noVersionPackageName },
                new PackageManifest() { PackageName = doNotTrackPackageName, AllowPackageTelemetry = false },
                new PackageManifest() { PackageName = trackingAllowedPackageName, AllowPackageTelemetry = true },
                new PackageManifest() { PackageName = assemblyVersionPackageName }
            });

            var typeFinderMock = new Mock<ITypeFinder>();
            typeFinderMock.Setup(x => x.AssembliesToScan).Returns(new[]
            {
                currentAssembly
            });

            var telemetryDataCollectors = new[]
            {
                new PackageVersionsTelemetryDataCollector(manifestParserMock.Object, typeFinderMock.Object)
            };

            var telemetryService = new TelemetryService(telemetryOptions, telemetryDataCollectors);

            var result = telemetryService.TryGetTelemetryReportData(out var telemetry);

            Assert.IsTrue(result);

            Assert.Contains(TelemetryData.PackageVersions, telemetry.Keys);

            Assert.Multiple(() =>
            {
                var packages = telemetry[TelemetryData.PackageVersions] as IEnumerable<PackageTelemetry>;

                Assert.NotNull(packages);
                Assert.AreEqual(4, packages.Count());

                var versionPackage = packages.FirstOrDefault(x => x.Name == versionPackageName);
                Assert.IsNotNull(versionPackage);
                Assert.AreEqual(packageVersion, versionPackage.Version);

                var noVersionPackage = packages.FirstOrDefault(x => x.Name == noVersionPackageName);
                Assert.IsNotNull(noVersionPackage);
                Assert.AreEqual(null, noVersionPackage.Version);

                var trackingAllowedPackage = packages.FirstOrDefault(x => x.Name == trackingAllowedPackageName);
                Assert.IsNotNull(trackingAllowedPackage);

                var assemblyVersionPackage = packages.FirstOrDefault(x => x.Name == assemblyVersionPackageName);
                Assert.IsNotNull(assemblyVersionPackage);
                Assert.AreEqual(currentAssembly.GetName().Version.ToString(3), assemblyVersionPackage.Version);
            });
        }

        private IOptionsMonitor<TelemetrySettings> CreateTelemetrySettings(TelemetryLevel level)
        {
            var telemetrySettings = new TelemetrySettings();
            telemetrySettings.Set(level);

            return Mock.Of<IOptionsMonitor<TelemetrySettings>>(x => x.CurrentValue == telemetrySettings);
        }
    }
}
