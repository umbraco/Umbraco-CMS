using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core.Events;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.BackOffice.Filters
{
    /// <summary>
    /// Automatically checks if any request is a non-GET and if the
    /// resulting message is INotificationModel in which case it will append any Event Messages
    /// currently in the request.
    /// </summary>
    internal sealed class AppendCurrentEventMessagesAttribute : TypeFilterAttribute
    {
        public AppendCurrentEventMessagesAttribute() : base(typeof(AppendCurrentEventMessagesFilter))
        {
        }

        private class AppendCurrentEventMessagesFilter : IActionFilter
        {
            private readonly IUmbracoContextAccessor _umbracoContextAccessor;
            private readonly IEventMessagesFactory _eventMessagesFactory;

            public AppendCurrentEventMessagesFilter(IUmbracoContextAccessor umbracoContextAccessor, IEventMessagesFactory eventMessagesFactory)
            {
                _umbracoContextAccessor = umbracoContextAccessor;
                _eventMessagesFactory = eventMessagesFactory;
            }

            public void OnActionExecuted(ActionExecutedContext context)
            {
                if (context.HttpContext.Response == null) return;
                if (context.HttpContext.Request.Method.Equals(HttpMethod.Get.ToString(), StringComparison.InvariantCultureIgnoreCase)) return;
                var umbracoContext = _umbracoContextAccessor.UmbracoContext;
                if (umbracoContext == null) return;

                if (!(context.Result is ObjectResult obj)) return;

                var notifications = obj.Value as INotificationModel;
                if (notifications == null) return;

                var msgs = _eventMessagesFactory.GetOrDefault();
                if (msgs == null) return;

                foreach (var eventMessage in msgs.GetAll())
                {
                    NotificationStyle msgType;
                    switch (eventMessage.MessageType)
                    {
                        case EventMessageType.Default:
                            msgType = NotificationStyle.Save;
                            break;
                        case EventMessageType.Info:
                            msgType = NotificationStyle.Info;
                            break;
                        case EventMessageType.Error:
                            msgType = NotificationStyle.Error;
                            break;
                        case EventMessageType.Success:
                            msgType = NotificationStyle.Success;
                            break;
                        case EventMessageType.Warning:
                            msgType = NotificationStyle.Warning;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    notifications.Notifications.Add(new BackOfficeNotification
                    {
                        Message = eventMessage.Message,
                        Header = eventMessage.Category,
                        NotificationType = msgType
                    });
                }
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
            }
        }
    }
}
