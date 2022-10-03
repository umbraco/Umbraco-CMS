using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Telemetry;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class TelemetryServiceTests : UmbracoIntegrationTest
{
    protected override void CustomTestSetup(IUmbracoBuilder builder) =>
        builder.Services.Configure<GlobalSettings>(options => options.Id = Guid.NewGuid().ToString());

    private ITelemetryService TelemetryService => GetRequiredService<ITelemetryService>();
    private IMetricsConsentService MetricsConsentService => GetRequiredService<IMetricsConsentService>();

    [Test]
    public void Expected_Detailed_Telemetry_Exists()
    {
        var expectedData = new[]
        {
            Constants.Telemetry.RootCount,
            Constants.Telemetry.DomainCount,
            Constants.Telemetry.ExamineIndexCount,
            Constants.Telemetry.LanguageCount,
            Constants.Telemetry.MacroCount,
            Constants.Telemetry.MediaCount,
            Constants.Telemetry.MediaCount,
            Constants.Telemetry.TemplateCount,
            Constants.Telemetry.ContentCount,
            Constants.Telemetry.DocumentTypeCount,
            Constants.Telemetry.Properties,
            Constants.Telemetry.UserCount,
            Constants.Telemetry.UserGroupCount,
            Constants.Telemetry.ServerOs,
            Constants.Telemetry.ServerFramework,
            Constants.Telemetry.OsLanguage,
            Constants.Telemetry.WebServer,
            Constants.Telemetry.ModelsBuilderMode,
            Constants.Telemetry.AspEnvironment,
            Constants.Telemetry.IsDebug,
            Constants.Telemetry.DatabaseProvider,
            Constants.Telemetry.CurrentServerRole
        };

        MetricsConsentService.SetConsentLevel(TelemetryLevel.Detailed);
        var success = TelemetryService.TryGetTelemetryReportData(out var telemetryReportData);
        var detailed = telemetryReportData.Detailed.ToArray();

        Assert.IsTrue(success);
        Assert.Multiple(() =>
        {
            Assert.IsNotNull(detailed);
            Assert.AreEqual(expectedData.Length, detailed.Length);

            foreach (var expectedInfo in expectedData)
            {
                var expected = detailed.FirstOrDefault(x => x.Name == expectedInfo);
                Assert.IsNotNull(expected, $"Expected {expectedInfo} to exists in the detailed list");
            }
        });
    }
}
