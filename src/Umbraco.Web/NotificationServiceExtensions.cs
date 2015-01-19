using System;
using System.Globalization;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using umbraco;
using umbraco.BusinessLogic.Actions;
using umbraco.interfaces;

namespace Umbraco.Web
{
    //NOTE: all of these require an UmbracoContext because currently to send the notifications we need an HttpContext, this is based on legacy code
    // for which probably requires updating so that these can be sent outside of the http context.

    internal static class NotificationServiceExtensions
    {
        internal static void SendNotification(this INotificationService service, IUmbracoEntity entity, IAction action, ApplicationContext applicationContext)
        {
            if (UmbracoContext.Current == null)
            {
                LogHelper.Warn(typeof(NotificationServiceExtensions), "Cannot send notifications, there is no current UmbracoContext");
                return;
            }
            service.SendNotification(entity, action, UmbracoContext.Current);
        }

        internal static void SendNotification(this INotificationService service, IUmbracoEntity entity, IAction action, UmbracoContext umbracoContext)
        {
            if (umbracoContext == null)
            {
                LogHelper.Warn(typeof(NotificationServiceExtensions), "Cannot send notifications, there is no current UmbracoContext");
                return;
            }
            service.SendNotification(entity, action, umbracoContext, umbracoContext.Application);
        }

        internal static void SendNotification(this INotificationService service, IUmbracoEntity entity, IAction action, UmbracoContext umbracoContext, ApplicationContext applicationContext)
        {
            if (umbracoContext == null)
            {
                LogHelper.Warn(typeof(NotificationServiceExtensions), "Cannot send notifications, there is no current UmbracoContext");
                return;
            }

            var user = umbracoContext.Security.CurrentUser;

            //if there is no current user, then use the admin 
            if (user == null)
            {
                LogHelper.Debug(typeof(NotificationServiceExtensions), "There is no current Umbraco user logged in, the notifications will be sent from the administrator");
                user = applicationContext.Services.UserService.GetUserById(0);
                if (user == null)
                {
                    LogHelper.Warn(typeof(NotificationServiceExtensions), "Noticiations can not be sent, no admin user with id 0 could be resolved");
                    return;
                }
            }
            service.SendNotification(user, entity, action, umbracoContext, applicationContext);
        }

        internal static void SendNotification(this INotificationService service, IUser sender, IUmbracoEntity entity, IAction action, UmbracoContext umbracoContext, ApplicationContext applicationContext)
        {
            if (sender == null) throw new ArgumentNullException("sender");
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            if (applicationContext == null) throw new ArgumentNullException("applicationContext");

            
            applicationContext.Services.NotificationService.SendNotifications(
                sender,
                entity,
                action.Letter.ToString(CultureInfo.InvariantCulture),
                ui.Text("actions", action.Alias),
                umbracoContext.HttpContext,
                (mailingUser, strings) => ui.Text("notifications", "mailSubject", strings, mailingUser),
                (mailingUser, strings) => UmbracoConfig.For.UmbracoSettings().Content.DisableHtmlEmail
                                              ? ui.Text("notifications", "mailBody", strings, mailingUser)
                                              : ui.Text("notifications", "mailBodyHtml", strings, mailingUser));
        }
    }
}