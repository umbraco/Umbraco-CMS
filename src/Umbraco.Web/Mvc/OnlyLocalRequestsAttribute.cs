
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Umbraco.Web.Mvc
{
    public class OnlyLocalRequestsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (!actionContext.Request.IsLocal())
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
        }
    }
}
