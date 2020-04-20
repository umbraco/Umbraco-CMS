using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Forces the response to have a specific http status code
    /// </summary>
    public class StatusCodeResultAttribute : ActionFilterAttribute
    {
        private readonly HttpStatusCode _statusCode;

        public StatusCodeResultAttribute(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);

            context.HttpContext.Response.StatusCode = (int)_statusCode;

            var disableIisCustomErrors = context.HttpContext.RequestServices.GetService<IWebRoutingSettings>().TrySkipIisCustomErrors;
            var statusCodePagesFeature = context.HttpContext.Features.Get<IStatusCodePagesFeature>();

            if (statusCodePagesFeature != null)
            {
                // if IIS Custom Errors are disabled, we won't enable the Status Code Pages
                statusCodePagesFeature.Enabled = !disableIisCustomErrors;
            }
        }
    }
}
