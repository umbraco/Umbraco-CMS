using System.Diagnostics.CodeAnalysis;
using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     Utility class for working with policy authorizers.
/// </summary>
public interface IAuthorizationHelper
{
    /// <summary>
    ///     Converts an <see cref="IPrincipal" /> into an <see cref="IUser" />.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <returns>
    ///     <see cref="IUser" />.
    /// </returns>
    IUser GetUmbracoUser(IPrincipal currentUser);

    /// <summary>
    ///     Attempts to convert an <see cref="IPrincipal" /> into an <see cref="IUser" />.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <param name="user">The resulting <see cref="IUser" />, if the conversion is successful.</param>
    /// <returns>True if the conversion is successful, false otherwise</returns>
    bool TryGetUmbracoUser(IPrincipal currentUser, [NotNullWhen(true)] out IUser? user)
    {
        try
        {
            user = GetUmbracoUser(currentUser);
            return true;
        }
        catch
        {
            user = null;
            return false;
        }
    }
}
