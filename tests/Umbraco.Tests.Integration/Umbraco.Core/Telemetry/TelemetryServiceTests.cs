using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry;
using Umbraco.Cms.Core.Webhooks;
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

    private WebhookEventCollection WebhookEventCollection => GetRequiredService<WebhookEventCollection>();

    [Test]
    public async Task Expected_Detailed_Telemetry_Exists()
    {
        var expectedData = new List<string>
        {
            Constants.Telemetry.RootCount,
            Constants.Telemetry.DomainCount,
            Constants.Telemetry.ExamineIndexCount,
            Constants.Telemetry.LanguageCount,
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
            Constants.Telemetry.CurrentServerRole,
            Constants.Telemetry.BackofficeExternalLoginProviderCount,
            Constants.Telemetry.RuntimeMode,
            Constants.Telemetry.DeliverApiEnabled,
            Constants.Telemetry.DeliveryApiPublicAccess,
            Constants.Telemetry.WebhookTotal,
            Constants.Telemetry.WebhookCustomHeaders,
            Constants.Telemetry.WebhookCustomEvent,
            Constants.Telemetry.RichTextEditorCount,
            Constants.Telemetry.RichTextBlockCount,
            Constants.Telemetry.TotalPropertyCount,
            Constants.Telemetry.HighestPropertyCount,
            Constants.Telemetry.TotalCompositions,
        };

        // Add the default webhook events.
        expectedData.AddRange(WebhookEventCollection.Select(eventInfo => $"{Constants.Telemetry.WebhookPrefix}{eventInfo.Alias}"));

        await MetricsConsentService.SetConsentLevelAsync(TelemetryLevel.Detailed);
        var telemetryReportData = await TelemetryService.GetTelemetryReportDataAsync();
        Assert.IsNotNull(telemetryReportData);

        var detailed = telemetryReportData!.Detailed.ToArray();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(detailed);
            Assert.AreEqual(expectedData.Count, detailed.Length);

            foreach (var expectedInfo in expectedData)
            {
                var expected = detailed.FirstOrDefault(x => x.Name == expectedInfo);
                Assert.IsNotNull(expected, $"Expected {expectedInfo} to exists in the detailed list");
            }
        });
    }
}
