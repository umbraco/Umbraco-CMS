using System.Collections;
using System.Net.Http;
using System.Web.Http.Filters;
using Umbraco.Web.Composing;
using Umbraco.Web.Editors;

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

            var user = Current.UmbracoContext.Security.CurrentUser;
            if (user == null) return;

            if (actionExecutedContext.Response.Content is ObjectContent objectContent)
            {
                var model = objectContent.Value;
                if (model != null)
                {
                    if (model is IDictionary modelDict)
                    {
                        foreach (var entity in modelDict)
                        {
                            if (entity is DictionaryEntry entry)
                            {
                                var args = CreateArgs(entry.Value);
                                EditorModelEventManager.EmitEvent(actionExecutedContext, args);
                                entry.Value = args.Model;
                            }
                        }
                    }
                    else
                    {
                        var args = CreateArgs(model);
                        EditorModelEventManager.EmitEvent(actionExecutedContext, args);
                        objectContent.Value = args.Model;
                    }
                }
            }

            base.OnActionExecuted(actionExecutedContext);
        }

        private EditorModelEventArgs CreateArgs(object model)
        {
            return new EditorModelEventArgs(
                model,
                Current.UmbracoContext);
        }
    }
}
