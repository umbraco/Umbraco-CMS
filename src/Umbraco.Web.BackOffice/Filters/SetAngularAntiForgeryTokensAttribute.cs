using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Web.BackOffice.Security;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     An attribute/filter to set the csrf cookie token based on angular conventions
/// </summary>
public class SetAngularAntiForgeryTokensAttribute : TypeFilterAttribute
{
    public SetAngularAntiForgeryTokensAttribute() : base(typeof(SetAngularAntiForgeryTokensFilter))
    {
    }

    internal class SetAngularAntiForgeryTokensFilter : IAsyncActionFilter
    {
        private readonly IBackOfficeAntiforgery _antiforgery;

        public SetAngularAntiForgeryTokensFilter(IBackOfficeAntiforgery antiforgery)
            => _antiforgery = antiforgery;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await next();

            // anti forgery tokens are based on the currently logged
            // in user assigned to the HttpContext which will be assigned during signin so
            // we can only execute after the action.
            if (context.HttpContext.Response?.StatusCode == (int)HttpStatusCode.OK)
            {
                _antiforgery.GetAndStoreTokens(context.HttpContext);
            }
        }
    }
}
