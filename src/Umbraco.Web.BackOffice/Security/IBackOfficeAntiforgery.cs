using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Umbraco.Cms.Core;
using Umbraco.Core;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Antiforgery implementation for the Umbraco back office
    /// </summary>
    public interface IBackOfficeAntiforgery
    {
        /// <summary>
        /// Validates the headers/cookies passed in for the request
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <param name="failedReason"></param>
        /// <returns></returns>
        Task<Attempt<string>> ValidateRequestAsync(HttpContext httpContext);

        /// <summary>
        /// Generates tokens to use for the cookie and header antiforgery values
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="cookieToken"></param>
        /// <param name="headerToken"></param>
        void GetTokens(HttpContext httpContext, out string cookieToken, out string headerToken);
    }
}
