using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Telemetry.Interfaces;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Telemetry;

[TestFixture]
public class UsageInformationServiceTests
{
    [Test]
    public void GetDetailed_Skips_Provider_That_Throws()
    {
        var failing = new Mock<IDetailedTelemetryProvider>();
        failing.Setup(x => x.GetInformation()).Throws(new InvalidOperationException("Boom"));

        var service = CreateService(failing.Object, CreateProvider("Working", 1));

        UsageInformation[] result = service.GetDetailed()!.ToArray();

        Assert.AreEqual(1, result.Length);
        Assert.AreEqual("Working", result[0].Name);
    }

    [Test]
    public void GetDetailed_Skips_Provider_That_Throws_While_Enumerating()
    {
        var failing = new Mock<IDetailedTelemetryProvider>();
        failing.Setup(x => x.GetInformation()).Returns(ThrowWhileEnumerating());

        var service = CreateService(failing.Object, CreateProvider("Working", 1));

        UsageInformation[] result = service.GetDetailed()!.ToArray();

        Assert.AreEqual(1, result.Length);
        Assert.AreEqual("Working", result[0].Name);
    }

    [Test]
    public void GetDetailed_Returns_Null_When_Consent_Is_Not_Detailed()
    {
        var service = CreateService(TelemetryLevel.Basic, CreateProvider("Working", 1));

        Assert.IsNull(service.GetDetailed());
    }

    private static IEnumerable<UsageInformation> ThrowWhileEnumerating()
    {
        yield return new UsageInformation("Partial", 1);
        throw new InvalidOperationException("Boom");
    }

    private static IDetailedTelemetryProvider CreateProvider(string name, object data)
    {
        var provider = new Mock<IDetailedTelemetryProvider>();
        provider.Setup(x => x.GetInformation()).Returns(new[] { new UsageInformation(name, data) });
        return provider.Object;
    }

    private static UsageInformationService CreateService(params IDetailedTelemetryProvider[] providers)
        => CreateService(TelemetryLevel.Detailed, providers);

    private static UsageInformationService CreateService(TelemetryLevel level, params IDetailedTelemetryProvider[] providers)
    {
        var consent = new Mock<IMetricsConsentService>();
        consent.Setup(x => x.GetConsentLevel()).Returns(level);
        return new UsageInformationService(consent.Object, providers, NullLogger<UsageInformationService>.Instance);
    }
}
