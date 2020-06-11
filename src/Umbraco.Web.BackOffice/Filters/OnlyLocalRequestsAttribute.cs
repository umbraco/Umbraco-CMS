using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Web.Common.Extensions;

namespace Umbraco.Web.BackOffice.Filters
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
