using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     An action filter used to do basic validation against the model and return a result
///     straight away if it fails.
/// </summary>
internal sealed class ValidationFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ModelStateDictionary modelState = context.ModelState;
        if (!modelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(modelState);
        }
    }
}
