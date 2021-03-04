using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Editors;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Filters
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

            private readonly IEventAggregator _eventAggregator;

            public OutgoingEditorModelEventFilter(
                IUmbracoContextAccessor umbracoContextAccessor,
                IBackOfficeSecurityAccessor backOfficeSecurityAccessor, IEventAggregator eventAggregator)
            {
                _umbracoContextAccessor = umbracoContextAccessor
                                          ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
                _backOfficeSecurityAccessor = backOfficeSecurityAccessor
                                              ?? throw new ArgumentNullException(nameof(backOfficeSecurityAccessor));
                _eventAggregator = eventAggregator
                                   ?? throw new ArgumentNullException(nameof(eventAggregator));
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                if (context.Result == null) return;

                var umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
                var currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser;
                if (currentUser == null) return;

                if (context.Result is ObjectResult objectContent)
                {
                    var model = objectContent.Value;

                    switch (model)
                    {
                        case ContentItemDisplay content:
                            _eventAggregator.Publish(new SendingContentNotification(content, umbracoContext));
                            break;
                        case MediaItemDisplay media:
                            _eventAggregator.Publish(new SendingMediaNotification(media, umbracoContext));
                            break;
                        case MemberDisplay member:
                            _eventAggregator.Publish(new SendingMemberNotification(member, umbracoContext));
                            break;
                        case UserDisplay user:
                            _eventAggregator.Publish(new SendingUserNotification(user, umbracoContext));
                            break;
                        case IEnumerable<Tab<IDashboardSlim>> dashboards:
                            _eventAggregator.Publish(new SendingDashboardsNotification(dashboards, umbracoContext));
                            break;
                    }
                }
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
            }
        }
    }
}
