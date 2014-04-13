using System;
using System.Globalization;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Services;
using umbraco;
using umbraco.BusinessLogic.Actions;
using umbraco.interfaces;

namespace Umbraco.Web
{
    internal static class NotificationServiceExtensions
    {
        internal static void SendNotification(this INotificationService service, IUmbracoEntity entity, IAction action, ApplicationContext applicationContext)
        {
            if (global::Umbraco.Web.UmbracoContext.Current == null) return;
            service.SendNotification(entity, action, global::Umbraco.Web.UmbracoContext.Current);
        }

        internal static void SendNotification(this INotificationService service, IUmbracoEntity entity, IAction action, UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) return;
            service.SendNotification(entity, action, umbracoContext, umbracoContext.Application);
        }

        internal static void SendNotification(this INotificationService service, IUmbracoEntity entity, IAction action, UmbracoContext umbracoContext, ApplicationContext applicationContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            if (applicationContext == null) throw new ArgumentNullException("applicationContext");

            var user = umbracoContext.Security.CurrentUser;
            applicationContext.Services.NotificationService.SendNotifications(
                user,
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