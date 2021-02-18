using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Filters
{
    public class OnlyLocalRequestsAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.IsLocal())
            {
                context.Result = new NotFoundResult();
            }
        }
    }
}
