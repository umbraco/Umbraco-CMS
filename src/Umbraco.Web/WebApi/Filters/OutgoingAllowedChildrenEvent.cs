using System;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Web.Composing;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.WebApi.Filters
    {
        /// <summary>
        /// Used to emit outgoing 'AllowedChildren' Event
        /// Enable developers to modify the list of available items to create based on custom logic
        /// </summary>
        internal sealed class OutgoingAllowedChildrenEventAttribute : ActionFilterAttribute
        {

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
            {
                if (actionExecutedContext.Response == null) return;
            var actionArguments = actionExecutedContext.ActionContext.ActionArguments;
            int? _contentId = -1;
            if (actionArguments.ContainsKey("contentId"))
            {
                _contentId = actionArguments["contentId"] as int?;
            }
            var user = Current.UmbracoContext.Security.CurrentUser;
                if (user == null) return;           

                if (actionExecutedContext.Response.Content is ObjectContent objectContent)
                {
                    var model = objectContent.Value;

                    if (model != null)
                    {
                        var args = new AllowedChildrenEventArgs(
                            model,
                            Current.UmbracoContext, _contentId);
                        AllowedChildrenEventManager.EmitEvent(actionExecutedContext, args);
                        objectContent.Value = args.Model;
                    }
                }

                base.OnActionExecuted(actionExecutedContext);
            }
        }
    }
