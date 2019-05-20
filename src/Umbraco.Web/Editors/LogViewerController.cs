using System;
using System.Collections.Generic;
using System.Web.Http;
using Umbraco.Core;
using Umbraco.Core.Logging.Viewer;
using Umbraco.Core.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

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

        private bool CanViewLogs()
        {
            //Can the interface deal with Large Files
            if (_logViewer.CanHandleLargeLogs)
                return true;

            //Interface CheckCanOpenLogs
            return _logViewer.CheckCanOpenLogs(startDate: DateTime.Now.AddDays(-1), endDate: DateTime.Now);
        }

        [HttpGet]
        public bool GetCanViewLogs()
        {
            return CanViewLogs();
        }

        [HttpGet]
        public int GetNumberOfErrors()
        {
            //We will need to stop the request if trying to do this on a 1GB file
            if (CanViewLogs() == false)
            {
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse("Unable to view logs, due to size"));
            }

            return _logViewer.GetNumberOfErrors(startDate: DateTime.Now.AddDays(-1), endDate: DateTime.Now);
        }

        [HttpGet]
        public LogLevelCounts GetLogLevelCounts()
        {
            //We will need to stop the request if trying to do this on a 1GB file
            if (CanViewLogs() == false)
            {
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse("Unable to view logs, due to size"));
            }

            return _logViewer.GetLogLevelCounts(startDate: DateTime.Now.AddDays(-1), endDate: DateTime.Now);
        }

        [HttpGet]
        public IEnumerable<LogTemplate> GetMessageTemplates()
        {
            //We will need to stop the request if trying to do this on a 1GB file
            if (CanViewLogs() == false)
            {
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse("Unable to view logs, due to size"));
            }

            return _logViewer.GetMessageTemplates(startDate: DateTime.Now.AddDays(-1), endDate: DateTime.Now);
        }

        [HttpGet]
        public PagedResult<LogMessage> GetLogs(string orderDirection = "Descending", int pageNumber = 1, string filterExpression = null, [FromUri]string[] logLevels = null)
        {
            //We will need to stop the request if trying to do this on a 1GB file
            if (CanViewLogs() == false)
            {
                throw new HttpResponseException(Request.CreateNotificationValidationErrorResponse("Unable to view logs, due to size"));
            }

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
