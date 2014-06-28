using System;
using System.Web.Mvc;

namespace Umbraco.Web.Mvc
{
    public class PreRenderViewActionFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var umbController = filterContext.Controller as Controller;
            if (umbController == null)
            {
                return;
            }

            var result = filterContext.Result as ViewResultBase;
            if (result == null)
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

            base.OnActionExecuted(filterContext);
        }


        public static event EventHandler<ActionExecutedEventArgs> ActionExecuted;

        private static void OnActionExecuted(ActionExecutedEventArgs e)
        {
            EventHandler<ActionExecutedEventArgs> handler = ActionExecuted;
            if (handler != null) handler(null, e);
        }
    }
}