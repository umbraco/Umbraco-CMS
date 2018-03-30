using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// An action filter used to do basic validation against the model and return a result
    /// straight away if it fails.
    /// </summary>
    internal sealed class ValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var m = actionContext.GetType().GetProperty("Request");
            if (m == null) throw new Exception("property"); // does not throw
            var x = m.GetMethod;
            if (x == null) throw new Exception("method"); // does not throw
            var v1 = m.GetValue(actionContext);
            if (v1 == null) throw new Exception("value1"); // does not throw
            var v2 = x.Invoke(actionContext, Array.Empty<object>());
            if (v2 == null) throw new Exception("value2"); // does not throw
            Thread.Sleep(10*1000);

            // throws get_Request method not found ?!
            var r = actionContext.Request;

            // this would show we are using our own, copied into asp.net temp
            //throw new Exception(actionContext.GetType().Assembly.Location);

            var modelState = actionContext.ModelState;

            if (!modelState.IsValid)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, modelState);
            }
        }
    }

}
