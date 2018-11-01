using System;
using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Core.Logging.Viewer;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Backoffice controller supporting the dashboard for viewing logs with some simple graphs & filtering
    /// </summary>
    [PluginController("UmbracoApi")]
    public class LogViewerController : UmbracoAuthorizedJsonController
    {
        private ILogViewer _logViewer;

        public LogViewerController(ILogViewer logViewer)
        {
            _logViewer = logViewer;
        }

        [HttpGet]
        public IHttpActionResult GetCanViewLogs()
        {
            //Returns 200 OK if the logs can be viewed

            //Check if the ILogViewer is our JSON file 
            var isJsonLogViewer = _logViewer is JsonLogViewer;

            //Don't WARN or check if it's not our JSON disk file approach
            if (isJsonLogViewer == false)
            {                
                return Ok();
            }

            //Go & fetch the number of log entries OR 
            var logSize = _logViewer.GetLogSize(startDate: DateTime.Now.AddDays(-1), endDate: DateTime.Now);

            //The GetLogSize call on JsonLogViewer returns the total filesize in bytes
            //Check if the logsize is not greater than 200Mb
            //TODO: Convert the bytes to Megabytes and check less than 200Mb
            if (logSize >= 10)
            {
                return Ok(logSize);
            }

            //TODO: It may need to be an Umbraco request with errow/warning notification?!
            //Depends how best to bubble up to UI - with some custom JS promise error that is caught
            return BadRequest();

        }

        [HttpGet]
        public int GetNumberOfErrors()
        {
            //TODO: We will need to stop the request if trying to do this on a 1GB file

            return _logViewer.GetNumberOfErrors(startDate: DateTime.Now.AddDays(-1), endDate: DateTime.Now);
        }

        [HttpGet]
        public LogLevelCounts GetLogLevelCounts()
        {
            //TODO: We will need to stop the request if trying to do this on a 1GB file

            return _logViewer.GetLogLevelCounts(startDate: DateTime.Now.AddDays(-1), endDate: DateTime.Now);
        }

        [HttpGet]
        public IEnumerable<LogTemplate> GetMessageTemplates()
        {
            //TODO: We will need to stop the request if trying to do this on a 1GB file

            return _logViewer.GetMessageTemplates(startDate: DateTime.Now.AddDays(-1), endDate: DateTime.Now);
        }

        [HttpGet]
        public PagedResult<LogMessage> GetLogs(string orderDirection = "Descending", int pageNumber = 1, string filterExpression = null, [FromUri]string[] logLevels = null)
        {
            //TODO: We will need to stop the request if trying to do this on a 1GB file

            var direction = orderDirection == "Descending" ? Direction.Descending : Direction.Ascending;
            return _logViewer.GetLogs(startDate: DateTime.Now.AddDays(-1), endDate: DateTime.Now, filterExpression: filterExpression, pageNumber: pageNumber, orderDirection: direction, logLevels: logLevels);
        }

        [HttpGet]
        public IEnumerable<SavedLogSearch> GetSavedSearches()
        {
            return _logViewer.GetSavedSearches();
        }

        [HttpPost]
        public IEnumerable<SavedLogSearch> PostSavedSearch(SavedLogSearch item)
        {
            return _logViewer.AddSavedSearch(item.Name, item.Query);
        }

        [HttpPost]
        public IEnumerable<SavedLogSearch> DeleteSavedSearch(SavedLogSearch item)
        {
            return _logViewer.DeleteSavedSearch(item.Name, item.Query);
        }
    }
}
