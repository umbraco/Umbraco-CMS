using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.DependencyInjection;
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
        private readonly IUmbracoMapper _mapper;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;

        [ActivatorUtilitiesConstructor]
        public OutgoingEditorModelEventFilter(
            IUmbracoContextAccessor umbracoContextAccessor,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IEventAggregator eventAggregator,
            IUmbracoMapper mapper)
        {
            _umbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor ?? throw new ArgumentNullException(nameof(backOfficeSecurityAccessor));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _mapper = mapper;
        }

        [Obsolete("Please use constructor that takes an IUmbracoMapper, scheduled for removal in V12")]
        public OutgoingEditorModelEventFilter(
            IUmbracoContextAccessor umbracoContextAccessor,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IEventAggregator eventAggregator)
        : this(
            umbracoContextAccessor,
            backOfficeSecurityAccessor,
            eventAggregator,
            StaticServiceProvider.Instance.GetRequiredService<IUmbracoMapper>())
        {
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
                    models = new[] { objectContent.Value };
                }

                foreach (var model in models)
                {
                    switch (model)
                    {
                        case ContentItemDisplay content:
                            _eventAggregator.Publish(new SendingContentNotification(content, umbracoContext));
                            break;

                            case ContentItemDisplayWithSchedule contentWithSchedule:
                            // This is a bit weird, since ContentItemDisplayWithSchedule was introduced later,
                            // the SendingContentNotification only accepts ContentItemDisplay,
                            // which means we have to map it to this before sending the notification.
                            ContentItemDisplay? display = _mapper.Map<ContentItemDisplayWithSchedule, ContentItemDisplay>(contentWithSchedule);
                            if (display is null)
                            {
                                // This will never happen.
                                break;
                            }

                            // Now that the display is mapped to the non-schedule one we can publish the notification.
                            _eventAggregator.Publish(new SendingContentNotification(display, umbracoContext));

                            // We want the changes the handler makes to take effect.
                            // So we have to map these changes back to the existing ContentItemWithSchedule.
                            // To avoid losing the schedule information we add the old variants to context.
                            _mapper.Map(display, contentWithSchedule, mapperContext => mapperContext.Items[nameof(contentWithSchedule.Variants)] = contentWithSchedule.Variants);
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
