using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Ensures controllers have detailed error messages even when debug mode is off
    /// </summary>
    public class EnableDetailedErrorsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            actionContext.ControllerContext.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        }
    }
}