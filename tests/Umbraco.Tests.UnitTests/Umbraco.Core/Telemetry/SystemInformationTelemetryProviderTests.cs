using System.Globalization;
using System.Linq;
using System.Threading;
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
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Telemetry.Providers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Telemetry;

[TestFixture]
public class SystemInformationTelemetryProviderTests
{
    [Test]
    [TestCase(ModelsMode.Nothing)]
    [TestCase(ModelsMode.InMemoryAuto)]
    [TestCase(ModelsMode.SourceCodeAuto)]
    [TestCase(ModelsMode.SourceCodeManual)]
    public void ReportsModelsModeCorrectly(ModelsMode modelsMode)
    {
        var telemetryProvider = CreateProvider(modelsMode);
        var usageInformation = telemetryProvider.GetInformation().ToArray();

        var actual = usageInformation.FirstOrDefault(x => x.Name == Constants.Telemetry.ModelsBuilderMode);
        Assert.IsNotNull(actual?.Data);
        Assert.AreEqual(modelsMode.ToString(), actual.Data);
    }

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

    private SystemInformationTelemetryProvider CreateProvider(
        ModelsMode modelsMode = ModelsMode.InMemoryAuto,
        bool isDebug = true,
        string environment = "")
    {
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.Setup(x => x.EnvironmentName).Returns(environment);

        var databaseMock = new Mock<IUmbracoDatabase>();
        databaseMock.Setup(x => x.DatabaseType.GetProviderName()).Returns("SQL");

        return new SystemInformationTelemetryProvider(
            Mock.Of<IUmbracoVersion>(),
            Mock.Of<ILocalizationService>(),
            Mock.Of<IOptionsMonitor<ModelsBuilderSettings>>(x => x.CurrentValue == new ModelsBuilderSettings{ ModelsMode = modelsMode }),
            Mock.Of<IOptionsMonitor<HostingSettings>>(x => x.CurrentValue == new HostingSettings { Debug = isDebug }),
            hostEnvironment.Object,
            Mock.Of<IUmbracoDatabaseFactory>(x => x.CreateDatabase() == Mock.Of<IUmbracoDatabase>(y => y.DatabaseType == DatabaseType.SQLite)),
            Mock.Of<IServerRoleAccessor>());
    }
}
