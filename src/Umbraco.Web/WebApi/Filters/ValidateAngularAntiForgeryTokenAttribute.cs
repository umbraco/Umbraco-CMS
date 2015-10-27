using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Filters;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// A filter to check for the csrf token based on Angular's standard approach
    /// </summary>
    /// <remarks>
    /// Code derived from http://ericpanorel.net/2013/07/28/spa-authentication-and-csrf-mvc4-antiforgery-implementation/
    /// 
    /// If the authentication type is cookie based, then this filter will execute, otherwise it will be disabled
    /// </remarks>
    public sealed class ValidateAngularAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            var userIdentity = ((ApiController) actionContext.ControllerContext.Controller).User.Identity as ClaimsIdentity;
            if (userIdentity != null)
            {
                //if there is not CookiePath claim, then exist
                if (userIdentity.HasClaim(x => x.Type == ClaimTypes.CookiePath) == false)
                {
                    base.OnActionExecuting(actionContext);
                    return;
                }
            }

            string failedReason;
            if (AngularAntiForgeryHelper.ValidateHeaders(actionContext.Request.Headers, out failedReason) == false)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.ExpectationFailed);
                actionContext.Response.ReasonPhrase = failedReason;
                return;
            }

            base.OnActionExecuting(actionContext);
        }
    }
}