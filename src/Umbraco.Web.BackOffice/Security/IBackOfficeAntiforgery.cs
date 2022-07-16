using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Web.BackOffice.Security;

/// <summary>
///     Antiforgery implementation for the Umbraco back office
/// </summary>
public interface IBackOfficeAntiforgery
{
    /// <summary>
    ///     Validates the headers/cookies passed in for the request
    /// </summary>
    /// <param name="requestHeaders"></param>
    /// <param name="failedReason"></param>
    /// <returns></returns>
    Task<Attempt<string?>> ValidateRequestAsync(HttpContext httpContext);

    /// <summary>
    ///     Generates tokens to use for the cookie and header antiforgery values
    /// </summary>
    /// <param name="httpContext"></param>
    /// <param name="cookieToken"></param>
    /// <param name="headerToken"></param>
    void GetAndStoreTokens(HttpContext httpContext);
}
