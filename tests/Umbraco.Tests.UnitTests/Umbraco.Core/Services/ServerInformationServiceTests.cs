using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class ServerInformationServiceTests
{
    private static readonly TimeZoneInfo TestTimeZone =
        TimeZoneInfo.CreateCustomTimeZone("Umbraco/Test", TimeSpan.FromHours(2), "Umbraco Test", "Umbraco Test");

    [TestCase(true, ExpectedResult = true)]
    [TestCase(false, ExpectedResult = false)]
    public bool GetServerInformation_Returns_IsDebugMode_From_HostingEnvironment(bool isDebugMode)
        => CreateSut(isDebugMode: isDebugMode).GetServerInformation().IsDebugMode;

    [TestCase(RuntimeMode.BackofficeDevelopment)]
    [TestCase(RuntimeMode.Development)]
    [TestCase(RuntimeMode.Production)]
    public void GetServerInformation_Returns_RuntimeMode_From_RuntimeSettings(RuntimeMode runtimeMode)
    {
        ServerInformation result = CreateSut(runtimeMode: runtimeMode).GetServerInformation();

        Assert.AreEqual(runtimeMode, result.RuntimeMode);
    }

    [Test]
    public void GetServerInformation_Returns_SemVersion_From_UmbracoVersion()
    {
        var semVersion = new SemVersion(17, 3, 1, "rc", "abc123");

        ServerInformation result = CreateSut(semVersion: semVersion).GetServerInformation();

        Assert.AreEqual(semVersion, result.SemVersion);
    }

    [Test]
    public void GetServerInformation_Returns_TimeZoneInfo_From_TimeProvider()
    {
        ServerInformation result = CreateSut().GetServerInformation();

        Assert.AreEqual(TestTimeZone, result.TimeZoneInfo);
    }

    private static ServerInformationService CreateSut(
        SemVersion? semVersion = null,
        RuntimeMode runtimeMode = RuntimeMode.BackofficeDevelopment,
        bool isDebugMode = false)
    {
        var timeProvider = new FakeTimeProvider();
        timeProvider.SetLocalTimeZone(TestTimeZone);

        return new ServerInformationService(
            Mock.Of<IUmbracoVersion>(x => x.SemanticVersion == (semVersion ?? new SemVersion(17, 0, 0))),
            timeProvider,
            Mock.Of<IOptionsMonitor<RuntimeSettings>>(x => x.CurrentValue == new RuntimeSettings { Mode = runtimeMode }),
            Mock.Of<IHostingEnvironment>(x => x.IsDebugMode == isDebugMode));
    }
}
