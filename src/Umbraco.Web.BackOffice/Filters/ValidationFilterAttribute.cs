using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Cms.Web.BackOffice.Filters
{
    /// <summary>
    /// An action filter used to do basic validation against the model and return a result
    /// straight away if it fails.
    /// </summary>
    internal sealed class ValidationFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var modelState = context.ModelState;
            if (!modelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(modelState);
            }
        }
    }
}
