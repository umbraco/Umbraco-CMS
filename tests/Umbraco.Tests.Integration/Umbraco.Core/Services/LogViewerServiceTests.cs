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

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(attempt.Status, LogViewerOperationStatus.Success);
        });
    }

    [Test]
    public async Task Can_Get_Logs()
    {
        var attempt = await LogViewerService.GetPagedLogsAsync(_startDate, _endDate, 0, int.MaxValue);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(attempt.Status, LogViewerOperationStatus.Success);
            Assert.IsNotNull(attempt.Result);
            Assert.IsNotEmpty(attempt.Result.Items);
            Assert.AreEqual(362, attempt.Result.Total);
        });
    }

    [Test]
    public async Task Can_Get_Logs_By_Filter_Expression()
    {
        var attempt = await LogViewerService.GetPagedLogsAsync(_startDate, _endDate, 0, int.MaxValue, filterExpression: "@Level='Error'");

        Assert.IsTrue(attempt.Success);
        Assert.AreEqual(LogViewerOperationStatus.Success, attempt.Status);
        Assert.IsNotNull(attempt.Result);
        Assert.IsNotEmpty(attempt.Result.Items);
        Assert.AreEqual(6, attempt.Result.Total);
    }

    [Test]
    public async Task Can_Get_Logs_By_Log_Levels()
    {
        var attempt = await LogViewerService.GetPagedLogsAsync(_startDate, _endDate, 0, int.MaxValue, logLevels: new[] { "Error" });

        Assert.IsTrue(attempt.Success);
        Assert.AreEqual(LogViewerOperationStatus.Success, attempt.Status);
        Assert.IsNotNull(attempt.Result);
        Assert.IsNotEmpty(attempt.Result.Items);
        Assert.AreEqual(6, attempt.Result.Total);
    }

    [Test]
    public async Task Can_Get_Log_Count()
    {
        var attempt = await LogViewerService.GetLogLevelCountsAsync(_startDate, _endDate);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(attempt.Status, LogViewerOperationStatus.Success);
            Assert.IsNotNull(attempt.Result);
            Assert.AreEqual(341, attempt.Result.Information);
            Assert.AreEqual(0, attempt.Result.Debug);
            Assert.AreEqual(9, attempt.Result.Warning);
            Assert.AreEqual(6, attempt.Result.Error);
            Assert.AreEqual(6, attempt.Result.Fatal);
        });
    }

    [Test]
    public async Task Can_Get_Message_Templates()
    {
        var attempt = await LogViewerService.GetMessageTemplatesAsync(_startDate, _endDate, 0, int.MaxValue);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(attempt.Status, LogViewerOperationStatus.Success);
            Assert.IsNotNull(attempt.Result);
            Assert.IsNotEmpty(attempt.Result.Items);
            Assert.AreEqual(31, attempt.Result.Total);

            // Assert its sorted correctly
            var mostPopularTemplate = attempt.Result.Items.First();
            Assert.AreEqual("Create Index:\n {Sql}", mostPopularTemplate.MessageTemplate);
            Assert.AreEqual(74, mostPopularTemplate.Count);
            Assert.AreEqual(attempt.Result.Items, attempt.Result.Items.OrderByDescending(x => x.Count));
        });
    }
}
