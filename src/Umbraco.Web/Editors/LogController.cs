using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using umbraco.BusinessLogic;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Editors
{
    /// <summary>
    /// The API controller used for getting log history
    /// </summary>
    [PluginController("UmbracoApi")]
    public class LogController : UmbracoAuthorizedJsonController
    {
        public IEnumerable<LogItem> GetEntityLog([FromUri] int id)
        {
            return Log.Instance.GetLogItems(id);
        }

        public IEnumerable<LogItem> GetUserLog([FromUri] string logType, [FromUri] DateTime sinceDate)
        {
            if (sinceDate == null)
                sinceDate = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0, 0));

            var u = new User(Security.CurrentUser);
            return Log.Instance.GetLogItems(u, (LogTypes)Enum.Parse(typeof(LogTypes), logType), sinceDate); 
        }

        public IEnumerable<LogItem> GetLog([FromUri] string logType, DateTime sinceDate)
        {
            if (sinceDate == null)
                sinceDate = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0, 0));

            return Log.Instance.GetLogItems( (LogTypes)Enum.Parse(typeof(LogTypes), logType), sinceDate);
        }
    }
}
