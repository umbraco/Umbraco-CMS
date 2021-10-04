using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services
{
    [TestFixture]
    public class UserDataServiceTests
    {
        private IUmbracoVersion _umbracoVersion;
        private ILocalizationService _localizationService;

        [OneTimeSetUp]
        public void CreateMocks() => CreateUmbracoVersion(9, 0, 0);

        [Test]
        [TestCase("en-US")]
        [TestCase("de-DE")]
        [TestCase("en-NZ")]
        [TestCase("sv-SE")]
        public void GetCorrectDefaultLanguageTest(string culture)
        {
            CreateLocalizationVersion(culture);
            var userDataService = new UserDataService(_umbracoVersion, _localizationService);
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
            CreateLocalizationVersion(culture);
            var userDataService = new UserDataService(_umbracoVersion, _localizationService);
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
            CreateLocalizationVersion(culture);
            var userDataService = new UserDataService(_umbracoVersion, _localizationService);
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
            CreateLocalizationVersion(culture);
            var userDataService = new UserDataService(_umbracoVersion, _localizationService);
            IEnumerable<UserData> userData = userDataService.GetUserData();
            Assert.Multiple(() =>
            {
                Assert.IsNotNull(userData.FirstOrDefault(x => x.Name == "Server OS"));
                Assert.IsNotNull(userData.FirstOrDefault(x => x.Name == "Server Framework"));
                Assert.IsNotNull(userData.FirstOrDefault(x => x.Name == "Current Webserver"));
            });
        }

        private void CreateLocalizationVersion(string culture)
        {
            var localizationService = new Mock<ILocalizationService>();
            localizationService.Setup(x => x.GetDefaultLanguageIsoCode()).Returns(culture);
            _localizationService = localizationService.Object;
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
