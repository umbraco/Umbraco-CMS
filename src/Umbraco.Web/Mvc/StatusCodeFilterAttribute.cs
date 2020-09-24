using System.Net;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Forces the response to have a specific http status code
    /// </summary>
    internal class StatusCodeResultAttribute : ActionFilterAttribute
    {
        private readonly HttpStatusCode _statusCode;

        public StatusCodeResultAttribute(HttpStatusCode statusCode)
        {
            _statusCode = statusCode;
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            filterContext.HttpContext.Response.StatusCode = (int)_statusCode;
            filterContext.HttpContext.Response.TrySkipIisCustomErrors = Current.Configs.Settings().WebRouting.TrySkipIisCustomErrors;
        }
    }
}
