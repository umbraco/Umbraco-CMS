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

/// <summary>
/// Contains unit tests for the <see cref="SystemInformationService"/> class.
/// </summary>
[TestFixture]
public class SystemInformationServiceTests
{
    /// <summary>
    /// Creates the necessary mocks for the tests.
    /// </summary>
    [OneTimeSetUp]
    public void CreateMocks() => CreateUmbracoVersion(9, 0, 0);

    private IUmbracoVersion _umbracoVersion;

    /// <summary>
    /// Tests that the default language returned by the system information service matches the expected culture.
    /// </summary>
    /// <param name="culture">The culture code to test as the default language.</param>
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
            Assert.IsNotNull(defaultLanguage);
            Assert.AreEqual(culture, defaultLanguage.Value);
        });
    }

    /// <summary>
    /// Verifies that the <see cref="SystemInformationService"/> correctly reports the current thread's culture.
    /// </summary>
    /// <param name="culture">The culture name (e.g., "en-US") to set as the current thread's culture for the test.</param>
    /// <remarks>
    /// This test sets the thread's culture, invokes the service, and asserts that the reported culture matches the expected value.
    /// </remarks>
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
            Assert.IsNotNull(currentCulture);
            Assert.AreEqual(culture, currentCulture.Value);
        });
    }

    /// <summary>
    /// Verifies that the SystemInformationService returns the correct current UI culture
    /// based on the culture string set for the current thread.
    /// </summary>
    /// <param name="culture">The culture name (e.g., "en-US") to set as the current UI culture for the test execution.</param>
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
            Assert.IsNotNull(currentCulture);
            Assert.AreEqual(culture, currentCulture.Value);
        });
    }

    /// <summary>
    /// Verifies that the system information service returns non-null troubleshooting information for the specified culture.
    /// Specifically, checks that the entries for "Server OS", "Server Framework", and "Current Webserver" are present and not null.
    /// </summary>
    /// <param name="culture">The culture code to use for the test (e.g., "en-US").</param>
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
            Assert.IsNotNull(userData.Select(x => x.Key == "Server OS"));
            Assert.IsNotNull(userData.Select(x => x.Key == "Server Framework"));
            Assert.IsNotNull(userData.Select(x => x.Key == "Current Webserver"));
        });
    }

    /// <summary>
    /// Tests that the system information service reports the correct Models Builder mode.
    /// </summary>
    /// <param name="modelsMode">The Models Builder mode to test.</param>
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
        Assert.IsNotNull(actual.Value);
        Assert.AreEqual(modelsMode, actual.Value);
    }

    /// <summary>
    /// Tests that the system information service reports the correct runtime mode.
    /// </summary>
    /// <param name="runtimeMode">The runtime mode to test.</param>
    [Test]
    [TestCase(RuntimeMode.BackofficeDevelopment)]
    [TestCase(RuntimeMode.Development)]
    [TestCase(RuntimeMode.Production)]
    public void ReportsRuntimeModeCorrectly(RuntimeMode runtimeMode)
    {
        var userDataService = CreateSystemInformationService(runtimeMode: runtimeMode);
        var userData = userDataService.GetTroubleshootingInformation().ToArray();

        var actual = userData.FirstOrDefault(x => x.Key == "Runtime Mode");
        Assert.IsNotNull(actual.Value);
        Assert.AreEqual(runtimeMode.ToString(), actual.Value);
    }

    /// <summary>
    /// Verifies that the system information service correctly reports the status of debug mode
    /// based on the provided <paramref name="isDebug"/> parameter.
    /// </summary>
    /// <param name="isDebug">If set to <c>true</c>, debug mode is enabled; otherwise, it is disabled.</param>
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReportsDebugModeCorrectly(bool isDebug)
    {
        var userDataService = CreateSystemInformationService(isDebug: isDebug);
        var userData = userDataService.GetTroubleshootingInformation().ToArray();

        var actual = userData.FirstOrDefault(x => x.Key == "Debug Mode");
        Assert.IsNotNull(actual.Value);
        Assert.AreEqual(isDebug.ToString(), actual.Value);
    }

    private ISystemTroubleshootingInformationService CreateSystemInformationService(
        string culture = "",
        string modelsMode = ModelsModeConstants.InMemoryAuto,
        bool isDebug = true,
        RuntimeMode runtimeMode = RuntimeMode.BackofficeDevelopment)
    {
        var localizationService = CreateILocalizationService(culture);

        var databaseMock = new Mock<IUmbracoDatabase>();
        databaseMock.Setup(x => x.DatabaseType.GetProviderName()).Returns("SQL");

        return new SystemTroubleshootingInformationTelemetryProvider(
            _umbracoVersion,
            localizationService,
            Mock.Of<IOptionsMonitor<ModelsBuilderSettings>>(x => x.CurrentValue == new ModelsBuilderSettings { ModelsMode = modelsMode }),
            Mock.Of<IOptionsMonitor<HostingSettings>>(x => x.CurrentValue == new HostingSettings { Debug = isDebug }),
            Mock.Of<IHostEnvironment>(),
            Mock.Of<IUmbracoDatabaseFactory>(x => x.CreateDatabase() == Mock.Of<IUmbracoDatabase>(y => y.DatabaseType == DatabaseType.SQLite)),
            Mock.Of<IServerRoleAccessor>(),
            Mock.Of<IOptionsMonitor<RuntimeSettings>>(x => x.CurrentValue == new RuntimeSettings { Mode = runtimeMode }));
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
