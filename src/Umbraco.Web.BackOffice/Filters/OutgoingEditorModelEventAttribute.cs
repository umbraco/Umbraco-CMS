using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Filters;

/// <summary>
///     Used to emit outgoing editor model events
/// </summary>
internal sealed class OutgoingEditorModelEventAttribute : TypeFilterAttribute
{
    public OutgoingEditorModelEventAttribute() : base(typeof(OutgoingEditorModelEventFilter))
    {
    }


    private class OutgoingEditorModelEventFilter : IActionFilter
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;

        private readonly IEventAggregator _eventAggregator;

        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

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
            if (context.Result == null)
            {
                return;
            }

            IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
            IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
            if (currentUser == null)
            {
                return;
            }

            if (context.Result is ObjectResult objectContent)
            {
                // Support both batch (dictionary) and single results
                IEnumerable models;
                if (objectContent.Value is IDictionary modelDictionary)
                {
                    models = modelDictionary.Values;
                }
                else
                {
                    models = new[] {objectContent.Value};
                }

                foreach (var model in models)
                {
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
                        case IEnumerable<ContentTypeBasic> allowedChildren:
                            // Changing the Enumerable will generate a new instance, so we need to update the context result with the new content
                            var notification = new SendingAllowedChildrenNotification(allowedChildren, umbracoContext);
                            _eventAggregator.Publish(notification);
                            context.Result = new ObjectResult(notification.Children);
                            break;
                    }
                }
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
        }
    }
}
