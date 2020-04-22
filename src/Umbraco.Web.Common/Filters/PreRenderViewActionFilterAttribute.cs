using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Web.Common.Events;

namespace Umbraco.Web.Common.Filters
{
    public class PreRenderViewActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (!(context.Controller is Controller umbController) || !(context.Result is ViewResult result))
            {
                return;
            }

            var model = result.Model;
            if (model == null)
            {
                return;
            }

            var args = new ActionExecutedEventArgs(umbController, model);
            OnActionExecuted(args);

            if (args.Model != model)
            {
                result.ViewData.Model = args.Model; 
            }

            base.OnActionExecuted(context);
        }


        public static event EventHandler<ActionExecutedEventArgs> ActionExecuted;

        private static void OnActionExecuted(ActionExecutedEventArgs e)
        {
            var handler = ActionExecuted;
            handler?.Invoke(null, e);
        }
    }
}
