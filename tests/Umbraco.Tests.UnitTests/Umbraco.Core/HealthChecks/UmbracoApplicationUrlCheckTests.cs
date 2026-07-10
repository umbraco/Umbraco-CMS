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
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.HealthChecks;

[TestFixture]
public class UmbracoApplicationUrlCheckTests
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

    private static UmbracoApplicationUrlCheck CreateCheck(string? url, ApplicationUrlDetection detection)
    {
        var settings = new WebRoutingSettings
        {
            UmbracoApplicationUrl = url!,
            ApplicationUrlDetection = detection,
        };

        IOptionsMonitor<WebRoutingSettings> monitor = new TestOptionsMonitor<WebRoutingSettings>(settings);
        return new UmbracoApplicationUrlCheck(MockTextService(), monitor);
    }

    [Test]
    public async Task When_Url_Is_Configured_Returns_Success()
    {
        UmbracoApplicationUrlCheck check = CreateCheck("https://mysite.com/", ApplicationUrlDetection.None);

        HealthCheckStatus status = (await check.GetStatusAsync()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Success));
            Assert.That(status.Message, Is.EqualTo("umbracoApplicationUrlCheckResultTrue"));
            Assert.That(status.ReadMoreLink, Is.Null.Or.Empty);
        });
    }

    [TestCase(ApplicationUrlDetection.FirstRequest)]
    [TestCase(ApplicationUrlDetection.EveryRequest)]
    public async Task When_Url_Is_Not_Configured_But_Detection_Is_Enabled_Returns_Warning(ApplicationUrlDetection detection)
    {
        UmbracoApplicationUrlCheck check = CreateCheck(null, detection);

        HealthCheckStatus status = (await check.GetStatusAsync()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Warning));
            Assert.That(status.Message, Is.EqualTo("umbracoApplicationUrlCheckResultFalse"));
            Assert.That(status.ReadMoreLink, Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public async Task When_Url_Is_Not_Configured_And_Detection_Is_None_Returns_Error()
    {
        UmbracoApplicationUrlCheck check = CreateCheck(null, ApplicationUrlDetection.None);

        HealthCheckStatus status = (await check.GetStatusAsync()).Single();

        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Error));
            Assert.That(status.Message, Is.EqualTo("umbracoApplicationUrlCheckResultError"));
            Assert.That(status.ReadMoreLink, Is.Not.Null.And.Not.Empty);
        });
    }
}
