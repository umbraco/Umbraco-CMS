using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Telemetry;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Telemetry
{
    [TestFixture]
    public class TelemetryServiceTests
    {
        [TestCase("0F1785C5-7BA0-4C52-AB62-863BD2C8F3FE", true)]
        [TestCase("This is not a guid", false)]
        [TestCase("", false)]
        public void OnlyParsesIfValidId(string guidString, bool shouldSucceed)
        {
            var telemetryOptions = CreateTelemetryOptions(TelemetryLevel.Basic);
            var globalSettings = CreateGlobalSettings(guidString);
            var version = CreateUmbracoVersion(9, 1, 1);
            var sut = new TelemetryService(telemetryOptions, globalSettings, Mock.Of<IManifestParser>(), version);

            var result = sut.TryGetTelemetryReportData(out var telemetry);

            Assert.AreEqual(shouldSucceed, result);
            if (shouldSucceed)
            {
                // When toString is called on a GUID it will to lower, so do the same to our guidString
                Assert.AreEqual(guidString.ToLower(), telemetry.Id.ToString());
            }
            else
            {
                Assert.IsNull(telemetry);
            }
        }

        [Test]
        public void ReturnsSemanticVersionWithoutBuild()
        {
            var telemetryOptions = CreateTelemetryOptions(TelemetryLevel.Basic);
            var globalSettings = CreateGlobalSettings();
            var version = CreateUmbracoVersion(9, 1, 1, "-rc", "-ad2f4k2d");

            var sut = new TelemetryService(telemetryOptions, globalSettings, Mock.Of<IManifestParser>(), version);

            var result = sut.TryGetTelemetryReportData(out var telemetry);

            Assert.IsTrue(result);
            Assert.AreEqual("9.1.1-rc", telemetry.Version);
        }

        [Test]
        public void CanGatherPackageTelemetry()
        {
            var telemetryOptions = CreateTelemetryOptions(TelemetryLevel.Basic);
            var globalSettings = CreateGlobalSettings();
            var version = CreateUmbracoVersion(9, 1, 1);
            var versionPackageName = "VersionPackage";
            var packageVersion = "1.0.0";
            var noVersionPackageName = "NoVersionPackage";
            PackageManifest[] manifests =
            {
                new () { PackageName = versionPackageName, Version = packageVersion },
                new () { PackageName = noVersionPackageName }
            };
            var manifestParser = CreateManifestParser(manifests);
            var sut = new TelemetryService(telemetryOptions, globalSettings, manifestParser, version);

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
            var telemetryOptions = CreateTelemetryOptions(TelemetryLevel.Basic);
            var globalSettings = CreateGlobalSettings();
            var version = CreateUmbracoVersion(9, 1, 1);
            PackageManifest[] manifests =
            {
                new () { PackageName = "DoNotTrack", AllowPackageTelemetry = false },
                new () { PackageName = "TrackingAllowed", AllowPackageTelemetry = true }
            };
            var manifestParser = CreateManifestParser(manifests);
            var sut = new TelemetryService(telemetryOptions, globalSettings, manifestParser, version);

            var success = sut.TryGetTelemetryReportData(out var telemetry);

            Assert.IsTrue(success);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, telemetry.Packages.Count());
                Assert.AreEqual("TrackingAllowed", telemetry.Packages.First().Name);
            });
        }

        private IOptionsMonitor<TelemetrySettings> CreateTelemetryOptions(TelemetryLevel level)
        {
            var telemetryOptions = new TelemetrySettings();
            telemetryOptions.Set(level);

            return Mock.Of<IOptionsMonitor<TelemetrySettings>>(x => x.CurrentValue == telemetryOptions);
        }

        private IOptionsMonitor<GlobalSettings> CreateGlobalSettings(string guidString = null)
        {
            var globalSettings = new GlobalSettings
            {
                Id = guidString ?? Guid.NewGuid().ToString()
            };

            return Mock.Of<IOptionsMonitor<GlobalSettings>>(x => x.CurrentValue == globalSettings);
        }

        private IUmbracoVersion CreateUmbracoVersion(int major, int minor, int patch, string prerelease = "", string build = "")
        {
            var version = new SemVersion(major, minor, patch, prerelease, build);
            return Mock.Of<IUmbracoVersion>(x => x.SemanticVersion == version);
        }

        private IManifestParser CreateManifestParser(IEnumerable<PackageManifest> manifests)
        {
            var manifestParserMock = new Mock<IManifestParser>();
            manifestParserMock.Setup(x => x.GetManifests()).Returns(manifests);
            return manifestParserMock.Object;
        }
    }
}
