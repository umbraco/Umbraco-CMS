using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Services;

/// <summary>
/// Provides methods for managing external authentication and login providers in the back office.
/// </summary>
public interface IBackOfficeExternalLoginService
{
    /// <summary>
    /// Gets the external login status for a user.
    /// </summary>
    /// <param name="userKey">The unique identifier of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an attempt with the external login providers and the operation status.</returns>
    Task<Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>> ExternalLoginStatusForUserAsync(Guid userKey);

    /// <summary>
    /// Unlinks an external login from the specified back office user.
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal representing the back office user.</param>
    /// <param name="loginProvider">The external login provider to unlink.</param>
    /// <param name="providerKey">The key identifying the external login to unlink.</param>
    /// <returns>An attempt result indicating the status of the unlink operation.</returns>
    Task<Attempt<ExternalLoginOperationStatus>> UnLinkLoginAsync(ClaimsPrincipal claimsPrincipal, string loginProvider, string providerKey);

    /// <summary>
    /// Handles the login callback from an external login provider.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an attempt with either a collection of identity errors or the external login operation status.</returns>
    Task<Attempt<IEnumerable<IdentityError>, ExternalLoginOperationStatus>> HandleLoginCallbackAsync(HttpContext httpContext);

    /// <summary>
    /// Generates a secret for the specified external login provider.
    /// </summary>
    /// <param name="claimsPrincipal">The claims principal representing the user.</param>
    /// <param name="loginProvider">The external login provider identifier.</param>
    /// <returns>An attempt containing the generated secret's GUID if successful, along with the operation status.</returns>
    Task<Attempt<Guid?, ExternalLoginOperationStatus>> GenerateLoginProviderSecretAsync(
        ClaimsPrincipal claimsPrincipal,
        string loginProvider);

    /// <summary>Retrieves a ClaimsPrincipal based on the specified login provider and link key.</summary>
    /// <param name="loginProvider">The login provider identifier.</param>
    /// <param name="linkKey">The unique link key associated with the external login.</param>
    /// <returns>An Attempt containing a ClaimsPrincipal if successful, or an ExternalLoginOperationStatus indicating failure.</returns>
    Task<Attempt<ClaimsPrincipal?, ExternalLoginOperationStatus>> ClaimsPrincipleFromLoginProviderLinkKeyAsync(
        string loginProvider,
        Guid linkKey);
}
