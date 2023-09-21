using System.Security.Principal;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Api.Management.Security.Authorization;

/// <summary>
///     Utility class for working with policy authorizers.
/// </summary>
public interface IAuthorizationHelper
{
    /// <summary>
    ///     Converts an <see cref="IUser" /> into <see cref="IPrincipal" />.
    /// </summary>
    /// <param name="currentUser">The current user's principal.</param>
    /// <returns>
    ///     <see cref="IUser" />.
    /// </returns>
    IUser GetCurrentUser(IPrincipal currentUser);
}
