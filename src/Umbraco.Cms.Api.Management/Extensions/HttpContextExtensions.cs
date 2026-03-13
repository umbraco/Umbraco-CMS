using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.Security;

namespace Umbraco.Extensions;

/// <summary>
/// Contains extension methods for enhancing the functionality of the <see cref="HttpContext"/> class.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Stores the specified <see cref="BackOfficeExternalLoginProviderErrors"/> instance in the <see cref="HttpContext.Items"/> collection.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> on which to set the errors.</param>
    /// <param name="errors">The <see cref="BackOfficeExternalLoginProviderErrors"/> to store.</param>
    public static void SetExternalLoginProviderErrors(this HttpContext httpContext, BackOfficeExternalLoginProviderErrors errors)
        => httpContext.Items[nameof(BackOfficeExternalLoginProviderErrors)] = errors;

    /// <summary>
    /// Retrieves the external login provider errors from the current HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context to retrieve errors from.</param>
    /// <returns>The external login provider errors if present; otherwise, null.</returns>
    public static BackOfficeExternalLoginProviderErrors? GetExternalLoginProviderErrors(this HttpContext httpContext)
        => httpContext.Items[nameof(BackOfficeExternalLoginProviderErrors)] as BackOfficeExternalLoginProviderErrors;
}
