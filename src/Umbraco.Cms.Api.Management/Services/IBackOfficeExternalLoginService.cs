using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Api.Management.Services;

public interface IBackOfficeExternalLoginService
{
    Task<Attempt<IEnumerable<UserExternalLoginProviderModel>, ExternalLoginOperationStatus>> ExternalLoginStatusForUserAsync(Guid userKey);

    Task<Attempt<ExternalLoginOperationStatus>> UnLinkLoginAsync(ClaimsPrincipal claimsPrincipal, string loginProvider, string providerKey);

    Task<Attempt<IEnumerable<IdentityError>, ExternalLoginOperationStatus>> HandleLoginCallbackAsync(HttpContext httpContext);

    Task<Attempt<Guid?, ExternalLoginOperationStatus>> GenerateLoginProviderSecretAsync(
        ClaimsPrincipal claimsPrincipal,
        string loginProvider);

    Task<Attempt<ClaimsPrincipal?, ExternalLoginOperationStatus>> ClaimsPrincipleFromLoginProviderLinkKeyAsync(
        string loginProvider,
        Guid linkKey);
}
