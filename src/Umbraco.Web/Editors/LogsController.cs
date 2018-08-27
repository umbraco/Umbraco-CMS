using System.Web.Mvc;
using Umbraco.Core.Logging.Viewer;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// Backoffice controller supporting the dashboard for language administration.
    /// </summary>
    [PluginController("UmbracoApi")]
    public class LogsController : UmbracoAuthorizedJsonController
    {
        private LogViewer _logViewer;

        public LogsController()
        {
            _logViewer = new LogViewer();
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
    }
}
