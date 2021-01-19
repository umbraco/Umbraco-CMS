﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Services.Implement
{
    public class NotificationService : INotificationService
    {
        private readonly IScopeProvider _uowProvider;
        private readonly IUserService _userService;
        private readonly IContentService _contentService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationsRepository _notificationsRepository;
        private readonly IGlobalSettings _globalSettings;
        private readonly IContentSection _contentSection;
        private readonly ILogger _logger;

        public NotificationService(IScopeProvider provider, IUserService userService, IContentService contentService, ILocalizationService localizationService,
            ILogger logger, INotificationsRepository notificationsRepository, IGlobalSettings globalSettings, IContentSection contentSection)
        {
            _notificationsRepository = notificationsRepository;
            _globalSettings = globalSettings;
            _contentSection = contentSection;
            _uowProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _localizationService = localizationService;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            return _contentService.GetVersion(allVersions[prevVersionIndex]);
        }

        /// <summary>
        /// Sends the notifications for the specified user regarding the specified node and action.
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="operatingUser"></param>
        /// <param name="action"></param>
        /// <param name="actionName"></param>
        /// <param name="siteUri"></param>
        /// <param name="createSubject"></param>
        /// <param name="createBody"></param>
        public void SendNotifications(IUser operatingUser, IEnumerable<IContent> entities, string action, string actionName, Uri siteUri,
            Func<(IUser user, NotificationEmailSubjectParams subject), string> createSubject,
            Func<(IUser user, NotificationEmailBodyParams body, bool isHtml), string> createBody)
        {
            var entitiesL = entities.ToList();

            //exit if there are no entities
            if (entitiesL.Count == 0) return;

            //put all entity's paths into a list with the same indices
            var paths = entitiesL.Select(x => x.Path.Split(',').Select(int.Parse).ToArray()).ToArray();

            // lazily get versions
            var prevVersionDictionary = new Dictionary<int, IContentBase>();

            // see notes above
            var id = Constants.Security.SuperUserId;
            const int pagesz = 400; // load batches of 400 users
            do
            {
                // users are returned ordered by id, notifications are returned ordered by user id
                var users = ((UserService)_userService).GetNextUsers(id, pagesz).Where(x => x.IsApproved).ToList();
                var notifications = GetUsersNotifications(users.Select(x => x.Id), action, Enumerable.Empty<int>(), Constants.ObjectTypes.Document).ToList();
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
                        var req = CreateNotificationRequest(operatingUser, user, content, prevVersionDictionary[content.Id], actionName, siteUri, createSubject, createBody);
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
            using (var scope = _uowProvider.CreateScope(autoComplete: true))
            {
                return _notificationsRepository.GetUsersNotifications(userIds, action, nodeIds, objectType);
            }
        }
        /// <summary>
        /// Gets the notifications for the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IEnumerable<Notification> GetUserNotifications(IUser user)
        {
            using (var scope = _uowProvider.CreateScope(autoComplete: true))
            {
                return _notificationsRepository.GetUserNotifications(user);
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
            using (var scope = _uowProvider.CreateScope(autoComplete: true))
            {
                return _notificationsRepository.GetEntityNotifications(entity);
            }
        }

        /// <summary>
        /// Deletes notifications by entity
        /// </summary>
        /// <param name="entity"></param>
        public void DeleteNotifications(IEntity entity)
        {
            using (var scope = _uowProvider.CreateScope())
            {
                _notificationsRepository.DeleteNotifications(entity);
                scope.Complete();
            }
        }

        /// <summary>
        /// Deletes notifications by user
        /// </summary>
        /// <param name="user"></param>
        public void DeleteNotifications(IUser user)
        {
            using (var scope = _uowProvider.CreateScope())
            {
                _notificationsRepository.DeleteNotifications(user);
                scope.Complete();
            }
        }

        /// <summary>
        /// Delete notifications by user and entity
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entity"></param>
        public void DeleteNotifications(IUser user, IEntity entity)
        {
            using (var scope = _uowProvider.CreateScope())
            {
                _notificationsRepository.DeleteNotifications(user, entity);
                scope.Complete();
            }
        }

        /// <summary>
        /// Sets the specific notifications for the user and entity
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entity"></param>
        /// <param name="actions"></param>
        /// <remarks>
        /// This performs a full replace
        /// </remarks>
        public IEnumerable<Notification> SetNotifications(IUser user, IEntity entity, string[] actions)
        {
            using (var scope = _uowProvider.CreateScope())
            {
                var notifications = _notificationsRepository.SetNotifications(user, entity, actions);
                scope.Complete();
                return notifications;
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
            using (var scope = _uowProvider.CreateScope())
            {
                var notification = _notificationsRepository.CreateNotification(user, entity, action);
                scope.Complete();
                return notification;
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
        /// <param name="siteUri"></param>
        /// <param name="createSubject">Callback to create the mail subject</param>
        /// <param name="createBody">Callback to create the mail body</param>
        private NotificationRequest CreateNotificationRequest(IUser performingUser, IUser mailingUser, IContent content, IContentBase oldDoc,
            string actionName,
            Uri siteUri,
            Func<(IUser user, NotificationEmailSubjectParams subject), string> createSubject,
            Func<(IUser user, NotificationEmailBodyParams body, bool isHtml), string> createBody)
        {
            if (performingUser == null) throw new ArgumentNullException("performingUser");
            if (mailingUser == null) throw new ArgumentNullException("mailingUser");
            if (content == null) throw new ArgumentNullException("content");
            if (siteUri == null) throw new ArgumentNullException("siteUri");
            if (createSubject == null) throw new ArgumentNullException("createSubject");
            if (createBody == null) throw new ArgumentNullException("createBody");

            // build summary
            var summary = new StringBuilder();

            if (content.ContentType.VariesByNothing())
            {
                if (!_contentSection.DisableHtmlEmail)
                {
                    //create the HTML summary for invariant content

                    //list all of the property values like we used to
                    summary.Append("<table style=\"width: 100 %; \">");
                    foreach (var p in content.Properties)
                    {
                        // TODO: doesn't take into account variants

                        var newText = p.GetValue() != null ? p.GetValue().ToString() : "";
                        var oldText = newText;

                        // check if something was changed and display the changes otherwise display the fields
                        if (oldDoc.Properties.Contains(p.PropertyType.Alias))
                        {
                            var oldProperty = oldDoc.Properties[p.PropertyType.Alias];
                            oldText = oldProperty.GetValue() != null ? oldProperty.GetValue().ToString() : "";

                            // replace HTML with char equivalent
                            ReplaceHtmlSymbols(ref oldText);
                            ReplaceHtmlSymbols(ref newText);
                        }

                        //show the values
                        summary.Append("<tr>");
                        summary.Append("<th style='text-align: left; vertical-align: top; width: 25%;border-bottom: 1px solid #CCC'>");
                        summary.Append(p.PropertyType.Name);
                        summary.Append("</th>");
                        summary.Append("<td style='text-align: left; vertical-align: top;border-bottom: 1px solid #CCC'>");
                        summary.Append(newText);
                        summary.Append("</td>");
                        summary.Append("</tr>");
                    }
                    summary.Append("</table>");
                }
                
            }
            else if (content.ContentType.VariesByCulture())
            {
                //it's variant, so detect what cultures have changed

                if (!_contentSection.DisableHtmlEmail)
                {
                    //Create the HTML based summary (ul of culture names)

                    var culturesChanged = content.CultureInfos.Values.Where(x => x.WasDirty())
                        .Select(x => x.Culture)
                        .Select(_localizationService.GetLanguageByIsoCode)
                        .WhereNotNull()
                        .Select(x => x.CultureName);
                    summary.Append("<ul>");
                    foreach (var culture in culturesChanged)
                    {
                        summary.Append("<li>");
                        summary.Append(culture);
                        summary.Append("</li>");
                    }
                    summary.Append("</ul>");
                }
                else
                {
                    //Create the text based summary (csv of culture names)

                    var culturesChanged = string.Join(", ", content.CultureInfos.Values.Where(x => x.WasDirty())
                        .Select(x => x.Culture)
                        .Select(_localizationService.GetLanguageByIsoCode)
                        .WhereNotNull()
                        .Select(x => x.CultureName));

                    summary.Append("'");
                    summary.Append(culturesChanged);
                    summary.Append("'");
                }
            }
            else
            {
                //not supported yet...
                throw new NotSupportedException();
            }

            var protocol = _globalSettings.UseHttps ? "https" : "http";

            var subjectVars = new NotificationEmailSubjectParams(
                string.Concat(siteUri.Authority, IOHelper.ResolveUrl(SystemDirectories.Umbraco)),
                actionName,
                content.Name);

            var bodyVars = new NotificationEmailBodyParams(
                mailingUser.Name,
                actionName,
                content.Name,
                content.Id.ToString(CultureInfo.InvariantCulture),
                string.Format("{2}://{0}/{1}",
                    string.Concat(siteUri.Authority),
                    // TODO: RE-enable this so we can have a nice URL
                    /*umbraco.library.NiceUrl(documentObject.Id))*/
                    string.Concat(content.Id, ".aspx"),
                    protocol),
                performingUser.Name,
                string.Concat(siteUri.Authority, IOHelper.ResolveUrl(SystemDirectories.Umbraco)),
                summary.ToString());

            // create the mail message
            var mail = new MailMessage(_contentSection.NotificationEmailAddress, mailingUser.Email);

            // populate the message


            mail.Subject = createSubject((mailingUser, subjectVars));
            if (_contentSection.DisableHtmlEmail)
            {
                mail.IsBodyHtml = false;
                mail.Body = createBody((user: mailingUser, body: bodyVars, false));
            }
            else
            {
                mail.IsBodyHtml = true;
                mail.Body =
                    string.Concat(@"<html><head>
</head>
<body style='font-family: Trebuchet MS, arial, sans-serif; font-color: black;'>
", createBody((user: mailingUser, body: bodyVars, true)));
            }

            // nh, issue 30724. Due to hardcoded http strings in resource files, we need to check for https replacements here
            // adding the server name to make sure we don't replace external links
            if (_globalSettings.UseHttps && string.IsNullOrEmpty(mail.Body) == false)
            {
                string serverName = siteUri.Host;
                mail.Body = mail.Body.Replace(
                    string.Format("http://{0}", serverName),
                    string.Format("https://{0}", serverName));
            }

            return new NotificationRequest(mail, actionName, mailingUser.Name, mailingUser.Email);
        }

        private string ReplaceLinks(string text, Uri siteUri)
        {
            var sb = new StringBuilder(_globalSettings.UseHttps ? "https://" : "http://");
            sb.Append(siteUri.Authority);
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
            if (oldString.IsNullOrWhiteSpace()) return;
            oldString = oldString.Replace("&nbsp;", " ");
            oldString = oldString.Replace("&rsquo;", "'");
            oldString = oldString.Replace("&amp;", "&");
            oldString = oldString.Replace("&ldquo;", "“");
            oldString = oldString.Replace("&rdquo;", "”");
            oldString = oldString.Replace("&quot;", "\"");
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
                                _logger.Debug<NotificationService>("Notification '{Action}' sent to {Username} ({Email})", request.Action, request.UserName, request.Email);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error<NotificationService>(ex, "An error occurred sending notification");
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
