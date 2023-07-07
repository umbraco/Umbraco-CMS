using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class LogViewerServiceTests : UmbracoIntegrationTest
{
    private ILogViewerService LogViewerService => GetRequiredService<ILogViewerService>();

    private const string LogfileName = "UmbracoTraceLog.INTEGRATIONTEST.20230707.json";

    private string _newLogfilePath;
    private string _newLogfileDirPath;
    private DateTime _startDate = new(2023, 7, 7);
    private DateTime _endDate = new(2023, 7, 8);

    [OneTimeSetUp]
    public void Setup()
    {
        // Create an example JSON log file to check results
        // As a one time setup for all tets in this class/fixture
        var testRoot = TestContext.CurrentContext.TestDirectory.Split("bin")[0];
        var ioHelper = TestHelper.IOHelper;
        var hostingEnv = TestHelper.GetHostingEnvironment();

        var loggingConfiguration = TestHelper.GetLoggingConfiguration(hostingEnv);

        var exampleLogfilePath = Path.Combine(testRoot, "TestData", "TestLogs", LogfileName);
        _newLogfileDirPath = loggingConfiguration.LogDirectory;
        _newLogfilePath = Path.Combine(_newLogfileDirPath, LogfileName);

        // Create/ensure Directory exists
        ioHelper.EnsurePathExists(_newLogfileDirPath);

        // Copy the sample files
        File.Copy(exampleLogfilePath, _newLogfilePath, true);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        // Cleanup & delete the example log & search files off disk
        // Once all tests in this class/fixture have run
        if (File.Exists(_newLogfilePath))
        {
            File.Delete(_newLogfilePath);
        }
    }

    [Test]
    public async Task Can_View_Logs()
    {
        var attempt = await LogViewerService.CanViewLogsAsync(_startDate, _endDate);

        Assert.AreEqual(attempt.Status, LogViewerOperationStatus.Success);
    }

    [Test]
    public async Task Can_Get_Logs()
    {
        var attempt = await LogViewerService.GetPagedLogsAsync(_startDate, _endDate, 0, int.MaxValue);

        Assert.AreEqual(attempt.Status, LogViewerOperationStatus.Success);
        Assert.IsNotNull(attempt.Result);
        Assert.IsNotEmpty(attempt.Result.Items);
        Assert.AreEqual(362, attempt.Result.Total);
    }
}
