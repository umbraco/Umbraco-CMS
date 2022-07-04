// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Serilog;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Logging.Viewer;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using File = System.IO.File;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Logging;

[TestFixture]
public class LogviewerTests
{
    [OneTimeSetUp]
    public void Setup()
    {
        var testRoot = TestContext.CurrentContext.TestDirectory.Split("bin")[0];

        // Create an example JSON log file to check results
        // As a one time setup for all tets in this class/fixture
        var ioHelper = TestHelper.IOHelper;
        var hostingEnv = TestHelper.GetHostingEnvironment();

        var loggingConfiguration = TestHelper.GetLoggingConfiguration(hostingEnv);

        var exampleLogfilePath = Path.Combine(testRoot, "TestHelpers", "Assets", LogfileName);
        _newLogfileDirPath = loggingConfiguration.LogDirectory;
        _newLogfilePath = Path.Combine(_newLogfileDirPath, LogfileName);

        // Create/ensure Directory exists
        ioHelper.EnsurePathExists(_newLogfileDirPath);

        // Copy the sample files
        File.Copy(exampleLogfilePath, _newLogfilePath, true);

        var logger = Mock.Of<ILogger<SerilogJsonLogViewer>>();
        var logViewerConfig = new LogViewerConfig(LogViewerQueryRepository, Mock.Of<IScopeProvider>());
        var logLevelLoader = Mock.Of<ILogLevelLoader>();
        _logViewer =
            new SerilogJsonLogViewer(logger, logViewerConfig, loggingConfiguration, logLevelLoader, Log.Logger);
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

    private ILogViewer _logViewer;

    private const string LogfileName = "UmbracoTraceLog.UNITTEST.20181112.json";

    private string _newLogfilePath;
    private string _newLogfileDirPath;

    private readonly LogTimePeriod _logTimePeriod = new(
        new DateTime(2018, 11, 12, 0, 0, 0),
        new DateTime(2018, 11, 13, 0, 0, 0));

    private ILogViewerQueryRepository LogViewerQueryRepository { get; } = new TestLogViewerQueryRepository();

    [Test]
    public void Logs_Contain_Correct_Error_Count()
    {
        var numberOfErrors = _logViewer.GetNumberOfErrors(_logTimePeriod);

        // Our dummy log should contain 2 errors
        Assert.AreEqual(1, numberOfErrors);
    }

    [Test]
    public void Logs_Contain_Correct_Log_Level_Counts()
    {
        var logCounts = _logViewer.GetLogLevelCounts(_logTimePeriod);

        Assert.AreEqual(55, logCounts.Debug);
        Assert.AreEqual(1, logCounts.Error);
        Assert.AreEqual(0, logCounts.Fatal);
        Assert.AreEqual(38, logCounts.Information);
        Assert.AreEqual(6, logCounts.Warning);
    }

    [Test]
    public void Logs_Contains_Correct_Message_Templates()
    {
        var templates = _logViewer.GetMessageTemplates(_logTimePeriod).ToArray();

        // Count no of templates
        Assert.AreEqual(25, templates.Count());

        // Verify all templates & counts are unique
        CollectionAssert.AllItemsAreUnique(templates);

        // Ensure the collection contains LogTemplate objects
        CollectionAssert.AllItemsAreInstancesOfType(templates, typeof(LogTemplate));

        // Get first item & verify its template & count are what we expect
        var popularTemplate = templates.FirstOrDefault();

        Assert.IsNotNull(popularTemplate);
        Assert.AreEqual("{EndMessage} ({Duration}ms) [Timing {TimingId}]", popularTemplate.MessageTemplate);
        Assert.AreEqual(26, popularTemplate.Count);
    }

    [Test]
    public void Logs_Can_Open_As_Small_File()
    {
        // We are just testing a return value (as we know the example file is less than 200MB)
        // But this test method does not test/check that
        var canOpenLogs = _logViewer.CheckCanOpenLogs(_logTimePeriod);
        Assert.IsTrue(canOpenLogs);
    }

    [Test]
    public void Logs_Can_Be_Queried()
    {
        var sw = new Stopwatch();
        sw.Start();

        // Should get me the most 100 recent log entries & using default overloads for remaining params
        var allLogs = _logViewer.GetLogs(_logTimePeriod, 1);

        sw.Stop();

        // Check we get 100 results back for a page & total items all good :)
        Assert.AreEqual(100, allLogs.Items.Count());
        Assert.AreEqual(102, allLogs.TotalItems);
        Assert.AreEqual(2, allLogs.TotalPages);

        // Check collection all contain same object type
        CollectionAssert.AllItemsAreInstancesOfType(allLogs.Items, typeof(LogMessage));

        // Check first item is newest
        var newestItem = allLogs.Items.First();
        DateTimeOffset.TryParse("2018-11-12T08:39:18.1971147Z", out var newDate);
        Assert.AreEqual(newDate, newestItem.Timestamp);

        // Check we call method again with a smaller set of results & in ascending
        var smallQuery = _logViewer.GetLogs(_logTimePeriod, 1, 10, Direction.Ascending);
        Assert.AreEqual(10, smallQuery.Items.Count());
        Assert.AreEqual(11, smallQuery.TotalPages);

        // Check first item is oldest
        var oldestItem = smallQuery.Items.First();
        DateTimeOffset.TryParse("2018-11-12T08:34:45.8371142Z", out var oldDate);
        Assert.AreEqual(oldDate, oldestItem.Timestamp);

        // Check invalid log levels
        // Rather than expect 0 items - get all items back & ignore the invalid levels
        string[] invalidLogLevels = { "Invalid", "NotALevel" };
        var queryWithInvalidLevels = _logViewer.GetLogs(_logTimePeriod, 1, logLevels: invalidLogLevels);
        Assert.AreEqual(102, queryWithInvalidLevels.TotalItems);

        // Check we can call method with an array of logLevel (error & warning)
        string[] logLevels = { "Warning", "Error" };
        var queryWithLevels = _logViewer.GetLogs(_logTimePeriod, 1, logLevels: logLevels);
        Assert.AreEqual(7, queryWithLevels.TotalItems);

        // Query @Level='Warning' BUT we pass in array of LogLevels for Debug & Info (Expect to get 0 results)
        string[] logLevelMismatch = { "Debug", "Information" };
        var filterLevelQuery = _logViewer.GetLogs(
            _logTimePeriod,
            1,
            filterExpression: "@Level='Warning'",
            logLevels: logLevelMismatch);
        Assert.AreEqual(0, filterLevelQuery.TotalItems);
    }

    [TestCase("", 102)]
    [TestCase("Has(@Exception)", 1)]
    [TestCase("Has(@x)", 1)]
    [TestCase("Has(Duration) and Duration > 1000", 2)]
    [TestCase("Not(@Level = 'Verbose') and Not(@Level = 'Debug')", 45)]
    [TestCase("Not(@l = 'Verbose') and Not(@l = 'Debug')", 45)]
    [TestCase("StartsWith(SourceContext, 'Umbraco.Core')", 86)]
    [TestCase("@MessageTemplate = '{EndMessage} ({Duration}ms) [Timing {TimingId}]'", 26)]
    [TestCase("@mt = '{EndMessage} ({Duration}ms) [Timing {TimingId}]'", 26)]
    [TestCase("SortedComponentTypes[?] = 'Umbraco.Web.Search.ExamineComponent'", 1)]
    [TestCase("Contains(SortedComponentTypes[?], 'DatabaseServer')", 1)]
    [Test]
    public void Logs_Can_Query_With_Expressions(string queryToVerify, int expectedCount)
    {
        var testQuery = _logViewer.GetLogs(_logTimePeriod, 1, filterExpression: queryToVerify);
        Assert.AreEqual(expectedCount, testQuery.TotalItems);
    }

    [Test]
    public void Log_Search_Can_Persist()
    {
        // Add a new search
        _logViewer.AddSavedSearch("Unit Test Example", "Has(UnitTest)");

        var searches = _logViewer.GetSavedSearches();

        // Check if we can find the newly added item from the results we get back
        var findItem = searches.Where(x => x.Name == "Unit Test Example" && x.Query == "Has(UnitTest)");

        Assert.IsNotNull(findItem, "We should have found the saved search, but get no results");
        Assert.AreEqual(1, findItem.Count(), "Our list of searches should only contain one result");

        // TODO: Need someone to help me find out why these don't work
        // CollectionAssert.Contains(searches, savedSearch, "Can not find the new search that was saved");
        // Assert.That(searches, Contains.Item(savedSearch));

        // Remove the search from above & ensure it no longer exists
        _logViewer.DeleteSavedSearch("Unit Test Example", "Has(UnitTest)");

        searches = _logViewer.GetSavedSearches();
        findItem = searches.Where(x => x.Name == "Unit Test Example" && x.Query == "Has(UnitTest)");
        Assert.IsEmpty(findItem, "The search item should no longer exist");
    }
}

internal class TestLogViewerQueryRepository : ILogViewerQueryRepository
{
    public TestLogViewerQueryRepository() =>
        Store = new List<ILogViewerQuery>(MigrateLogViewerQueriesFromFileToDb._defaultLogQueries
            .Select(LogViewerQueryModelFactory.BuildEntity));

    private IList<ILogViewerQuery> Store { get; }

    private LogViewerQueryRepository.LogViewerQueryModelFactory LogViewerQueryModelFactory { get; } = new();

    public ILogViewerQuery Get(int id) => Store.FirstOrDefault(x => x.Id == id);

    public IEnumerable<ILogViewerQuery> GetMany(params int[] ids) =>
        ids.Any() ? Store.Where(x => ids.Contains(x.Id)) : Store;

    public bool Exists(int id) => Get(id) is not null;

    public void Save(ILogViewerQuery entity)
    {
        var item = Get(entity.Id);

        if (item is null)
        {
            Store.Add(entity);
        }
        else
        {
            item.Name = entity.Name;
            item.Query = entity.Query;
        }
    }

    public void Delete(ILogViewerQuery entity)
    {
        var item = Get(entity.Id);

        if (item is not null)
        {
            Store.Remove(item);
        }
    }

    public IEnumerable<ILogViewerQuery> Get(IQuery<ILogViewerQuery> query) => throw new NotImplementedException();

    public int Count(IQuery<ILogViewerQuery> query) => throw new NotImplementedException();

    public ILogViewerQuery GetByName(string name) => Store.FirstOrDefault(x => x.Name == name);
}
