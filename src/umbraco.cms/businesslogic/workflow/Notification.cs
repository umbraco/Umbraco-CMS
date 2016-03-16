using Umbraco.Core.Models;
using Umbraco.Core.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Models.Rdbms;
using umbraco.DataLayer;
using umbraco.interfaces;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace umbraco.cms.businesslogic.workflow
{
    //TODO: Update this to wrap new services/repo!

    /// <summary>
    /// Notifications are a part of the umbraco workflow.
    /// A notification is created every time an action on a node occurs and a umbraco user has subscribed to this specific action on this specific node.
    /// Notifications generates an email, which is send to the subscribing users.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Private constructor as this object should not be allowed to be created currently
        /// </summary>
        private Notification()
        {
        }

        public int NodeId { get; private set; }
        public int UserId { get; private set; }
        public char ActionId { get; private set; }

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        [Obsolete("Obsolete, For querying the database use the new UmbracoDatabase object ApplicationContext.Current.DatabaseContext.Database", false)]
        protected static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

        /// <summary>
        /// Sends the notifications for the specified user regarding the specified node and action.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="user">The user.</param>
        /// <param name="action">The action.</param>
        public static void GetNotifications(CMSNode node, IUser user, IAction action)
        {

            int totalUsers;
            var allUsers = ApplicationContext.Current.Services.UserService.GetAll(0, int.MaxValue, out totalUsers);
            foreach (var u in allUsers)
            {
                try
                {
                    if (u.IsApproved)
                    {
                        //TODO: N+1 !!!!
                        var notifications = ApplicationContext.Current.Services.NotificationService.GetUserNotifications(u, node.Path);
                        
                        if (notifications.Any(x => x.Action.InvariantEquals(action.Letter.ToString(CultureInfo.InvariantCulture))))
                        {
                            LogHelper.Debug<Notification>(string.Format("Notification about {0} sent to {1} ({2})",
                            ApplicationContext.Current.Services.TextService.Localize("actions/" + action.Alias, u.GetUserCulture(ApplicationContext.Current.Services.TextService)),
                            u.Name, u.Email));
                            SendNotification(user, u, (Document)node, action);
                        }
                    }
                }
                catch (Exception notifyExp)
                {
					LogHelper.Error<Notification>("Error in notification", notifyExp);
                }
            }
        }

        //TODO: Include update with html mail notification and document contents
        private static void SendNotification(IUser performingUser, IUser mailingUser, Document documentObject, IAction action)
        {
            var nService = ApplicationContext.Current.Services.NotificationService;
            var pUser = ApplicationContext.Current.Services.UserService.GetUserById(performingUser.Id);

            nService.SendNotifications(
                pUser, documentObject.ContentEntity, action.Letter.ToString(CultureInfo.InvariantCulture), ApplicationContext.Current.Services.TextService.Localize(action.Alias), 
                new HttpContextWrapper(HttpContext.Current),
                (user, strings) => ApplicationContext.Current.Services.TextService.Localize("notifications/mailSubject", mailingUser.GetUserCulture(ApplicationContext.Current.Services.TextService), strings),
                (user, strings) => UmbracoConfig.For.UmbracoSettings().Content.DisableHtmlEmail
                    ? ApplicationContext.Current.Services.TextService.Localize("notifications/mailBody", mailingUser.GetUserCulture(ApplicationContext.Current.Services.TextService), strings)
                    : ApplicationContext.Current.Services.TextService.Localize("notifications/mailBodyHtml", mailingUser.GetUserCulture(ApplicationContext.Current.Services.TextService), strings));
        }

        /// <summary>
        /// Returns the notifications for a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static IEnumerable<Notification> GetUserNotifications(IUser user)
        {
            var items = new List<Notification>();
            var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<User2NodeNotifyDto>(
                "WHERE userId = @UserId ORDER BY nodeId", new { UserId = user.Id });

            foreach (var dto in dtos)
            {
                items.Add(new Notification
                          {
                              NodeId = dto.NodeId,
                              ActionId = Convert.ToChar(dto.Action),
                              UserId = dto.UserId
                          });
            }

            return items;
        }

        /// <summary>
        /// Returns the notifications for a node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<Notification> GetNodeNotifications(CMSNode node)
        {
            var items = new List<Notification>();
            var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<User2NodeNotifyDto>(
                "WHERE userId = @UserId ORDER BY nodeId", new { nodeId = node.Id });

            foreach (var dto in dtos)
            {
                items.Add(new Notification
                {
                    NodeId = dto.NodeId,
                    ActionId = Convert.ToChar(dto.Action),
                    UserId = dto.UserId
                });
            }
            return items;
        }

        /// <summary>
        /// Deletes notifications by node
        /// </summary>
        /// <param name="node"></param>
        public static void DeleteNotifications(CMSNode node)
        {
            // delete all settings on the node for this node id
            ApplicationContext.Current.DatabaseContext.Database.Delete<User2NodeNotifyDto>("WHERE nodeId = @nodeId",
                new {nodeId = node.Id});
        }

        /// <summary>
        /// Delete notifications by user
        /// </summary>
        /// <param name="user"></param>
        public static void DeleteNotifications(IUser user)
        {
            // delete all settings on the node for this node id
            ApplicationContext.Current.DatabaseContext.Database.Delete<User2NodeNotifyDto>("WHERE userId = @userId",
                new { userId = user.Id });
        }

        /// <summary>
        /// Delete notifications by user and node
        /// </summary>
        /// <param name="user"></param>
        /// <param name="node"></param>
        public static void DeleteNotifications(IUser user, CMSNode node)
        {
            // delete all settings on the node for this user
            ApplicationContext.Current.DatabaseContext.Database.Delete<User2NodeNotifyDto>(
                "WHERE userId = @userId AND nodeId = @nodeId", new {userId = user.Id, nodeId = node.Id});
        }

        /// <summary>
        /// Creates a new notification
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="node">The node.</param>
        /// <param name="actionLetter">The action letter.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void MakeNew(IUser user, CMSNode node, char actionLetter)
        {
            bool exists = ApplicationContext.Current.DatabaseContext.Database.ExecuteScalar<int>(
                "SELECT COUNT(userId) FROM umbracoUser2nodeNotify WHERE userId = @userId AND nodeId = @nodeId AND action = @action", 
                new { userId = user.Id, nodeId = node.Id, action = actionLetter.ToString()}) > 0;

            if (exists == false)
            {
                ApplicationContext.Current.DatabaseContext.Database.Insert(new User2NodeNotifyDto
                                                                           {
                                                                               Action = actionLetter.ToString(),
                                                                               NodeId = node.Id,
                                                                               UserId = user.Id
                                                                           });
            }
        }

        /// <summary>
        /// Updates the notifications.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="node">The node.</param>
        /// <param name="notifications">The notifications.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void UpdateNotifications(IUser user, CMSNode node, string notifications)
        {
            // delete all settings on the node for this user
            DeleteNotifications(user, node);

            // Loop through the permissions and create them
            foreach (char c in notifications)
                MakeNew(user, node, c);
        }

    }
}