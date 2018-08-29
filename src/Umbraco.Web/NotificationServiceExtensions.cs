using System;
using System.Globalization;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Core.Models;
using Umbraco.Web._Legacy.Actions;
using System.Collections.Generic;
using Umbraco.Core.Models.Entities;
using Umbraco.Web.Composing;

namespace Umbraco.Web
{
    // TODO: all of these require an UmbracoContext because currently to send the notifications we need an HttpContext, this is based on legacy code
    // for which probably requires updating so that these can be sent outside of the http context.

    internal static class NotificationServiceExtensions
    {
        internal static void SendNotification(this INotificationService service, IUmbracoEntity entity, IAction action)
        {
            if (Current.UmbracoContext == null)
            {
                Current.Logger.Warn(typeof(NotificationServiceExtensions), "Cannot send notifications, there is no current UmbracoContext");
                return;
            }
            service.SendNotification(entity, action, Current.UmbracoContext, Current.Services.TextService, UmbracoConfig.For.GlobalSettings());
        }

        internal static void SendNotification(this INotificationService service, IEnumerable<IUmbracoEntity> entities, IAction action)
        {
            if (Current.UmbracoContext == null)
            {
                Current.Logger.Warn(typeof(NotificationServiceExtensions), "Cannot send notifications, there is no current UmbracoContext");
                return;
            }
            service.SendNotification(entities, action, Current.UmbracoContext, Current.Services.TextService, UmbracoConfig.For.GlobalSettings());
        }

        //internal static void SendNotification(this INotificationService service, IUmbracoEntity entity, IAction action, UmbracoContext umbracoContext)
        //{
        //    if (umbracoContext == null)
        //    {
        //        Current.Logger.Warn(typeof(NotificationServiceExtensions), "Cannot send notifications, there is no current UmbracoContext");
        //        return;
        //    }
        //    service.SendNotification(entity, action, umbracoContext);
        //}

        //internal static void SendNotification(this INotificationService service, IEnumerable<IUmbracoEntity> entities, IAction action, UmbracoContext umbracoContext)
        //{
        //    if (umbracoContext == null)
        //    {
        //        Current.Logger.Warn(typeof(NotificationServiceExtensions), "Cannot send notifications, there is no current UmbracoContext");
        //        return;
        //    }
        //    service.SendNotification(entities, action, umbracoContext);
        //}

        internal static void SendNotification(this INotificationService service, IUmbracoEntity entity, IAction action, UmbracoContext umbracoContext, ILocalizedTextService textService, IGlobalSettings globalSettings)
        {
            if (umbracoContext == null)
            {
                Current.Logger.Warn(typeof(NotificationServiceExtensions), "Cannot send notifications, there is no current UmbracoContext");
                return;
            }

            var user = umbracoContext.Security.CurrentUser;
            var userService = Current.Services.UserService; // fixme inject

            //if there is no current user, then use the admin
            if (user == null)
            {
                Current.Logger.Debug(typeof(NotificationServiceExtensions), "There is no current Umbraco user logged in, the notifications will be sent from the administrator");
                user = userService.GetUserById(Constants.Security.SuperUserId);
                if (user == null)
                {
                    Current.Logger.Warn(typeof(NotificationServiceExtensions), "Noticiations can not be sent, no admin user with id {SuperUserId} could be resolved", Constants.Security.SuperUserId);
                    return;
                }
            }
            service.SendNotification(user, entity, action, umbracoContext, textService, globalSettings);
        }

        internal static void SendNotification(this INotificationService service, IEnumerable<IUmbracoEntity> entities, IAction action, UmbracoContext umbracoContext, ILocalizedTextService textService, IGlobalSettings globalSettings)
        {
            if (umbracoContext == null)
            {
                Current.Logger.Warn(typeof(NotificationServiceExtensions), "Cannot send notifications, there is no current UmbracoContext");
                return;
            }

            var user = umbracoContext.Security.CurrentUser;
            var userService = Current.Services.UserService; // fixme inject

            //if there is no current user, then use the admin
            if (user == null)
            {
                Current.Logger.Debug(typeof(NotificationServiceExtensions), "There is no current Umbraco user logged in, the notifications will be sent from the administrator");
                user = userService.GetUserById(Constants.Security.SuperUserId);
                if (user == null)
                {
                    Current.Logger.Warn(typeof(NotificationServiceExtensions), "Noticiations can not be sent, no admin user with id {SuperUserId} could be resolved", Constants.Security.SuperUserId);
                    return;
                }
            }
            service.SendNotification(user, entities, action, umbracoContext, textService, globalSettings);
        }

        internal static void SendNotification(this INotificationService service, IUser sender, IUmbracoEntity entity, IAction action, UmbracoContext umbracoContext, ILocalizedTextService textService, IGlobalSettings globalSettings)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));

            service.SendNotifications(
                sender,
                entity,
                action.Letter.ToString(CultureInfo.InvariantCulture),
                textService.Localize("actions", action.Alias),
                umbracoContext.HttpContext,
                (mailingUser, strings) => textService.Localize("notifications/mailSubject", mailingUser.GetUserCulture(textService, globalSettings), strings),
                (mailingUser, strings) => UmbracoConfig.For.UmbracoSettings().Content.DisableHtmlEmail
                                              ? textService.Localize("notifications/mailBody", mailingUser.GetUserCulture(textService, globalSettings), strings)
                                              : textService.Localize("notifications/mailBodyHtml", mailingUser.GetUserCulture(textService, globalSettings), strings));
        }

         internal static void SendNotification(this INotificationService service, IUser sender, IEnumerable<IUmbracoEntity> entities, IAction action, UmbracoContext umbracoContext, ILocalizedTextService textService, IGlobalSettings globalSettings)
        {
            if (sender == null) throw new ArgumentNullException(nameof(sender));
            if (umbracoContext == null) throw new ArgumentNullException(nameof(umbracoContext));

            service.SendNotifications(
                sender,
                entities,
                action.Letter.ToString(CultureInfo.InvariantCulture),
                textService.Localize("actions", action.Alias),
                umbracoContext.HttpContext,
                (mailingUser, strings) => textService.Localize("notifications/mailSubject", mailingUser.GetUserCulture(textService, globalSettings), strings),
                (mailingUser, strings) => UmbracoConfig.For.UmbracoSettings().Content.DisableHtmlEmail
                                              ? textService.Localize("notifications/mailBody", mailingUser.GetUserCulture(textService, globalSettings), strings)
                                              : textService.Localize("notifications/mailBodyHtml", mailingUser.GetUserCulture(textService, globalSettings), strings));
        }
    }
}
