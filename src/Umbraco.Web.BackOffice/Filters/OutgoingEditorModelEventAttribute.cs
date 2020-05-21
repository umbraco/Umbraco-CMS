using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Web.Editors;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Used to emit outgoing editor model events
    /// </summary>
    internal sealed class OutgoingEditorModelEventAttribute : ActionFilterAttribute
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        public OutgoingEditorModelEventAttribute(IUmbracoContextAccessor umbracoContextAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result == null) return;

            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            var user = umbracoContext.Security.CurrentUser;
            if (user == null) return;

            if (context.Result is ObjectResult objectContent)
            {
                var model = objectContent.Value;

                if (model != null)
                {
                    var args = new EditorModelEventArgs(
                        model,
                        umbracoContext);
                    EditorModelEventManager.EmitEvent(context, args);
                    objectContent.Value = args.Model;
                }
            }

            base.OnActionExecuted(context);
        }

    }
}
