using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.BackOffice.Security;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     An attribute/filter to check for the csrf token based on Angular's standard approach
/// </summary>
public sealed class ValidateAngularAntiForgeryTokenAttribute : TypeFilterAttribute
{
    public ValidateAngularAntiForgeryTokenAttribute()
        : base(typeof(ValidateAngularAntiForgeryTokenFilter))
    {
    }

    private class ValidateAngularAntiForgeryTokenFilter : IAsyncActionFilter
    {
        private readonly IBackOfficeAntiforgery _antiforgery;

        public ValidateAngularAntiForgeryTokenFilter(IBackOfficeAntiforgery antiforgery) => _antiforgery = antiforgery;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.Controller is ControllerBase controller &&
                controller.User.Identity is ClaimsIdentity userIdentity)
            {
                // if there is not CookiePath claim, then exit
                if (userIdentity.HasClaim(x => x.Type == ClaimTypes.CookiePath) == false)
                {
                    await next();
                    return;
                }
            }

            HttpContext httpContext = context.HttpContext;
            Attempt<string?> validateResult = await _antiforgery.ValidateRequestAsync(httpContext);
            if (!validateResult.Success)
            {
                httpContext.SetReasonPhrase(validateResult.Result);
                context.Result = new StatusCodeResult((int)HttpStatusCode.ExpectationFailed);
                return;
            }

            await next();
        }
    }
}
