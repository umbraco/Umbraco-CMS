using NUnit.Framework;
using System;
using System.IO;
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

        
    }
}
