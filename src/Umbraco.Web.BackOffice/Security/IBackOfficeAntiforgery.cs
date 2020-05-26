using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Umbraco.Core;

namespace Umbraco.Web.BackOffice.Security
{
    /// <summary>
    /// Antiforgery implementation for the Umbraco back office
    /// </summary>
    public interface IBackOfficeAntiforgery //: IAntiforgery
    {
        Task<Attempt<string>> ValidateHeadersAsync(HttpContext httpContext);
        Task<bool> ValidateTokensAsync(HttpContext httpContext, string cookieToken, string headerToken);
        void GetTokens(HttpContext httpContext, out string cookieToken, out string headerToken);
    }
}
