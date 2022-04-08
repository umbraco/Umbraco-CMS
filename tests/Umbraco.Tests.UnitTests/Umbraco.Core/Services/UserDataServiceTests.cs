using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Providers.Providers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services
{
    [TestFixture]
    public class UserDataServiceTests
    {
        private IUmbracoVersion _umbracoVersion;

        [OneTimeSetUp]
        public void CreateMocks() => CreateUmbracoVersion(9, 0, 0);

        [Test]
        [TestCase("en-US")]
        [TestCase("de-DE")]
        [TestCase("en-NZ")]
        [TestCase("sv-SE")]
        public void GetCorrectDefaultLanguageTest(string culture)
        {
            var userDataService = CreateUserDataService(culture);
            var defaultLanguage = userDataService.GetUserData().FirstOrDefault(x => x.Name == "Default Language");
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(defaultLanguage);
                Assert.AreEqual(culture, defaultLanguage.Data);
            });
        }

        [Test]
        [TestCase("en-US")]
        [TestCase("de-DE")]
        [TestCase("en-NZ")]
        [TestCase("sv-SE")]
        public void GetCorrectCultureTest(string culture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var userDataService = CreateUserDataService(culture);
            var currentCulture = userDataService.GetUserData().FirstOrDefault(x => x.Name == "Current Culture");
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(currentCulture);
                Assert.AreEqual(culture, currentCulture.Data);
            });
        }

        [Test]
        [TestCase("en-US")]
        [TestCase("de-DE")]
        [TestCase("en-NZ")]
        [TestCase("sv-SE")]
        public void GetCorrectUICultureTest(string culture)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
            var userDataService = CreateUserDataService(culture);
            var currentCulture = userDataService.GetUserData().FirstOrDefault(x => x.Name == "Current UI Culture");
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(currentCulture);
                Assert.AreEqual(culture, currentCulture.Data);
            });
        }

        [Test]
        [TestCase("en-US")]
        [TestCase("de-DE")]
        [TestCase("en-NZ")]
        [TestCase("sv-SE")]
        public void RunTimeInformationNotNullTest(string culture)
        {
            var userDataService = CreateUserDataService(culture);
            IEnumerable<UserData> userData = userDataService.GetUserData().ToList();
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(userData.Select(x => x.Name == "Server OS"));
                Assert.IsNotNull(userData.Select(x => x.Name == "Server Framework"));
                Assert.IsNotNull(userData.Select(x => x.Name == "Current Webserver"));
            });
        }

        [Test]
        [TestCase(ModelsMode.Nothing)]
        [TestCase(ModelsMode.InMemoryAuto)]
        [TestCase(ModelsMode.SourceCodeAuto)]
        [TestCase(ModelsMode.SourceCodeManual)]
        public void ReportsModelsModeCorrectly(ModelsMode modelsMode)
        {
            var userDataService = CreateUserDataService(modelsMode: modelsMode);
            UserData[] userData = userDataService.GetUserData().ToArray();

            var actual = userData.FirstOrDefault(x => x.Name == "Models Builder Mode");
            Assert.IsNotNull(actual?.Data);
            Assert.AreEqual(modelsMode.ToString(), actual.Data);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void ReportsDebugModeCorrectly(bool isDebug)
        {
            var userDataService = CreateUserDataService(isDebug: isDebug);
            UserData[] userData = userDataService.GetUserData().ToArray();

            var actual = userData.FirstOrDefault(x => x.Name == "Debug Mode");
            Assert.IsNotNull(actual?.Data);
            Assert.AreEqual(isDebug.ToString(), actual.Data);
        }

        private SystemInformationTelemetryProvider CreateUserDataService(string culture = "", ModelsMode modelsMode = ModelsMode.InMemoryAuto, bool isDebug = true)
        {
            var localizationService = CreateILocalizationService(culture);
            return new SystemInformationTelemetryProvider(
                _umbracoVersion,
                localizationService,
                Mock.Of<IOptions<ModelsBuilderSettings>>(x => x.Value == new ModelsBuilderSettings { ModelsMode = modelsMode }),
                Mock.Of<IOptions<HostingSettings>>(x => x.Value == new HostingSettings { Debug = isDebug }),
                Mock.Of<IOptions<GlobalSettings>>(x => x.Value == new GlobalSettings()),
                Mock.Of<IHostEnvironment>());
        }

        private ILocalizationService CreateILocalizationService(string culture)
        {
            var localizationService = new Mock<ILocalizationService>();
            localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns(culture);
            return localizationService.Object;
        }

        private void CreateUmbracoVersion(int major, int minor, int patch)
        {
            var umbracoVersion = new Mock<IUmbracoVersion>();
            var semVersion = new SemVersion(major, minor, patch);
            umbracoVersion.Setup(x => x.SemanticVersion).Returns(semVersion);
            _umbracoVersion = umbracoVersion.Object;
        }
    }
}
