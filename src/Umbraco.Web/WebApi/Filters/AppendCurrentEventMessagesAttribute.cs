using System;
using System.Net.Http;
using System.Web.Http.Filters;
using Umbraco.Core.Events;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Automatically checks if any request is a non-GET and if the
    /// resulting message is INotificationModel in which case it will append any Event Messages
    /// currently in the request.
    /// </summary>
    internal sealed class AppendCurrentEventMessagesAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (context.Response == null) return;
            if (context.Request.Method == HttpMethod.Get) return;
            if (Current.UmbracoContext == null) return;

            var obj = context.Response.Content as ObjectContent;
            if (obj == null) return;

            var notifications = obj.Value as INotificationModel;
            if (notifications == null) return;

            var msgs = Current.EventMessages;
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

                notifications.Notifications.Add(new Notification
                {
                    Message = eventMessage.Message,
                    Header = eventMessage.Category,
                    NotificationType = msgType
                });
            }
        }
    }
}
