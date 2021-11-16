using Moq;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Logging.Viewer;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Tests.Logging
{
    [TestFixture]
    public class LogviewerTests
    {
        private ILogViewer _logViewer;

        const string _logfileName = "UmbracoTraceLog.UNITTEST.20181112.json";
        const string _searchfileName = "logviewer.searches.config.js";

        private string _newLogfilePath;
        private string _newLogfileDirPath;

        private string _newSearchfilePath;
        private string _newSearchfileDirPath;

        private LogTimePeriod _logTimePeriod = new LogTimePeriod(
            new DateTime(year: 2018, month: 11, day: 12, hour:0, minute:0, second:0),
            new DateTime(year: 2018, month: 11, day: 13, hour: 0, minute: 0, second: 0)
            );
        [OneTimeSetUp]
        public void Setup()
        {
            //Create an example JSON log file to check results
            //As a one time setup for all tets in this class/fixture

            var exampleLogfilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Logging\", _logfileName);
            _newLogfileDirPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"App_Data\Logs\");
            _newLogfilePath = Path.Combine(_newLogfileDirPath, _logfileName);

            var exampleSearchfilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Logging\", _searchfileName);
            _newSearchfileDirPath = Path.Combine(TestContext.CurrentContext.TestDirectory, @"Config\");
            _newSearchfilePath = Path.Combine(_newSearchfileDirPath, _searchfileName);

            //Create/ensure Directory exists
            IOHelper.EnsurePathExists(_newLogfileDirPath);
            IOHelper.EnsurePathExists(_newSearchfileDirPath);

            //Copy the sample files
            File.Copy(exampleLogfilePath, _newLogfilePath, true);
            File.Copy(exampleSearchfilePath, _newSearchfilePath, true);

            var logger = Mock.Of<Core.Logging.ILogger>();
            _logViewer = new JsonLogViewer(logger, logsPath: _newLogfileDirPath, searchPath: _newSearchfilePath);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            //Cleanup & delete the example log & search files off disk
            //Once all tests in this class/fixture have run
            if (File.Exists(_newLogfilePath))
                File.Delete(_newLogfilePath);

            if (File.Exists(_newSearchfilePath))
                File.Delete(_newSearchfilePath);
        }

        [Test]
        public void Logs_Contain_Correct_Error_Count()
        {
            var numberOfErrors = _logViewer.GetNumberOfErrors(_logTimePeriod);

            //Our dummy log should contain 2 errors
            Assert.AreEqual(2, numberOfErrors);
        }

        [Test]
        public void Logs_Contain_Correct_Log_Level_Counts()
        {
            var logCounts = _logViewer.GetLogLevelCounts(_logTimePeriod);

            Assert.AreEqual(1954, logCounts.Debug);
            Assert.AreEqual(2, logCounts.Error);
            Assert.AreEqual(0, logCounts.Fatal);
            Assert.AreEqual(62, logCounts.Information);
            Assert.AreEqual(7, logCounts.Warning);
        }

        [Test]
        public void Logs_Contains_Correct_Message_Templates()
        {
            var templates = _logViewer.GetMessageTemplates(_logTimePeriod);

            //Count no of templates
            Assert.AreEqual(43, templates.Count());

            //Verify all templates & counts are unique
            CollectionAssert.AllItemsAreUnique(templates);

            //Ensure the collection contains LogTemplate objects
            CollectionAssert.AllItemsAreInstancesOfType(templates, typeof(LogTemplate));

            //Get first item & verify its template & count are what we expect
            var popularTemplate = templates.FirstOrDefault();

            Assert.IsNotNull(popularTemplate);
            Assert.AreEqual("{LogPrefix} Task added {TaskType}", popularTemplate.MessageTemplate);
            Assert.AreEqual(689, popularTemplate.Count);
        }

        [Test]
        public void Logs_Can_Open_As_Small_File()
        {
            //We are just testing a return value (as we know the example file is less than 200MB)
            //But this test method does not test/check that
            var canOpenLogs = _logViewer.CheckCanOpenLogs(_logTimePeriod);
            Assert.IsTrue(canOpenLogs);
        }

        [Test]
        public void Logs_Can_Be_Queried()
        {
            //Should get me the most 100 recent log entries & using default overloads for remaining params
            var allLogs = _logViewer.GetLogs(_logTimePeriod, pageNumber: 1);

            //Check we get 100 results back for a page & total items all good :)
            Assert.AreEqual(100, allLogs.Items.Count());
            Assert.AreEqual(2410, allLogs.TotalItems);
            Assert.AreEqual(25, allLogs.TotalPages);

            //Check collection all contain same object type
            CollectionAssert.AllItemsAreInstancesOfType(allLogs.Items, typeof(LogMessage));

            //Check first item is newest
            var newestItem = allLogs.Items.First();
            DateTimeOffset newDate;
            DateTimeOffset.TryParse("2018-11-12T09:24:27.4057583Z", out newDate);
            Assert.AreEqual(newDate, newestItem.Timestamp);


            //Check we call method again with a smaller set of results & in ascending
            var smallQuery = _logViewer.GetLogs(_logTimePeriod, pageNumber: 1, pageSize: 10, orderDirection: Direction.Ascending);
            Assert.AreEqual(10, smallQuery.Items.Count());
            Assert.AreEqual(241, smallQuery.TotalPages);

            //Check first item is oldest
            var oldestItem = smallQuery.Items.First();
            DateTimeOffset oldDate;
            DateTimeOffset.TryParse("2018-11-12T08:34:45.8371142Z", out oldDate);
            Assert.AreEqual(oldDate, oldestItem.Timestamp);


            //Check invalid log levels
            //Rather than expect 0 items - get all items back & ignore the invalid levels
            string[] invalidLogLevels = { "Invalid", "NotALevel" };
            var queryWithInvalidLevels = _logViewer.GetLogs(_logTimePeriod, pageNumber: 1, logLevels: invalidLogLevels);
            Assert.AreEqual(2410, queryWithInvalidLevels.TotalItems);

            //Check we can call method with an array of logLevel (error & warning)
            string [] logLevels = { "Warning", "Error" };
            var queryWithLevels = _logViewer.GetLogs(_logTimePeriod, pageNumber: 1, logLevels: logLevels);
            Assert.AreEqual(9, queryWithLevels.TotalItems);

            //Query @Level='Warning' BUT we pass in array of LogLevels for Debug & Info (Expect to get 0 results)
            string[] logLevelMismatch = { "Debug", "Information" };
            var filterLevelQuery = _logViewer.GetLogs(_logTimePeriod, pageNumber: 1, filterExpression: "@Level='Warning'", logLevels: logLevelMismatch);
            Assert.AreEqual(0, filterLevelQuery.TotalItems);
        }

        [TestCase("", 2410)]
        [TestCase("Has(@Exception)", 2)]
        [TestCase("Has(Duration) and Duration > 1000", 13)]
        [TestCase("Not(@Level = 'Verbose') and Not(@Level= 'Debug')", 71)]
        [TestCase("StartsWith(SourceContext, 'Umbraco.Core')", 1183)]
        [TestCase("@MessageTemplate = '{EndMessage} ({Duration}ms) [Timing {TimingId}]'", 622)]
        [TestCase("SortedComponentTypes[?] = 'Umbraco.Web.Search.ExamineComponent'", 1)]
        [TestCase("Contains(SortedComponentTypes[?], 'DatabaseServer')", 1)]
        [Test]
        public void Logs_Can_Query_With_Expressions(string queryToVerify, int expectedCount)
        {
            var testQuery = _logViewer.GetLogs(_logTimePeriod, pageNumber: 1, filterExpression: queryToVerify);
            Assert.AreEqual(expectedCount, testQuery.TotalItems);
        }

        [Test]
        public void Log_Search_Can_Persist()
        {
            //Add a new search
            _logViewer.AddSavedSearch("Unit Test Example", "Has(UnitTest)");

            var searches = _logViewer.GetSavedSearches();

            var savedSearch = new SavedLogSearch
            {
                Name = "Unit Test Example",
                Query = "Has(UnitTest)"
            };

            //Check if we can find the newly added item from the results we get back
            var findItem = searches.Where(x => x.Name == "Unit Test Example" && x.Query == "Has(UnitTest)");

            Assert.IsNotNull(findItem, "We should have found the saved search, but get no results");
            Assert.AreEqual(1, findItem.Count(), "Our list of searches should only contain one result");

            // TODO: Need someone to help me find out why these don't work
            //CollectionAssert.Contains(searches, savedSearch, "Can not find the new search that was saved");
            //Assert.That(searches, Contains.Item(savedSearch));

            //Remove the search from above & ensure it no longer exists
            _logViewer.DeleteSavedSearch("Unit Test Example", "Has(UnitTest)");

            searches = _logViewer.GetSavedSearches();
            findItem = searches.Where(x => x.Name == "Unit Test Example" && x.Query == "Has(UnitTest)");
            Assert.IsEmpty(findItem, "The search item should no longer exist");
        }
    }
}
