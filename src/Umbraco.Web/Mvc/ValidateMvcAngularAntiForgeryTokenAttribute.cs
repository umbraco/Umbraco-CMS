using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A filter to check for the csrf token based on Angular's standard approach
    /// </summary>
    /// <remarks>
    /// Code derived from http://ericpanorel.net/2013/07/28/spa-authentication-and-csrf-mvc4-antiforgery-implementation/
    /// 
    /// If the authentication type is cookie based, then this filter will execute, otherwise it will be disabled
    /// </remarks>
    public sealed class ValidateMvcAngularAntiForgeryTokenAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var userIdentity = filterContext.HttpContext.User.Identity as ClaimsIdentity;
            if (userIdentity != null)
            {
                //if there is not CookiePath claim, then exist
                if (userIdentity.HasClaim(x => x.Type == ClaimTypes.CookiePath) == false)
                {
                    base.OnActionExecuting(filterContext);
                    return;
                }
            }

            string failedReason;
            var headers = new List<KeyValuePair<string, List<string>>>();
            foreach (var key in filterContext.HttpContext.Request.Headers.AllKeys)
            {
                if (headers.Any(x => x.Key == key))
                {
                    var found = headers.First(x => x.Key == key);
                    found.Value.Add(filterContext.HttpContext.Request.Headers[key]);
                }
                else
                {
                    headers.Add(new KeyValuePair<string, List<string>>(key, new List<string> { filterContext.HttpContext.Request.Headers[key] }));
                }
            }
            var cookie = filterContext.HttpContext.Request.Cookies[AngularAntiForgeryHelper.CsrfValidationCookieName];
            if (AngularAntiForgeryHelper.ValidateHeaders(
                headers.Select(x => new KeyValuePair<string, IEnumerable<string>>(x.Key, x.Value)).ToArray(),
                cookie == null ? "" : cookie.Value,
                out failedReason) == false)
            {
                var result = new HttpStatusCodeResult(HttpStatusCode.ExpectationFailed);
                filterContext.Result = result;
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}