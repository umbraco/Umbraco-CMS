// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.HealthChecks;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.HealthChecks;

[TestFixture]
public class UmbracoReadinessHealthCheckTests
{
    [Test]
    public async Task CheckHealthAsync_WhenLevelIsRun_ReturnsHealthy()
    {
        var check = CreateCheck(RuntimeLevel.Run);
        HealthCheckResult result = await check.CheckHealthAsync(null!);
        Assert.That(result.Status, Is.EqualTo(HealthStatus.Healthy));
    }

    [TestCase(RuntimeLevel.Upgrading)]
    [TestCase(RuntimeLevel.Boot)]
    [TestCase(RuntimeLevel.Install)]
    [TestCase(RuntimeLevel.Upgrade)]
    [TestCase(RuntimeLevel.BootFailed)]
    public async Task CheckHealthAsync_WhenLevelIsNotRun_ReturnsDegraded(RuntimeLevel level)
    {
        var check = CreateCheck(level);
        HealthCheckResult result = await check.CheckHealthAsync(null!);
        Assert.That(result.Status, Is.EqualTo(HealthStatus.Degraded));
    }

    [Test]
    public async Task CheckHealthAsync_WhenLevelIsUpgrading_IncludesLevelInDescription()
    {
        var check = CreateCheck(RuntimeLevel.Upgrading);
        HealthCheckResult result = await check.CheckHealthAsync(null!);
        Assert.That(result.Description, Does.Contain(RuntimeLevel.Upgrading.ToString()));
    }

    private static UmbracoReadinessHealthCheck CreateCheck(RuntimeLevel level)
        => new(Mock.Of<IRuntimeState>(s => s.Level == level));
}
