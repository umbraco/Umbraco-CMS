using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Logging.Viewer;

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

        private DateTimeOffset _startDate = new DateTime(year: 2018, month: 11, day: 12, hour:0, minute:0, second:0);
        private DateTimeOffset _endDate = new DateTime(year: 2018, month: 11, day: 13, hour: 0, minute: 0, second: 0);

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

            _logViewer = new JsonLogViewer(logsPath: _newLogfileDirPath, searchPath: _newSearchfilePath);
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
            var numberOfErrors = _logViewer.GetNumberOfErrors(startDate: _startDate, endDate: _endDate);

            //Our dummy log should contain 2 errors
            Assert.AreEqual(2, numberOfErrors);
        }

        [Test]
        public void Logs_Contain_Correct_Log_Level_Counts()
        {
            var logCounts = _logViewer.GetLogLevelCounts(startDate: _startDate, endDate: _endDate);

            Assert.AreEqual(1954, logCounts.Debug);
            Assert.AreEqual(2, logCounts.Error);
            Assert.AreEqual(0, logCounts.Fatal);
            Assert.AreEqual(62, logCounts.Information);
            Assert.AreEqual(7, logCounts.Warning);
        }

        [Test]
        public void Logs_Contains_Correct_Message_Templates()
        {
            var templates = _logViewer.GetMessageTemplates(startDate: _startDate, endDate: _endDate);

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
            var canOpenLogs = _logViewer.CheckCanOpenLogs(startDate: _startDate, endDate: _endDate);
            Assert.IsTrue(canOpenLogs);
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

            //TODO: Need someone to help me find out why these don't work
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
