using System.Web;
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
            if (HttpContext.Current?.IsDebuggingEnabled ?? false)
            {
                actionContext.ControllerContext.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            }
            else
            {
                actionContext.ControllerContext.Configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Default;
            }
        }
    }
}
