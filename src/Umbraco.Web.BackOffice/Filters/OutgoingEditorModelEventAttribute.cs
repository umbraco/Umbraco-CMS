using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Editors;
using Umbraco.Web.Security;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Used to emit outgoing editor model events
    /// </summary>
    internal sealed class OutgoingEditorModelEventAttribute : ActionFilterAttribute
    {
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IWebSecurityAccessor _webSecurityAccessor;

        public OutgoingEditorModelEventAttribute(IUmbracoContextAccessor umbracoContextAccessor, IWebSecurityAccessor webSecurityAccessor)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _webSecurityAccessor = webSecurityAccessor ?? throw new ArgumentNullException(nameof(webSecurityAccessor));
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Result == null) return;

            var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            var user = _webSecurityAccessor.WebSecurity.CurrentUser;
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
