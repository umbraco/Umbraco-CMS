using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Core.Logging.Viewer;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Backoffice controller supporting the dashboard for viewing logs with some simple graphs & filtering
    /// </summary>
    [PluginController("UmbracoApi")]
    public class LogsController : UmbracoAuthorizedJsonController
    {
        private ILogViewer _logViewer;

        public LogsController(ILogViewer logViewer)
        {
            _logViewer = logViewer;
        }

        [HttpGet]
        public int GetNumberOfErrors()
        {
            return _logViewer.GetNumberOfErrors();
        }
        
        [HttpGet]
        public LogLevelCounts GetLogLevelCounts()
        {
            return _logViewer.GetLogLevelCounts();
        }

        [HttpGet]
        public IEnumerable<CommonLogMessage> GetCommonLogMessages()
        {
            return _logViewer.GetCommonLogMessages();
        }

        [HttpGet]
        public IEnumerable<LogMessage> GetLogs()
        {
            return _logViewer.GetLogs();
        }

    }
}
