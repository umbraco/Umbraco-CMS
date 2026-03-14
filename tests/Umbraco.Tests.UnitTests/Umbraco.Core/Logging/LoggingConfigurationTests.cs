using NUnit.Framework;
using Umbraco.Cms.Core.Logging;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Logging;

    /// <summary>
    /// Contains unit tests for the <see cref="LoggingConfiguration"/> class to verify its behavior and configuration logic.
    /// </summary>
[TestFixture]
public class LoggingConfigurationTests
{
    /// <summary>
    /// Sets up the test environment by configuring the ASPNETCORE_ENVIRONMENT variable.
    /// </summary>
    [SetUp]
    public void SetUp() => Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

    /// <summary>
    /// Tests that the LoggingConfiguration correctly retrieves supported log file name format arguments.
    /// </summary>
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
