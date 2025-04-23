using NUnit.Framework;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Logging;

[TestFixture]
public class LoggingConfigurationTests
{
    [SetUp]
    public void SetUp() => Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

    [Test]
    public void Can_Get_Supported_Log_File_Name_Format_Arguments()
    {
        var config = new LoggingConfiguration("c:\\logs\\", "UmbracoLogFile_{0}_{1}..json", "MachineName,EnvironmentName");
        var result = config.GetLogFileNameFormatArguments();

        Assert.AreEqual(2, result.Length);

        var expectedMachineName = Environment.MachineName;
        var expectedEnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Assert.AreEqual(expectedMachineName, result[0]);
        Assert.AreEqual(expectedEnvironmentName, result[1]);
    }
}
