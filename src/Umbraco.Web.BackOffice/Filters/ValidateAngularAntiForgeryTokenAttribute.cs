using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Umbraco.Core;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Security;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// An attribute/filter to check for the csrf token based on Angular's standard approach
    /// </summary>
    /// <remarks>
    /// Code derived from http://ericpanorel.net/2013/07/28/spa-authentication-and-csrf-mvc4-antiforgery-implementation/
    ///
    /// If the authentication type is cookie based, then this filter will execute, otherwise it will be disabled
    /// </remarks>
    public sealed class ValidateAngularAntiForgeryTokenAttribute : TypeFilterAttribute
    {
        public ValidateAngularAntiForgeryTokenAttribute()
            : base(typeof(ValidateAngularAntiForgeryTokenFilter))
        {
        }

        private class ValidateAngularAntiForgeryTokenFilter : IAsyncActionFilter
        {
            private readonly ILogger<ValidateAngularAntiForgeryTokenFilter> _logger;
            private readonly IBackOfficeAntiforgery _antiforgery;
            private readonly ICookieManager _cookieManager;

            public ValidateAngularAntiForgeryTokenFilter(ILogger<ValidateAngularAntiForgeryTokenFilter> logger, IBackOfficeAntiforgery antiforgery, ICookieManager cookieManager)
            {
                _logger = logger;
                _antiforgery = antiforgery;
                _cookieManager = cookieManager;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (context.Controller is ControllerBase controller && controller.User.Identity is ClaimsIdentity userIdentity)
                {
                    // if there is not CookiePath claim, then exit
                    if (userIdentity.HasClaim(x => x.Type == ClaimTypes.CookiePath) == false)
                    {
                        await next();
                    }
                }

                var cookieToken = _cookieManager.GetCookieValue(Constants.Web.CsrfValidationCookieName);
                var httpContext = context.HttpContext;

                var validateResult = await ValidateHeaders(httpContext, cookieToken);
                if (validateResult.Item1 == false)
                {
                    httpContext.SetReasonPhrase(validateResult.Item2);
                    context.Result = new StatusCodeResult((int)HttpStatusCode.ExpectationFailed);
                    return;
                }

                await next();
            }

            private async Task<(bool, string)> ValidateHeaders(
                HttpContext httpContext,
                string cookieToken)
            {
                var requestHeaders = httpContext.Request.Headers;
                if (requestHeaders.Any(z => z.Key.InvariantEquals(Constants.Web.AngularHeadername)) == false)
                {
                    return (false, "Missing token");
                }

                var headerToken = requestHeaders
                    .Where(z => z.Key.InvariantEquals(Constants.Web.AngularHeadername))
                    .Select(z => z.Value)
                    .SelectMany(z => z)
                    .FirstOrDefault();

                // both header and cookie must be there
                if (cookieToken == null || headerToken == null)
                {
                    return (false, "Missing token null");
                }

                if (await ValidateTokens(httpContext) == false)
                {
                    return (false, "Invalid token");
                }

                return (true, "Success");
            }

            private async Task<bool> ValidateTokens(HttpContext httpContext)
            {
                // ensure that the cookie matches the header and then ensure it matches the correct value!
                try
                {
                    await _antiforgery.ValidateRequestAsync(httpContext);
                    return true;
                }
                catch (AntiforgeryValidationException ex)
                {
                    _logger.LogError(ex, "Could not validate XSRF token");
                    return false;
                }
            }
        }
    }
}
