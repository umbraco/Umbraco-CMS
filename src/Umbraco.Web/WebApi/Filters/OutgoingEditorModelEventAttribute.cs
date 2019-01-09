using System;
using System.Net.Http;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Used to emit outgoing editor model events
    /// </summary>
    internal sealed class OutgoingEditorModelEventAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext.Response == null) return;

            var user = UmbracoContext.Current.Security.CurrentUser;
            if (user == null) return;

            if (actionExecutedContext.Response.Content is ObjectContent objectContent)
            {
                var model = objectContent.Value;

                if (model != null)
                {
                    var args = new EditorModelEventArgs(
                        model,
                        UmbracoContext.Current);
                    EditorModelEventManager.EmitEvent(actionExecutedContext, args);
                    objectContent.Value = args.Model;
                }
            }

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
