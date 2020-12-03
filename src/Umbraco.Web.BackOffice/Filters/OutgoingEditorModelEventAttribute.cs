using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;
using Umbraco.Core.Security;
using Umbraco.Web.Editors;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Used to emit outgoing editor model events
    /// </summary>
    internal sealed class OutgoingEditorModelEventAttribute : TypeFilterAttribute
    {
        public OutgoingEditorModelEventAttribute() : base(typeof(OutgoingEditorModelEventFilter))
        {
        }


        private class OutgoingEditorModelEventFilter : IActionFilter
        {

            private readonly IUmbracoContextAccessor _umbracoContextAccessor;

            private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

            public OutgoingEditorModelEventFilter(
                IUmbracoContextAccessor umbracoContextAccessor,
                IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
            {
                _umbracoContextAccessor = umbracoContextAccessor
                                          ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
                _backOfficeSecurityAccessor = backOfficeSecurityAccessor
                                              ?? throw new ArgumentNullException(nameof(backOfficeSecurityAccessor));
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                if (context.Result == null) return;

                var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
                var user = _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser;
                if (user == null) return;

                if (context.Result is ObjectResult objectContent)
                {
                    var model = objectContent.Value;

                    if (model != null)
                    {
                        var args = new EditorModelEventArgs(model, umbracoContext);
                        EditorModelEventManager.EmitEvent(context, args);
                        objectContent.Value = args.Model;
                    }
                }
            }
            
            public void OnActionExecuting(ActionExecutingContext context)
            {
            }
        }
    }
}
