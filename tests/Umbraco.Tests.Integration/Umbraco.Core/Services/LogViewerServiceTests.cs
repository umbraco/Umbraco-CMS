using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class LogViewerServiceTests : UmbracoIntegrationTest
{
    private ILogViewerService LogViewerService => GetRequiredService<ILogViewerService>();

    private const string LogfileName = "UmbracoTraceLog.INTEGRATIONTEST.20230707.json";

    private string _newLogfilePath;

    private readonly DateTime _startDate = new(2023, 7, 7);
    private readonly DateTime _endDate = new(2023, 7, 8);

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

        string newLogfileDirPath = loggingConfiguration.LogDirectory;
        _newLogfilePath = Path.Combine(newLogfileDirPath, LogfileName);

        // Create/ensure Directory exists
        ioHelper.EnsurePathExists(newLogfileDirPath);

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
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Status, Is.EqualTo(LogViewerOperationStatus.Success));
        });
    }

    [Test]
    public async Task Can_Get_Logs()
    {
        var attempt = await LogViewerService.GetPagedLogsAsync(_startDate, _endDate, 0, int.MaxValue);

        Assert.Multiple(() =>
        {
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Status, Is.EqualTo(LogViewerOperationStatus.Success));
            Assert.That(attempt.Result, Is.Not.Null);
            Assert.That(attempt.Result.Items, Is.Not.Empty);
            Assert.That(attempt.Result.Total, Is.EqualTo(362));
        });
    }

    [Test]
    public async Task Can_Get_Logs_By_Filter_Expression()
    {
        var attempt = await LogViewerService.GetPagedLogsAsync(
            _startDate,
            _endDate,
            0,
            int.MaxValue,
            filterExpression: "@Level='Error'");

        Assert.Multiple(() =>
        {
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Status, Is.EqualTo(LogViewerOperationStatus.Success));
            Assert.That(attempt.Result, Is.Not.Null);
            Assert.That(attempt.Result.Items, Is.Not.Empty);
            Assert.That(attempt.Result.Total, Is.EqualTo(6));
        });
    }

    [Test]
    public async Task Can_Get_Logs_By_Log_Levels()
    {
        var attempt =
            await LogViewerService.GetPagedLogsAsync(_startDate, _endDate, 0, int.MaxValue, logLevels: new[] {"Error"});

        Assert.Multiple(() =>
        {
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Status, Is.EqualTo(LogViewerOperationStatus.Success));
            Assert.That(attempt.Result, Is.Not.Null);
            Assert.That(attempt.Result.Items, Is.Not.Empty);
            Assert.That(attempt.Result.Total, Is.EqualTo(6));
        });
    }

    [Test]
    public async Task Can_Get_Log_Count()
    {
        var attempt = await LogViewerService.GetLogLevelCountsAsync(_startDate, _endDate);

        Assert.Multiple(() =>
        {
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Status, Is.EqualTo(LogViewerOperationStatus.Success));
            Assert.That(attempt.Result, Is.Not.Null);
            Assert.That(attempt.Result.Information, Is.EqualTo(341));
            Assert.That(attempt.Result.Debug, Is.EqualTo(0));
            Assert.That(attempt.Result.Warning, Is.EqualTo(9));
            Assert.That(attempt.Result.Error, Is.EqualTo(6));
            Assert.That(attempt.Result.Fatal, Is.EqualTo(6));
        });
    }

    [Test]
    public async Task Can_Get_Message_Templates()
    {
        var attempt = await LogViewerService.GetMessageTemplatesAsync(_startDate, _endDate, 0, int.MaxValue);

        Assert.Multiple(() =>
        {
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Status, Is.EqualTo(LogViewerOperationStatus.Success));
            Assert.That(attempt.Result, Is.Not.Null);
            Assert.That(attempt.Result.Items, Is.Not.Empty);
            Assert.That(attempt.Result.Total, Is.EqualTo(31));

            // Assert its sorted correctly
            var mostPopularTemplate = attempt.Result.Items.First();
            Assert.That(mostPopularTemplate.MessageTemplate, Is.EqualTo("Create Index:\n {Sql}"));
            Assert.That(mostPopularTemplate.Count, Is.EqualTo(74));
            Assert.That(attempt.Result.Items.OrderByDescending(x => x.Count), Is.EqualTo(attempt.Result.Items));
        });
    }

    [Test]
    public async Task Can_Add_Saved_Query()
    {
        const string queryName = "testQuery";
        const string query = "@Message like '%test critical%'";
        var attempt = await LogViewerService.AddSavedLogQueryAsync(queryName, query);

        Assert.Multiple(() =>
        {
            Assert.That(attempt.Success, Is.True);
            Assert.That(attempt.Status, Is.EqualTo(LogViewerOperationStatus.Success));
            Assert.That(attempt.Result, Is.Not.Null);
            Assert.That(attempt.Result.Name, Is.EqualTo(queryName));
            Assert.That(attempt.Result.Query, Is.EqualTo(query));
        });
    }

    [Test]
    public async Task Can_Get_Saved_Query()
    {
        const string queryName = "testQuery";
        const string query = "@Message like '%test critical%'";
        var savedAttempt = await LogViewerService.AddSavedLogQueryAsync(queryName, query);

        Assert.Multiple(() =>
        {
            Assert.That(savedAttempt.Success, Is.True);
            Assert.That(savedAttempt.Status, Is.EqualTo(LogViewerOperationStatus.Success));
            Assert.That(savedAttempt.Result, Is.Not.Null);
        });

        var getAttempt = await LogViewerService.GetSavedLogQueryByNameAsync(queryName);

        Assert.That(savedAttempt.Result, Is.EqualTo(getAttempt));
    }

    [Test]
    public async Task Can_Delete_Saved_Query()
    {
        const string queryName = "testQuery";
        const string query = "@Message like '%test critical%'";
        var savedAttempt = await LogViewerService.AddSavedLogQueryAsync(queryName, query);

        Assert.Multiple(() =>
        {
            Assert.That(savedAttempt.Success, Is.True);
            Assert.That(savedAttempt.Status, Is.EqualTo(LogViewerOperationStatus.Success));
            Assert.That(savedAttempt.Result, Is.Not.Null);
        });

        var deleteAttempt = await LogViewerService.DeleteSavedLogQueryAsync(queryName);

        Assert.Multiple(() =>
        {
            Assert.That(deleteAttempt.Success, Is.True);
            Assert.That(deleteAttempt.Status, Is.EqualTo(LogViewerOperationStatus.Success));
            Assert.That(deleteAttempt.Result, Is.EqualTo(savedAttempt.Result));
        });
    }
}
