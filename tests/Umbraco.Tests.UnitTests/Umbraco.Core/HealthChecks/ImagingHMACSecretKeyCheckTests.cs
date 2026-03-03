// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.HealthChecks.Checks.Security;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.HealthChecks;

[TestFixture]
public class ImagingHMACSecretKeyCheckTests
{
    // ILocalizedTextService.Localize is called via the extension Localize(area, alias) which delegates to
    // Localize(area, alias, CultureInfo, IDictionary). We return the alias so tests can assert on which key was used.
    private static ILocalizedTextService MockTextService()
    {
        var mock = new Mock<ILocalizedTextService>();
        mock.Setup(x => x.Localize(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CultureInfo?>(),
                It.IsAny<IDictionary<string, string?>>()))
            .Returns((string? _, string? alias, CultureInfo? _, IDictionary<string, string?> _) => alias ?? string.Empty);
        return mock.Object;
    }

    [Test]
    public async Task GetStatusAsync_WhenHMACKeyIsEmpty_ReturnsWarning()
    {
        var settings = new ImagingSettings { HMACSecretKey = [] };
        var optionsMonitor = Mock.Of<IOptionsMonitor<ImagingSettings>>(x => x.CurrentValue == settings);
        var check = new ImagingHMACSecretKeyCheck(MockTextService(), optionsMonitor);

        IEnumerable<HealthCheckStatus> statuses = await check.GetStatusAsync();

        HealthCheckStatus status = statuses.Single();
        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Warning));
            Assert.That(status.Message, Is.EqualTo("imagingHMACSecretKeyCheckWarningMessage"));
            Assert.That(status.ReadMoreLink, Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public async Task GetStatusAsync_WhenHMACKeyIsSet_ReturnsSuccess()
    {
        var settings = new ImagingSettings { HMACSecretKey = [1, 2, 3, 4, 5] };
        var optionsMonitor = Mock.Of<IOptionsMonitor<ImagingSettings>>(x => x.CurrentValue == settings);
        var check = new ImagingHMACSecretKeyCheck(MockTextService(), optionsMonitor);

        IEnumerable<HealthCheckStatus> statuses = await check.GetStatusAsync();

        HealthCheckStatus status = statuses.Single();
        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Success));
            Assert.That(status.Message, Is.EqualTo("imagingHMACSecretKeyCheckSuccessMessage"));
            Assert.That(status.ReadMoreLink, Is.Null.Or.Empty);
        });
    }

    [Test]
    public void ExecuteAction_AlwaysThrows()
    {
        var settings = new ImagingSettings();
        var optionsMonitor = Mock.Of<IOptionsMonitor<ImagingSettings>>(x => x.CurrentValue == settings);
        var check = new ImagingHMACSecretKeyCheck(MockTextService(), optionsMonitor);

        Assert.Throws<NotSupportedException>(() => check.ExecuteAction(new HealthCheckAction("fix", check.Id)));
    }
}
