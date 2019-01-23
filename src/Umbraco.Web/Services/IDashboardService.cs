using System.Collections.Generic;
using Umbraco.Core.Dashboards;
using Umbraco.Core.Models.Membership;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Services
{
    public interface IDashboardService
    {
        /// <summary>
        /// Gets dashboard for a specific section/application
        /// For a specific backoffice user
        /// </summary>
        /// <param name="section"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        IEnumerable<Tab<IDashboardSection>> GetDashboards(string section, IUser currentUser);

        /// <summary>
        /// Gets all dashboards, organized by section, for a user.
        /// </summary>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        IDictionary<string, IEnumerable<Tab<IDashboardSection>>> GetDashboards(IUser currentUser);

    }
}
