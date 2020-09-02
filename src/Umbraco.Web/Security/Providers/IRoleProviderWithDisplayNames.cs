using Umbraco.Web.Models;

namespace Umbraco.Web.Security.Providers
{
    /// <summary>
    /// Provides a way for a custom RoleProvider to expose roles with a friendly name and icon for display in the backoffice.
    /// </summary>
    public interface IRoleProviderWithDisplayNames
    {
        /// <summary>
        /// Creates a list of roles with display names and icons.
        /// </summary>
        /// <returns></returns>
        RoleDisplay[] GetAllRolesWithDisplayName();
    }
}
