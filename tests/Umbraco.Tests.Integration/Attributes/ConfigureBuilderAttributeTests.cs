using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Attributes;

public class ConfigureBuilderAttributeTests : UmbracoIntegrationTest
{
    private const string TestTelemetryId = "IdSetbyTestAttribute";

    public static void Configure(IUmbracoBuilder builder)
    {
        builder.Services.Configure<GlobalSettings>(config =>
            config.Id = TestTelemetryId);
    }

    public static void ConfigureWithValue(IUmbracoBuilder builder, string telemetryId)
    {
        builder.Services.Configure<GlobalSettings>(config =>
            config.Id = telemetryId);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [ConfigureBuilder(ActionName = nameof(Configure))]
    public void MethodAttributeOverwritesSetupForAllCases(int testValue)
    {
        var settings = GetRequiredService<IOptions<GlobalSettings>>().Value;
        Assert.AreEqual(TestTelemetryId, settings.Id);
    }

    [TestCase(1, "IdOne")]
    [TestCase(2, "IdTwo")]
    [TestCase(3, "IdThree")]
    [ConfigureBuilderTestCase(ActionName = nameof(ConfigureWithValue), IndexOfParameter = 1)]
    public void CaseAttributeOverwritesSetupForSpecificCase(int testValue, string telemetryId)
    {
        var settings = GetRequiredService<IOptions<GlobalSettings>>().Value;
        Assert.AreEqual(telemetryId, settings.Id);
    }
}
