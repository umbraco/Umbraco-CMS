using System.Security.Claims;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Provides core sign-in functionality for back office users.
/// </summary>
public interface ICoreBackOfficeSignInManager
{
    /// <summary>
    ///     Creates a <see cref="ClaimsPrincipal" /> for the specified user.
    /// </summary>
    /// <param name="userKey">The unique key of the user.</param>
    /// <returns>A task that resolves to the <see cref="ClaimsPrincipal" /> for the user, or <c>null</c> if the user is not found.</returns>
    Task<ClaimsPrincipal?> CreateUserPrincipalAsync(Guid userKey);
}
