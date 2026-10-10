using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.HealthChecks;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HealthChecks.Checks.Data;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.HealthChecks;

[TestFixture]
public class MigrationStateCheckTests
{
    private static readonly IUmbracoVersion _umbracoVersion =
        Mock.Of<IUmbracoVersion>(x => x.SemanticVersion == new SemVersion(18, 0, 0));

    [Test]
    public async Task Recorded_State_Matching_Final_State_Returns_Success()
    {
        var plan = new UmbracoPlan(_umbracoVersion);

        HealthCheckStatus result = await GetStatus(plan.FinalState);

        Assert.AreEqual(StatusResultType.Success, result.ResultType);
    }

    [Test]
    public async Task Recorded_State_Earlier_In_Plan_Returns_Error()
    {
        var plan = new UmbracoPlan(_umbracoVersion);

        HealthCheckStatus result = await GetStatus(plan.InitialState);

        Assert.AreEqual(StatusResultType.Error, result.ResultType);
        StringAssert.Contains("16.4.0", result.Message);
    }

    [TestCase(null)]
    [TestCase("")]
    public async Task Missing_Recorded_State_Returns_Error(string? state)
    {
        HealthCheckStatus result = await GetStatus(state);

        Assert.AreEqual(StatusResultType.Error, result.ResultType);
    }

    [Test]
    public async Task Recorded_State_Not_In_Plan_Returns_Warning()
    {
        HealthCheckStatus result = await GetStatus("{00000000-0000-0000-0000-000000000001}");

        Assert.AreEqual(StatusResultType.Warning, result.ResultType);
    }

    private static async Task<HealthCheckStatus> GetStatus(string? recordedState)
    {
        var keyValueService = new Mock<IKeyValueService>();
        keyValueService
            .Setup(x => x.GetValue(Constants.Conventions.Migrations.UmbracoUpgradePlanKey))
            .Returns(recordedState);

        var check = new MigrationStateCheck(keyValueService.Object, _umbracoVersion);

        return (await check.GetStatusAsync()).Single();
    }
}
