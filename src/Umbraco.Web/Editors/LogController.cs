using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;
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
        public IEnumerable<AuditLog> GetEntityLog(int id)
        {
            return Mapper.Map<IEnumerable<AuditLog>>(
                Log.Instance.GetLogItems(id));
        }

        public IEnumerable<AuditLog> GetCurrentUserLog(AuditLogType logType, DateTime? sinceDate)
        {
            if (sinceDate == null)
                sinceDate = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0, 0));

            var u = new User(Security.CurrentUser);
            return Mapper.Map<IEnumerable<AuditLog>>(
                Log.Instance.GetLogItems(u, Enum<LogTypes>.Parse(logType.ToString()), sinceDate.Value));
        }

        public IEnumerable<AuditLog> GetLog(AuditLogType logType, DateTime? sinceDate)
        {
            if (sinceDate == null)
                sinceDate = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0, 0));

            return Mapper.Map<IEnumerable<AuditLog>>(
                Log.Instance.GetLogItems(Enum<LogTypes>.Parse(logType.ToString()), sinceDate.Value));
        }

    }
}
