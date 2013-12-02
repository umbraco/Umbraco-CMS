using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// A filter to check for the csrf token based on Angular's standard approach
    /// </summary>
    /// <remarks>
    /// Code derived from http://ericpanorel.net/2013/07/28/spa-authentication-and-csrf-mvc4-antiforgery-implementation/
    /// </remarks>
    public sealed class ValidateAngularAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            string failedReason;
            if (AngularAntiForgeryHelper.Validate(actionContext.Request.Headers, out failedReason) == false)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.ExpectationFailed);
                actionContext.Response.ReasonPhrase = failedReason;
                return;
            }

            base.OnActionExecuting(actionContext);
        }
    }
}