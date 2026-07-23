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

    // A pre-recorded log file copied from TestData/TestLogs/ into the configured log directory in
    // OneTimeSetUp. Contains 362 valid Compact JSON entries from a real Umbraco test run and is the
    // basis for the "Can_*" assertions below.
    private const string SampleLogfileName = "UmbracoTraceLog.INTEGRATIONTEST.20230707.json";
    private readonly DateTime _sampleStartDate = new(2023, 7, 7);
    private readonly DateTime _sampleEndDate = new(2023, 7, 8);

    // A second log file written in OneTimeSetUp that deliberately contains an unterminated JSON
    // entry between two valid entries, to exercise corrupt-line recovery in LogViewerRepository.
    // The date is deliberately outside the sample file's range so the exact-count assertions on
    // the sample file are not influenced by these entries.
    private const string CorruptLogfileName = "UmbracoTraceLog.WITHCORRUPTLINE.20240109.json";
    private readonly DateTime _corruptStartDate = new(2024, 1, 9);
    private readonly DateTime _corruptEndDate = new(2024, 1, 10);

    private const string CorruptFileValidLineBefore =
        """{"@t":"2024-01-09T09:00:00.0000000Z","@mt":"First valid entry","SourceContext":"Test","ProcessId":1,"ProcessName":"Test","ThreadId":1,"MachineName":"TEST","Log4NetLevel":"INFO "}""";

    // Truncated mid-string — mirrors the failure mode from https://github.com/umbraco/Umbraco-CMS/issues/22820
    // (JsonReaderException: Unterminated string).
    private const string CorruptFileTruncatedLine =
        """{"@t":"2024-01-09T09:00:01.0000000Z","@mt":"Truncated entry""";

    private const string CorruptFileValidLineAfter =
        """{"@t":"2024-01-09T09:00:02.0000000Z","@mt":"Second valid entry","SourceContext":"Test","ProcessId":1,"ProcessName":"Test","ThreadId":1,"MachineName":"TEST","Log4NetLevel":"INFO "}""";

    // Three consecutive day files written in OneTimeSetUp, each holding an early and a late entry.
    // Timestamps and filenames are derived from *local* time so file selection (by local calendar
    // date) and the per-entry range check (by absolute instant) line up on any machine time zone.
    // A range starting midday on the first day and ending midday on the last day therefore includes
    // the whole middle day but only the in-range entries from the boundary day files (#14710).
    private readonly DateTime _boundaryStartDate = new(2026, 7, 21, 12, 0, 0);
    private readonly DateTime _boundaryEndDate = new(2026, 7, 23, 12, 0, 0);
    private readonly List<string> _boundaryLogfilePaths = new();

    private string _sampleLogfilePath;
    private string _corruptLogfilePath;

    [OneTimeSetUp]
    public new void Setup()
    {
        var testRoot = TestContext.CurrentContext.TestDirectory.Split("bin")[0];
        var ioHelper = TestHelper.IOHelper;
        var hostingEnv = TestHelper.GetHostingEnvironment();
        var loggingConfiguration = TestHelper.GetLoggingConfiguration(hostingEnv);

        var newLogfileDirPath = loggingConfiguration.LogDirectory;
        ioHelper.EnsurePathExists(newLogfileDirPath);

        // Sample log file (good content, 362 entries).
        var sampleLogfileSource = Path.Combine(testRoot, "TestData", "TestLogs", SampleLogfileName);
        _sampleLogfilePath = Path.Combine(newLogfileDirPath, SampleLogfileName);
        File.Copy(sampleLogfileSource, _sampleLogfilePath, true);

        // Corrupt log file (two valid entries around a truncated one).
        _corruptLogfilePath = Path.Combine(newLogfileDirPath, CorruptLogfileName);
        var corruptContent = string.Join(
            Environment.NewLine,
            new[] { CorruptFileValidLineBefore, CorruptFileTruncatedLine, CorruptFileValidLineAfter }) + Environment.NewLine;
        File.WriteAllText(_corruptLogfilePath, corruptContent);

        // Boundary-trimming fixture: one file per local day, each with an entry before and after
        // midday. With a range of [day1 12:00, day3 12:00] the "early" entry on day1 and the "late"
        // entry on day3 fall outside the range and must be excluded, while the middle day is whole.
        WriteBoundaryLogfile(newLogfileDirPath, _boundaryStartDate.Date, "Day21 early", "Day21 late");
        WriteBoundaryLogfile(newLogfileDirPath, _boundaryStartDate.Date.AddDays(1), "Day22 early", "Day22 late");
        WriteBoundaryLogfile(newLogfileDirPath, _boundaryEndDate.Date, "Day23 early", "Day23 late");
    }

    private void WriteBoundaryLogfile(string logfileDirPath, DateTime localDay, string earlyMessage, string lateMessage)
    {
        var path = Path.Combine(logfileDirPath, $"UmbracoTraceLog.BOUNDARYTEST.{localDay:yyyyMMdd}.json");
        var content = string.Join(
            Environment.NewLine,
            new[]
            {
                BoundaryLogLine(localDay.AddHours(6), earlyMessage),
                BoundaryLogLine(localDay.AddHours(18), lateMessage),
            }) + Environment.NewLine;
        File.WriteAllText(path, content);
        _boundaryLogfilePaths.Add(path);
    }

    private static string BoundaryLogLine(DateTime localTime, string message)
    {
        // Serilog Compact records the timestamp as an absolute instant; write the local wall time as
        // its UTC equivalent so the entry's instant matches what the range comparison expects.
        DateTime utc = DateTime.SpecifyKind(localTime, DateTimeKind.Local).ToUniversalTime();
        return $$"""{"@t":"{{utc:o}}","@mt":"{{message}}","SourceContext":"Test","ProcessId":1,"ProcessName":"Test","ThreadId":1,"MachineName":"TEST","Log4NetLevel":"INFO "}""";
    }

    [OneTimeTearDown]
    public new void TearDown()
    {
        if (File.Exists(_sampleLogfilePath))
        {
            File.Delete(_sampleLogfilePath);
        }

        if (File.Exists(_corruptLogfilePath))
        {
            File.Delete(_corruptLogfilePath);
        }

        foreach (var path in _boundaryLogfilePaths.Where(File.Exists))
        {
            File.Delete(path);
        }
    }

    [Test]
    public async Task Can_View_Logs()
    {
        var attempt = await LogViewerService.CanViewLogsAsync(_sampleStartDate, _sampleEndDate);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(attempt.Status, LogViewerOperationStatus.Success);
        });
    }

    [Test]
    public async Task Can_Get_Logs()
    {
        var attempt = await LogViewerService.GetPagedLogsAsync(_sampleStartDate, _sampleEndDate, 0, int.MaxValue);

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
        var attempt = await LogViewerService.GetPagedLogsAsync(
            _sampleStartDate,
            _sampleEndDate,
            0,
            int.MaxValue,
            filterExpression: "@Level='Error'");

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(LogViewerOperationStatus.Success, attempt.Status);
            Assert.IsNotNull(attempt.Result);
            Assert.IsNotEmpty(attempt.Result.Items);
            Assert.AreEqual(6, attempt.Result.Total);
        });
    }

    [Test]
    public async Task Can_Get_Logs_By_Log_Levels()
    {
        var attempt =
            await LogViewerService.GetPagedLogsAsync(_sampleStartDate, _sampleEndDate, 0, int.MaxValue, logLevels: new[] {"Error"});

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(LogViewerOperationStatus.Success, attempt.Status);
            Assert.IsNotNull(attempt.Result);
            Assert.IsNotEmpty(attempt.Result.Items);
            Assert.AreEqual(6, attempt.Result.Total);
        });
    }

    [Test]
    public async Task Can_Get_Log_Count()
    {
        var attempt = await LogViewerService.GetLogLevelCountsAsync(_sampleStartDate, _sampleEndDate);

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
        var attempt = await LogViewerService.GetMessageTemplatesAsync(_sampleStartDate, _sampleEndDate, 0, int.MaxValue);

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

    /// <summary>
    /// Verifies that a range spanning several day files returns every entry from the fully-covered
    /// day but only the in-range entries from the boundary day files, rather than the whole of those
    /// files. Coverage for the server side of https://github.com/umbraco/Umbraco-CMS/issues/14710.
    /// </summary>
    [Test]
    public async Task Get_Logs_Excludes_Boundary_File_Entries_Outside_Time_Range()
    {
        var attempt = await LogViewerService.GetPagedLogsAsync(_boundaryStartDate, _boundaryEndDate, 0, int.MaxValue);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(LogViewerOperationStatus.Success, attempt.Status);
            Assert.IsNotNull(attempt.Result);
            Assert.AreEqual(4, attempt.Result.Total, "Entries outside the range in the boundary day files should be excluded.");
            Assert.That(
                attempt.Result.Items.Select(x => x.RenderedMessage),
                Is.EquivalentTo(new[] { "Day21 late", "Day22 early", "Day22 late", "Day23 early" }));
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
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(attempt.Status, LogViewerOperationStatus.Success);
            Assert.IsNotNull(attempt.Result);
            Assert.AreEqual(queryName, attempt.Result.Name);
            Assert.AreEqual(query, attempt.Result.Query);
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
            Assert.IsTrue(savedAttempt.Success);
            Assert.AreEqual(savedAttempt.Status, LogViewerOperationStatus.Success);
            Assert.IsNotNull(savedAttempt.Result);
        });

        var getAttempt = await LogViewerService.GetSavedLogQueryByNameAsync(queryName);

        Assert.AreEqual(getAttempt, savedAttempt.Result);
    }

    [Test]
    public async Task Can_Delete_Saved_Query()
    {
        const string queryName = "testQuery";
        const string query = "@Message like '%test critical%'";
        var savedAttempt = await LogViewerService.AddSavedLogQueryAsync(queryName, query);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(savedAttempt.Success);
            Assert.AreEqual(savedAttempt.Status, LogViewerOperationStatus.Success);
            Assert.IsNotNull(savedAttempt.Result);
        });

        var deleteAttempt = await LogViewerService.DeleteSavedLogQueryAsync(queryName);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(deleteAttempt.Success);
            Assert.AreEqual(deleteAttempt.Status, LogViewerOperationStatus.Success);
            Assert.AreEqual(savedAttempt.Result, deleteAttempt.Result);
        });
    }

    /// <summary>
    /// Verifies that a log file containing an unterminated JSON entry between two valid entries
    /// does not prevent the log viewer from returning the surrounding valid entries.
    /// Regression coverage for https://github.com/umbraco/Umbraco-CMS/issues/22820.
    /// </summary>
    [Test]
    public async Task Reads_Valid_Lines_Either_Side_Of_Corrupt_Line()
    {
        var attempt = await LogViewerService.GetPagedLogsAsync(_corruptStartDate, _corruptEndDate, 0, int.MaxValue);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(LogViewerOperationStatus.Success, attempt.Status);
            Assert.IsNotNull(attempt.Result);
            Assert.AreEqual(2, attempt.Result.Total, "Expected the two valid lines either side of the corrupt line to be returned.");
            Assert.That(attempt.Result.Items.Select(x => x.RenderedMessage), Is.EquivalentTo(new[] { "First valid entry", "Second valid entry" }));
        });
    }

    /// <summary>
    /// Verifies that the level-counts request returns successfully and tallies the surrounding valid
    /// entries in the presence of an unterminated JSON entry in the source file.
    /// </summary>
    [Test]
    public async Task Get_Log_Count_Succeeds_With_Corrupt_Line()
    {
        var attempt = await LogViewerService.GetLogLevelCountsAsync(_corruptStartDate, _corruptEndDate);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(attempt.Success);
            Assert.AreEqual(LogViewerOperationStatus.Success, attempt.Status);
            Assert.IsNotNull(attempt.Result);
            Assert.AreEqual(2, attempt.Result.Information);
        });
    }
}
