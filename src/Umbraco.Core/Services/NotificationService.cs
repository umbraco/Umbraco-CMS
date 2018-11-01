using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;
        private readonly IUserService _userService;
        private readonly IContentService _contentService;
        private readonly ILogger _logger;

        public NotificationService(IDatabaseUnitOfWorkProvider provider, IUserService userService, IContentService contentService, ILogger logger)
        {
            if (provider == null) throw new ArgumentNullException("provider");
            if (userService == null) throw new ArgumentNullException("userService");
            if (contentService == null) throw new ArgumentNullException("contentService");
            if (logger == null) throw new ArgumentNullException("logger");
            _uowProvider = provider;
            _userService = userService;
            _contentService = contentService;
            _logger = logger;
        }

        /// <summary>
        /// Sends the notifications for the specified user regarding the specified node and action.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="operatingUser"></param>
        /// <param name="action"></param>
        /// <param name="actionName"></param>
        /// <param name="http"></param>
        /// <param name="createSubject"></param>
        /// <param name="createBody"></param>
        /// <remarks>
        /// Currently this will only work for Content entities!
        /// </remarks>
        public void SendNotifications(IUser operatingUser, IUmbracoEntity entity, string action, string actionName, HttpContextBase http,
            Func<IUser, string[], string> createSubject,
            Func<IUser, string[], string> createBody)
        {
            if (entity is IContent == false)
                throw new NotSupportedException();

            SendNotifications(operatingUser, new[] { (IContent)entity }, action, actionName, http, createSubject, createBody);
        }

        /// <summary>
        /// Gets the previous version to the latest version of the content item if there is one
        /// </summary>
        /// <param name="contentId"></param>
        /// <returns></returns>
        private IContentBase GetPreviousVersion(int contentId)
        {
            // Regarding this: http://issues.umbraco.org/issue/U4-5180
            // we know they are descending from the service so we know that newest is first
            // we are only selecting the top 2 rows since that is all we need
            var allVersions = _contentService.GetVersionIds(contentId, 2).ToList();
            var prevVersionIndex = allVersions.Count > 1 ? 1 : 0;
            return _contentService.GetByVersion(allVersions[prevVersionIndex]);
        }

        /// <summary>
        /// Sends the notifications for the specified user regarding the specified node and action.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="operatingUser"></param>
        /// <param name="action"></param>
        /// <param name="actionName"></param>
        /// <param name="http"></param>
        /// <param name="createSubject"></param>
        /// <param name="createBody"></param>
        /// <remarks>
        /// Currently this will only work for Content entities!
        /// </remarks>
        public void SendNotifications(IUser operatingUser, IEnumerable<IUmbracoEntity> entities, string action, string actionName, HttpContextBase http,
            Func<IUser, string[], string> createSubject,
            Func<IUser, string[], string> createBody)
        {
            if (entities is IEnumerable<IContent> == false)
                throw new NotSupportedException();

            var entitiesL = entities as List<IContent> ?? entities.Cast<IContent>().ToList();

            //exit if there are no entities
            if (entitiesL.Count == 0) return;

            //put all entity's paths into a list with the same indicies
            var paths = entitiesL.Select(x => x.Path.Split(',').Select(int.Parse).ToArray()).ToArray();

            // lazily get versions
            var prevVersionDictionary = new Dictionary<int, IContentBase>();

            // do not load *all* users in memory at once
            // do not load notifications *per user* (N+1 select)
            // cannot load users & notifications in 1 query (combination btw User2AppDto and User2NodeNotifyDto)
            // => get batches of users, get all their notifications in 1 query
            // re. users:
            //  users being (dis)approved = not an issue, filtered in memory not in SQL
            //  users being modified or created = not an issue, ordering by ID, as long as we don't *insert* low IDs
            //  users being deleted = not an issue for GetNextUsers
            var id = 0;
            const int pagesz = 400; // load batches of 400 users
            do
            {
                // users are returned ordered by id, notifications are returned ordered by user id
                var users = ((UserService)_userService).GetNextUsers(id, pagesz).Where(x => x.IsApproved).ToList();
                var notifications = GetUsersNotifications(users.Select(x => x.Id), action, Enumerable.Empty<int>(), Constants.ObjectTypes.DocumentGuid).ToList();
                if (notifications.Count == 0) break;

                var i = 0;
                foreach (var user in users)
                {
                    // continue if there's no notification for this user
                    if (notifications[i].UserId != user.Id) continue; // next user

                    for (var j = 0; j < entitiesL.Count; j++)
                    {
                        var content = entitiesL[j];
                        var path = paths[j];
                        
                        // test if the notification applies to the path ie to this entity
                        if (path.Contains(notifications[i].EntityId) == false) continue; // next entity
                        
                        if (prevVersionDictionary.ContainsKey(content.Id) == false)
                        {
                            prevVersionDictionary[content.Id] = GetPreviousVersion(content.Id);
                        }
                        
                        // queue notification
                        var req = CreateNotificationRequest(operatingUser, user, content, prevVersionDictionary[content.Id], actionName, http, createSubject, createBody);
                        Enqueue(req);
                    }

                    // skip other notifications for this user, essentially this means moving i to the next index of notifications
                    // for the next user.
                    do
                    {
                        i++;
                    } while (i < notifications.Count && notifications[i].UserId == user.Id);
                    
                    if (i >= notifications.Count) break; // break if no more notifications
                }

                // load more users if any
                id = users.Count == pagesz ? users.Last().Id + 1 : -1;

            } while (id > 0);
        }

        private IEnumerable<Notification> GetUsersNotifications(IEnumerable<int> userIds, string action, IEnumerable<int> nodeIds, Guid objectType)
        {
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                var repository = new NotificationsRepository(uow);
                var ret = repository.GetUsersNotifications(userIds, action, nodeIds, objectType);
                uow.Commit();
                return ret;
            }
        }

        /// <summary>
        /// Gets the notifications for the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IEnumerable<Notification> GetUserNotifications(IUser user)
        {
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                var repository = new NotificationsRepository(uow);
                var ret = repository.GetUserNotifications(user);
                uow.Commit();
                return ret;
            }
        }

        /// <summary>
        /// Gets the notifications for the user based on the specified node path
        /// </summary>
        /// <param name="user"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <remarks>
        /// Notifications are inherited from the parent so any child node will also have notifications assigned based on it's parent (ancestors)
        /// </remarks>
        public IEnumerable<Notification> GetUserNotifications(IUser user, string path)
        {
            var userNotifications = GetUserNotifications(user);
            return FilterUserNotificationsByPath(userNotifications, path);
        }

        /// <summary>
        /// Filters a userNotifications collection by a path
        /// </summary>
        /// <param name="userNotifications"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public IEnumerable<Notification> FilterUserNotificationsByPath(IEnumerable<Notification> userNotifications, string path)
        {
            var pathParts = path.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
            return userNotifications.Where(r => pathParts.InvariantContains(r.EntityId.ToString(CultureInfo.InvariantCulture))).ToList();
        }

        /// <summary>
        /// Deletes notifications by entity
        /// </summary>
        /// <param name="entity"></param>
        public IEnumerable<Notification> GetEntityNotifications(IEntity entity)
        {
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                var repository = new NotificationsRepository(uow);
                var ret= repository.GetEntityNotifications(entity);
                uow.Commit();
                return ret;
            }
        }

        /// <summary>
        /// Deletes notifications by entity
        /// </summary>
        /// <param name="entity"></param>
        public void DeleteNotifications(IEntity entity)
        {
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                var repository = new NotificationsRepository(uow);
                repository.DeleteNotifications(entity);
                uow.Commit();
            }
        }

        /// <summary>
        /// Deletes notifications by user
        /// </summary>
        /// <param name="user"></param>
        public void DeleteNotifications(IUser user)
        {
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                var repository = new NotificationsRepository(uow);
                repository.DeleteNotifications(user);
                uow.Commit();
            }
        }

        /// <summary>
        /// Delete notifications by user and entity
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entity"></param>
        public void DeleteNotifications(IUser user, IEntity entity)
        {
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                var repository = new NotificationsRepository(uow);
                repository.DeleteNotifications(user, entity);
                uow.Commit();
            }
        }

        /// <summary>
        /// Creates a new notification
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entity"></param>
        /// <param name="action">The action letter - note: this is a string for future compatibility</param>
        /// <returns></returns>
        public Notification CreateNotification(IUser user, IEntity entity, string action)
        {
            using (var uow = _uowProvider.GetUnitOfWork())
            {
                var repository = new NotificationsRepository(uow);
                var ret = repository.CreateNotification(user, entity, action);
                uow.Commit();
                return ret;
            }
        }

        #region private methods

        /// <summary>
        /// Sends the notification
        /// </summary>
        /// <param name="performingUser"></param>
        /// <param name="mailingUser"></param>
        /// <param name="content"></param>
        /// <param name="oldDoc"></param>
        /// <param name="actionName">The action readable name - currently an action is just a single letter, this is the name associated with the letter </param>
        /// <param name="http"></param>
        /// <param name="createSubject">Callback to create the mail subject</param>
        /// <param name="createBody">Callback to create the mail body</param>
        private NotificationRequest CreateNotificationRequest(IUser performingUser, IUser mailingUser, IContentBase content, IContentBase oldDoc,            
            string actionName, HttpContextBase http,
            Func<IUser, string[], string> createSubject,
            Func<IUser, string[], string> createBody)
        {
            if (performingUser == null) throw new ArgumentNullException("performingUser");
            if (mailingUser == null) throw new ArgumentNullException("mailingUser");
            if (content == null) throw new ArgumentNullException("content");
            if (http == null) throw new ArgumentNullException("http");
            if (createSubject == null) throw new ArgumentNullException("createSubject");
            if (createBody == null) throw new ArgumentNullException("createBody");            
            
            // build summary
            var summary = new StringBuilder();
            var props = content.Properties.ToArray();
            foreach (var p in props)
            {
                var newText = p.Value != null ? p.Value.ToString() : "";
                var oldText = newText;

                // check if something was changed and display the changes otherwise display the fields
                if (oldDoc.Properties.Contains(p.PropertyType.Alias))
                {
                    var oldProperty = oldDoc.Properties[p.PropertyType.Alias];
                    oldText = oldProperty.Value != null ? oldProperty.Value.ToString() : "";

                    // replace html with char equivalent
                    ReplaceHtmlSymbols(ref oldText);
                    ReplaceHtmlSymbols(ref newText);
                }


                // make sure to only highlight changes done using TinyMCE editor... other changes will be displayed using default summary
                // TODO: We should probably allow more than just tinymce??
                if ((p.PropertyType.PropertyEditorAlias == Constants.PropertyEditors.TinyMCEAlias)
                    && string.CompareOrdinal(oldText, newText) != 0)
                {
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'> Note: </th>");
                    summary.Append(
                        "<td style='text-align: left; vertical-align: top;'> <span style='background-color:red;'>Red for deleted characters</span>&nbsp;<span style='background-color:yellow;'>Yellow for inserted characters</span></td>");
                    summary.Append("</tr>");
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'> New ");
                    summary.Append(p.PropertyType.Name);
                    summary.Append("</th>");
                    summary.Append("<td style='text-align: left; vertical-align: top;'>");
                    summary.Append(ReplaceLinks(CompareText(oldText, newText, true, false, "<span style='background-color:yellow;'>", string.Empty), http.Request));
                    summary.Append("</td>");
                    summary.Append("</tr>");
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'> Old ");
                    summary.Append(p.PropertyType.Name);
                    summary.Append("</th>");
                    summary.Append("<td style='text-align: left; vertical-align: top;'>");
                    summary.Append(ReplaceLinks(CompareText(newText, oldText, true, false, "<span style='background-color:red;'>", string.Empty), http.Request));
                    summary.Append("</td>");
                    summary.Append("</tr>");
                }
                else
                {
                    summary.Append("<tr>");
                    summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;'>");
                    summary.Append(p.PropertyType.Name);
                    summary.Append("</th>");
                    summary.Append("<td style='text-align: left; vertical-align: top;'>");
                    summary.Append(newText);
                    summary.Append("</td>");
                    summary.Append("</tr>");
                }
                summary.Append(
                    "<tr><td colspan=\"2\" style=\"border-bottom: 1px solid #CCC; font-size: 2px;\">&nbsp;</td></tr>");
            }

            string protocol = GlobalSettings.UseSSL ? "https" : "http";


            string[] subjectVars = {
                                       string.Concat(http.Request.ServerVariables["SERVER_NAME"], ":", http.Request.Url.Port, IOHelper.ResolveUrl(SystemDirectories.Umbraco)),
                                       actionName,
                                       content.Name
                                   };
            string[] bodyVars = {
                                    mailingUser.Name,
                                    actionName,
                                    content.Name,
                                    performingUser.Name,
                                    string.Concat(http.Request.ServerVariables["SERVER_NAME"], ":", http.Request.Url.Port, IOHelper.ResolveUrl(SystemDirectories.Umbraco)),
                                    content.Id.ToString(CultureInfo.InvariantCulture), summary.ToString(),
                                    string.Format("{2}://{0}/{1}",
                                                  string.Concat(http.Request.ServerVariables["SERVER_NAME"], ":", http.Request.Url.Port),
                                                  //TODO: RE-enable this so we can have a nice url
                                                  /*umbraco.library.NiceUrl(documentObject.Id))*/
                                                  string.Concat(content.Id, ".aspx"),
                                                  protocol)

                                };

            // create the mail message
            var mail = new MailMessage(UmbracoConfig.For.UmbracoSettings().Content.NotificationEmailAddress, mailingUser.Email);

            // populate the message
            mail.Subject = createSubject(mailingUser, subjectVars);
            if (UmbracoConfig.For.UmbracoSettings().Content.DisableHtmlEmail)
            {
                mail.IsBodyHtml = false;
                mail.Body = createBody(mailingUser, bodyVars);
            }
            else
            {
                mail.IsBodyHtml = true;
                mail.Body =
                    string.Concat(@"<html><head>
</head>
<body style='font-family: Trebuchet MS, arial, sans-serif; font-color: black;'>
", createBody(mailingUser, bodyVars));
            }

            // nh, issue 30724. Due to hardcoded http strings in resource files, we need to check for https replacements here
            // adding the server name to make sure we don't replace external links
            if (GlobalSettings.UseSSL && string.IsNullOrEmpty(mail.Body) == false)
            {
                string serverName = http.Request.ServerVariables["SERVER_NAME"];
                mail.Body = mail.Body.Replace(
                    string.Format("http://{0}", serverName),
                    string.Format("https://{0}", serverName));
            }

            return new NotificationRequest(mail, actionName, mailingUser.Name, mailingUser.Email);
        }

        private static string ReplaceLinks(string text, HttpRequestBase request)
        {
            var sb = new StringBuilder(GlobalSettings.UseSSL ? "https://" : "http://");
            sb.Append(request.ServerVariables["SERVER_NAME"]);
            sb.Append(":");
            sb.Append(request.Url.Port);
            sb.Append("/");
            var domain = sb.ToString();
            text = text.Replace("href=\"/", "href=\"" + domain);
            text = text.Replace("src=\"/", "src=\"" + domain);
            return text;
        }

        /// <summary>
        /// Replaces the HTML symbols with the character equivalent.
        /// </summary>
        /// <param name="oldString">The old string.</param>
        private static void ReplaceHtmlSymbols(ref string oldString)
        {
            oldString = oldString.Replace("&nbsp;", " ");
            oldString = oldString.Replace("&rsquo;", "'");
            oldString = oldString.Replace("&amp;", "&");
            oldString = oldString.Replace("&ldquo;", "“");
            oldString = oldString.Replace("&rdquo;", "”");
            oldString = oldString.Replace("&quot;", "\"");
        }

        /// <summary>
        /// Compares the text.
        /// </summary>
        /// <param name="oldText">The old text.</param>
        /// <param name="newText">The new text.</param>
        /// <param name="displayInsertedText">if set to <c>true</c> [display inserted text].</param>
        /// <param name="displayDeletedText">if set to <c>true</c> [display deleted text].</param>
        /// <param name="insertedStyle">The inserted style.</param>
        /// <param name="deletedStyle">The deleted style.</param>
        /// <returns></returns>
        private static string CompareText(string oldText, string newText, bool displayInsertedText,
                                          bool displayDeletedText, string insertedStyle, string deletedStyle)
        {
            var sb = new StringBuilder();
            var diffs = Diff.DiffText1(oldText, newText);

            int pos = 0;
            for (int n = 0; n < diffs.Length; n++)
            {
                Diff.Item it = diffs[n];

                // write unchanged chars
                while ((pos < it.StartB) && (pos < newText.Length))
                {
                    sb.Append(newText[pos]);
                    pos++;
                } // while

                // write deleted chars
                if (displayDeletedText && it.DeletedA > 0)
                {
                    sb.Append(deletedStyle);
                    for (int m = 0; m < it.DeletedA; m++)
                    {
                        sb.Append(oldText[it.StartA + m]);
                    } // for
                    sb.Append("</span>");
                }

                // write inserted chars
                if (displayInsertedText && pos < it.StartB + it.InsertedB)
                {
                    sb.Append(insertedStyle);
                    while (pos < it.StartB + it.InsertedB)
                    {
                        sb.Append(newText[pos]);
                        pos++;
                    } // while
                    sb.Append("</span>");
                } // if
            } // while

            // write rest of unchanged chars
            while (pos < newText.Length)
            {
                sb.Append(newText[pos]);
                pos++;
            } // while

            return sb.ToString();
        }

        // manage notifications
        // ideally, would need to use IBackgroundTasks - but they are not part of Core!

        private static readonly object Locker = new object();
        private static readonly BlockingCollection<NotificationRequest> Queue = new BlockingCollection<NotificationRequest>();
        private static volatile bool _running;

        private void Enqueue(NotificationRequest notification)
        {
            Queue.Add(notification);
            if (_running) return;
            lock (Locker)
            {
                if (_running) return;
                Process(Queue);
                _running = true;
            }
        }

        private class NotificationRequest
        {
            public NotificationRequest(MailMessage mail, string action, string userName, string email)
            {
                Mail = mail;
                Action = action;
                UserName = userName;
                Email = email;
            }

            public MailMessage Mail { get; private set; }

            public string Action { get; private set; }

            public string UserName { get; private set; }

            public string Email { get; private set; }
        }

        private void Process(BlockingCollection<NotificationRequest> notificationRequests)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                var s = new SmtpClient();
                try
                {
                    _logger.Debug<NotificationService>("Begin processing notifications.");
                    while (true)
                    {
                        NotificationRequest request;
                        while (notificationRequests.TryTake(out request, 8 * 1000)) // stay on for 8s
                        {
                            try
                            {
                                if (Sendmail != null) Sendmail(s, request.Mail, _logger); else s.Send(request.Mail);
                                _logger.Debug<NotificationService>(string.Format("Notification \"{0}\" sent to {1} ({2})", request.Action, request.UserName, request.Email));
                            }
                            catch (Exception ex)
                            {
                                _logger.Error<NotificationService>("An error occurred sending notification", ex);
                                s.Dispose();
                                s = new SmtpClient();
                            }
                            finally
                            {
                                request.Mail.Dispose();
                            }
                        }
                        lock (Locker)
                        {
                            if (notificationRequests.Count > 0) continue; // last chance
                            _running = false; // going down
                            break;
                        }
                    }
                }
                finally
                {
                    s.Dispose();
                }
                _logger.Debug<NotificationService>("Done processing notifications.");
            });
        }

        // for tests
        internal static Action<SmtpClient, MailMessage, ILogger> Sendmail;
            //= (_, msg, logger) => logger.Debug<NotificationService>("Email " + msg.To.ToString());

        #endregion
    }
}
