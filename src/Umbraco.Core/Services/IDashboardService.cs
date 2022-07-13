using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Services;

public interface IDashboardService
{
    /// <summary>
    ///     Gets dashboard for a specific section/application
    ///     For a specific backoffice user
    /// </summary>
    /// <param name="section"></param>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    IEnumerable<Tab<IDashboard>> GetDashboards(string section, IUser? currentUser);

    /// <summary>
    ///     Gets all dashboards, organized by section, for a user.
    /// </summary>
    /// <param name="currentUser"></param>
    /// <returns></returns>
    IDictionary<string, IEnumerable<Tab<IDashboard>>> GetDashboards(IUser? currentUser);
}
