using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.DevelopmentMode.Backoffice.InMemoryAuto;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Telemetry.Providers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class SystemInformationServiceTests
{
    [OneTimeSetUp]
    public void CreateMocks() => CreateUmbracoVersion(9, 0, 0);

    private IUmbracoVersion _umbracoVersion;

    [Test]
    [TestCase("en-US")]
    [TestCase("de-DE")]
    [TestCase("en-NZ")]
    [TestCase("sv-SE")]
    public void GetCorrectDefaultLanguageTest(string culture)
    {
        var userDataService = CreateSystemInformationService(culture);
        var defaultLanguage = userDataService.GetTroubleshootingInformation().FirstOrDefault(x => x.Key == "Default Language");
        Assert.Multiple(() =>
        {
            Assert.That(defaultLanguage, Is.Not.Null);
            Assert.That(defaultLanguage.Value, Is.EqualTo(culture));
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
        var userDataService = CreateSystemInformationService(culture);
        var currentCulture = userDataService.GetTroubleshootingInformation().FirstOrDefault(x => x.Key == "Current Culture");
        Assert.Multiple(() =>
        {
            Assert.That(currentCulture, Is.Not.Null);
            Assert.That(currentCulture.Value, Is.EqualTo(culture));
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
        var userDataService = CreateSystemInformationService(culture);
        var currentCulture = userDataService.GetTroubleshootingInformation().FirstOrDefault(x => x.Key == "Current UI Culture");
        Assert.Multiple(() =>
        {
            Assert.That(currentCulture, Is.Not.Null);
            Assert.That(currentCulture.Value, Is.EqualTo(culture));
        });
    }

    [Test]
    [TestCase("en-US")]
    [TestCase("de-DE")]
    [TestCase("en-NZ")]
    [TestCase("sv-SE")]
    public void RunTimeInformationNotNullTest(string culture)
    {
        var userDataService = CreateSystemInformationService(culture);
        var userData = userDataService.GetTroubleshootingInformation().ToList();
        Assert.Multiple(() =>
        {
            Assert.That(userData.Select(x => x.Key == "Server OS"), Is.Not.Null);
            Assert.That(userData.Select(x => x.Key == "Server Framework"), Is.Not.Null);
            Assert.That(userData.Select(x => x.Key == "Current Webserver"), Is.Not.Null);
        });
    }

    [Test]
    [TestCase(Constants.ModelsBuilder.ModelsModes.Nothing)]
    [TestCase(ModelsModeConstants.InMemoryAuto)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeAuto)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeManual)]
    public void ReportsModelsModeCorrectly(string modelsMode)
    {
        var userDataService = CreateSystemInformationService(modelsMode: modelsMode);
        var userData = userDataService.GetTroubleshootingInformation().ToArray();

        var actual = userData.FirstOrDefault(x => x.Key == "Models Builder Mode");
        Assert.That(actual.Value, Is.Not.Null);
        Assert.That(actual.Value, Is.EqualTo(modelsMode));
    }

    [Test]
    [TestCase(RuntimeMode.BackofficeDevelopment)]
    [TestCase(RuntimeMode.Development)]
    [TestCase(RuntimeMode.Production)]
    public void ReportsRuntimeModeCorrectly(RuntimeMode runtimeMode)
    {
        var userDataService = CreateSystemInformationService(runtimeMode: runtimeMode);
        var userData = userDataService.GetTroubleshootingInformation().ToArray();

        var actual = userData.FirstOrDefault(x => x.Key == "Runtime Mode");
        Assert.That(actual.Value, Is.Not.Null);
        Assert.That(actual.Value, Is.EqualTo(runtimeMode.ToString()));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReportsDebugModeCorrectly(bool isDebug)
    {
        var userDataService = CreateSystemInformationService(isDebug: isDebug);
        var userData = userDataService.GetTroubleshootingInformation().ToArray();

        var actual = userData.FirstOrDefault(x => x.Key == "Debug Mode");
        Assert.That(actual.Value, Is.Not.Null);
        Assert.That(actual.Value, Is.EqualTo(isDebug.ToString()));
    }

    private ISystemTroubleshootingInformationService CreateSystemInformationService(
        string culture = "",
        string modelsMode = ModelsModeConstants.InMemoryAuto,
        bool isDebug = true,
        RuntimeMode runtimeMode = RuntimeMode.BackofficeDevelopment)
    {
        var languageService = CreateILanguageService(culture);

        var databaseMock = new Mock<IUmbracoDatabase>();
        databaseMock.Setup(x => x.DatabaseType.GetProviderName()).Returns("SQL");

        return new SystemTroubleshootingInformationTelemetryProvider(
            _umbracoVersion,
            languageService,
            Mock.Of<IOptionsMonitor<ModelsBuilderSettings>>(x => x.CurrentValue == new ModelsBuilderSettings { ModelsMode = modelsMode }),
            Mock.Of<IOptionsMonitor<HostingSettings>>(x => x.CurrentValue == new HostingSettings { Debug = isDebug }),
            Mock.Of<IHostEnvironment>(),
            Mock.Of<IUmbracoDatabaseFactory>(x => x.CreateDatabase() == Mock.Of<IUmbracoDatabase>(y => y.DatabaseType == DatabaseType.SQLite)),
            Mock.Of<IServerRoleAccessor>(),
            Mock.Of<IOptionsMonitor<RuntimeSettings>>(x => x.CurrentValue == new RuntimeSettings { Mode = runtimeMode }));
    }

    private ILanguageService CreateILanguageService(string culture)
    {
        var languageService = new Mock<ILanguageService>();
        languageService.Setup(x => x.GetDefaultIsoCodeAsync()).ReturnsAsync(culture);
        return languageService.Object;
    }

    private void CreateUmbracoVersion(int major, int minor, int patch)
    {
        var umbracoVersion = new Mock<IUmbracoVersion>();
        var semVersion = new SemVersion(major, minor, patch);
        umbracoVersion.Setup(x => x.SemanticVersion).Returns(semVersion);
        _umbracoVersion = umbracoVersion.Object;
    }
}
