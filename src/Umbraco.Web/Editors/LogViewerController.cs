using System;
using System.Collections.Generic;
using System.Web.Mvc;
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
        public int GetNumberOfErrors()
        {
            return _logViewer.GetNumberOfErrors(startDate: DateTime.Now, endDate: DateTime.Now);
        }
        
        [HttpGet]
        public LogLevelCounts GetLogLevelCounts()
        {
            return _logViewer.GetLogLevelCounts(startDate: DateTime.Now, endDate: DateTime.Now);
        }

        [HttpGet]
        public IEnumerable<CommonLogMessage> GetCommonLogMessages()
        {
            return _logViewer.GetCommonLogMessages(startDate: DateTime.Now, endDate: DateTime.Now);
        }

        [HttpGet]
        public PagedResult<LogMessage> GetLogs(string orderDirection = "Descending", int pageNumber = 1, string filterExpression = null)
        {
            var direction = orderDirection == "Descending" ? Direction.Descending : Direction.Ascending;
            return _logViewer.GetLogs(startDate: DateTime.Now, endDate: DateTime.Now, filterExpression: filterExpression, pageNumber: pageNumber, orderDirection: direction);
        }

    }
}
