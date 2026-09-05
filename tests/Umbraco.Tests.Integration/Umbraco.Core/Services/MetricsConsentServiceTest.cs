using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class MetricsConsentServiceTest : UmbracoIntegrationTest
{
    private IMetricsConsentService MetricsConsentService => GetRequiredService<IMetricsConsentService>();

    private IKeyValueService KeyValueService => GetRequiredService<IKeyValueService>();

    [Test]
    [TestCase(TelemetryLevel.Minimal)]
    [TestCase(TelemetryLevel.Basic)]
    [TestCase(TelemetryLevel.Detailed)]
    public async Task Can_Store_Consent(TelemetryLevel level)
    {
        await MetricsConsentService.SetConsentLevelAsync(level);

        var actual = MetricsConsentService.GetConsentLevel();
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.EqualTo(level));
    }

    [Test]
    public async Task Enum_Stored_as_string()
    {
        await MetricsConsentService.SetConsentLevelAsync(TelemetryLevel.Detailed);

        var stringValue = KeyValueService.GetValue(Cms.Core.Services.MetricsConsentService.Key);

        Assert.That(stringValue, Is.EqualTo("Detailed"));
    }
}
