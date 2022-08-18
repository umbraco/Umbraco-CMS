using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Web.BackOffice.Controllers;

namespace Umbraco.Extensions;

public static class HttpContextExtensions
{
    public static void SetExternalLoginProviderErrors(this HttpContext httpContext, BackOfficeExternalLoginProviderErrors errors)
        => httpContext.Items[nameof(BackOfficeExternalLoginProviderErrors)] = errors;

    public static BackOfficeExternalLoginProviderErrors? GetExternalLoginProviderErrors(this HttpContext httpContext)
        => httpContext.Items[nameof(BackOfficeExternalLoginProviderErrors)] as BackOfficeExternalLoginProviderErrors;

    public static string GetAuthorizedServiceRedirectUri(this HttpContext httpContext)
        => $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/umbraco/api/{nameof(AuthorizedServiceResponseController).Replace("Controller", string.Empty)}/{nameof(AuthorizedServiceResponseController.HandleIdentityResponse)}";
}
