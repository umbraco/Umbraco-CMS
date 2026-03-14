using System.Globalization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.DevelopmentMode.Backoffice.InMemoryAuto;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Telemetry.Providers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Telemetry;

/// <summary>
/// Contains unit tests that verify the functionality of the <see cref="SystemTroubleshootingInformationTelemetryProvider"/> class.
/// </summary>
[TestFixture]
public class SystemTroubleshootingInformationTelemetryProviderTests
{
    /// <summary>
    /// Tests that the telemetry provider reports the models mode correctly.
    /// </summary>
    /// <param name="modelsMode">The models mode to test.</param>
    [Test]
    [TestCase(Constants.ModelsBuilder.ModelsModes.Nothing)]
    [TestCase(ModelsModeConstants.InMemoryAuto)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeAuto)]
    [TestCase(Constants.ModelsBuilder.ModelsModes.SourceCodeManual)]
    public void ReportsModelsModeCorrectly(string modelsMode)
    {
        var telemetryProvider = CreateProvider(modelsMode);
        var usageInformation = telemetryProvider.GetInformation().ToArray();

        var actual = usageInformation.FirstOrDefault(x => x.Name == Constants.Telemetry.ModelsBuilderMode);
        Assert.IsNotNull(actual?.Data);
        Assert.AreEqual(modelsMode, actual.Data);
    }

    /// <summary>
    /// Tests that the telemetry provider reports the runtime mode correctly.
    /// </summary>
    /// <param name="runtimeMode">The runtime mode to test.</param>
    [Test]
    [TestCase(RuntimeMode.BackofficeDevelopment)]
    [TestCase(RuntimeMode.BackofficeDevelopment)]
    [TestCase(RuntimeMode.BackofficeDevelopment)]

    public void ReportsRuntimeModeCorrectly(RuntimeMode runtimeMode)
    {
        var telemetryProvider = CreateProvider(runtimeMode: runtimeMode);
        var usageInformation = telemetryProvider.GetInformation().ToArray();

        var actual = usageInformation.FirstOrDefault(x => x.Name == Constants.Telemetry.RuntimeMode);
        Assert.IsNotNull(actual?.Data);
        Assert.AreEqual(runtimeMode.ToString(), actual.Data);
    }

    /// <summary>
    /// Verifies that the telemetry provider correctly reports the debug mode status based on the input parameter.
    /// </summary>
    /// <param name="isDebug">A boolean value indicating whether debug mode is enabled for the test case.</param>
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ReportsDebugModeCorrectly(bool isDebug)
    {
        var telemetryProvider = CreateProvider(isDebug: isDebug);
        var usageInformation = telemetryProvider.GetInformation().ToArray();

        var actual = usageInformation.FirstOrDefault(x => x.Name == Constants.Telemetry.IsDebug);
        Assert.IsNotNull(actual?.Data);
        Assert.AreEqual(isDebug, actual.Data);
    }

    /// <summary>
    /// Tests that the OS language is reported correctly by the telemetry provider for the given culture.
    /// </summary>
    /// <param name="culture">The culture string to set as the current thread's culture.</param>
    [Test]
    [TestCase("en-US")]
    [TestCase("de-DE")]
    [TestCase("en-NZ")]
    [TestCase("sv-SE")]
    public void ReportsOsLanguageCorrectly(string culture)
    {
        Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
        var telemetryProvider = CreateProvider();

        var usageInformation = telemetryProvider.GetInformation().ToArray();
        var actual = usageInformation.FirstOrDefault(x => x.Name == Constants.Telemetry.OsLanguage);

        Assert.NotNull(actual?.Data);
        Assert.AreEqual(culture, actual.Data.ToString());
    }

    /// <summary>
    /// Verifies that the telemetry provider correctly reports the specified ASP.NET environment value.
    /// </summary>
    /// <param name="environment">The ASP.NET environment name to test (e.g., "Development", "Staging", or "Production").</param>
    [Test]
    [TestCase("Development")]
    [TestCase("Staging")]
    [TestCase("Production")]
    public void ReportsCorrectAspEnvironment(string environment)
    {
        var telemetryProvider = CreateProvider(environment: environment);

        var usageInformation = telemetryProvider.GetInformation().ToArray();
        var actual = usageInformation.FirstOrDefault(x => x.Name == Constants.Telemetry.AspEnvironment);

        Assert.NotNull(actual?.Data);
        Assert.AreEqual(environment, actual.Data);
    }

    private SystemTroubleshootingInformationTelemetryProvider CreateProvider(
        string modelsMode = ModelsModeConstants.InMemoryAuto,
        bool isDebug = true,
        string environment = "",
        RuntimeMode runtimeMode = RuntimeMode.BackofficeDevelopment)
    {
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.Setup(x => x.EnvironmentName).Returns(environment);

        var databaseMock = new Mock<IUmbracoDatabase>();
        databaseMock.Setup(x => x.DatabaseType.GetProviderName()).Returns("SQL");

        return new SystemTroubleshootingInformationTelemetryProvider(
            Mock.Of<IUmbracoVersion>(),
            Mock.Of<ILocalizationService>(),
            Mock.Of<IOptionsMonitor<ModelsBuilderSettings>>(x => x.CurrentValue == new ModelsBuilderSettings { ModelsMode = modelsMode }),
            Mock.Of<IOptionsMonitor<HostingSettings>>(x => x.CurrentValue == new HostingSettings { Debug = isDebug }),
            hostEnvironment.Object,
            Mock.Of<IUmbracoDatabaseFactory>(x => x.CreateDatabase() == Mock.Of<IUmbracoDatabase>(y => y.DatabaseType == DatabaseType.SQLite)),
            Mock.Of<IServerRoleAccessor>(),
            Mock.Of<IOptionsMonitor<RuntimeSettings>>(x => x.CurrentValue == new RuntimeSettings { Mode = runtimeMode }));
    }
}
