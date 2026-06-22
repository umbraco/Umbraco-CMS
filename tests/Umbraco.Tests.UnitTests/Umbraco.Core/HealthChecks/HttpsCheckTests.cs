// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.HealthChecks.Checks.Security;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.HealthChecks;

[TestFixture]
public class HttpsCheckTests
{
    private const string HttpsUrl = "https://example.com/";
    private const string HttpUrl = "http://example.com/";

    [Test]
    public async Task GetStatusAsync_WhenApplicationMainUrlIsNull_AllChecksReturnInfo()
    {
        HttpsCheck check = CreateCheck(applicationMainUrl: null);

        HealthCheckStatus[] statuses = (await check.GetStatusAsync()).ToArray();

        Assert.That(statuses, Has.Length.EqualTo(3));
        Assert.Multiple(() =>
        {
            foreach (HealthCheckStatus status in statuses)
            {
                Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Info));
                Assert.That(status.Message, Is.EqualTo("httpsCheckNoApplicationUrl"));
            }
        });
    }

    [Test]
    public async Task CheckIfCurrentSchemeIsHttps_WhenUrlIsHttps_ReturnsSuccess()
    {
        HttpsCheck check = CreateCheck(applicationMainUrl: new Uri(HttpsUrl));

        HealthCheckStatus status = await check.CheckIfCurrentSchemeIsHttps();

        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Success));
            Assert.That(status.ReadMoreLink, Is.Null.Or.Empty);
        });
    }

    [Test]
    public async Task CheckIfCurrentSchemeIsHttps_WhenUrlIsHttp_ReturnsError()
    {
        HttpsCheck check = CreateCheck(applicationMainUrl: new Uri(HttpUrl));

        HealthCheckStatus status = await check.CheckIfCurrentSchemeIsHttps();

        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Error));
            Assert.That(status.ReadMoreLink, Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public async Task CheckHttpsConfigurationSetting_WhenUrlIsHttp_ReturnsInfo()
    {
        HttpsCheck check = CreateCheck(applicationMainUrl: new Uri(HttpUrl), useHttps: false);

        HealthCheckStatus status = await check.CheckHttpsConfigurationSetting();

        Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Info));
    }

    [Test]
    public async Task CheckHttpsConfigurationSetting_WhenUrlIsHttpsAndUseHttpsEnabled_ReturnsSuccess()
    {
        HttpsCheck check = CreateCheck(applicationMainUrl: new Uri(HttpsUrl), useHttps: true);

        HealthCheckStatus status = await check.CheckHttpsConfigurationSetting();

        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Success));
            Assert.That(status.ReadMoreLink, Is.Null.Or.Empty);
        });
    }

    [Test]
    public async Task CheckHttpsConfigurationSetting_WhenUrlIsHttpsAndUseHttpsDisabled_ReturnsError()
    {
        HttpsCheck check = CreateCheck(applicationMainUrl: new Uri(HttpsUrl), useHttps: false);

        HealthCheckStatus status = await check.CheckHttpsConfigurationSetting();

        Assert.Multiple(() =>
        {
            Assert.That(status.ResultType, Is.EqualTo(StatusResultType.Error));
            Assert.That(status.ReadMoreLink, Is.Not.Null.And.Not.Empty);
        });
    }

    // ILocalizedTextService.Localize is called via extensions that delegate to the 4-arg interface method.
    // We return the alias so tests can assert on which key was used.
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

    private static HttpsCheck CreateCheck(Uri? applicationMainUrl, bool useHttps = false)
    {
        IHostingEnvironment hostingEnvironment = Mock.Of<IHostingEnvironment>(x => x.ApplicationMainUrl == applicationMainUrl);
        IOptionsMonitor<GlobalSettings> globalSettings =
            Mock.Of<IOptionsMonitor<GlobalSettings>>(m => m.CurrentValue == new GlobalSettings { UseHttps = useHttps });
        return new HttpsCheck(MockTextService(), globalSettings, hostingEnvironment);
    }
}
