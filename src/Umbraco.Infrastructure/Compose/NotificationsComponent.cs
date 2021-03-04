// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Implement;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Compose
{
    /// <remarks>
    /// TODO: this component must be removed entirely - there is some code duplication in <see cref="UserNotificationsHandler"/> in anticipation of this component being deleted
    /// </remarks>
    public sealed class NotificationsComponent : IComponent
    {
        private readonly Notifier _notifier;
        private readonly ActionCollection _actions;
        private readonly IContentService _contentService;

        public NotificationsComponent(Notifier notifier, ActionCollection actions, IContentService contentService)
        {
            _notifier = notifier;
            _actions = actions;
            _contentService = contentService;
        }

        public void Initialize()
        {
            //Send notifications for the public access changed action
            PublicAccessService.Saved += PublicAccessService_Saved;

            UserService.UserGroupPermissionsAssigned += UserService_UserGroupPermissionsAssigned;
        }

        public void Terminate()
        {
            PublicAccessService.Saved -= PublicAccessService_Saved;
            UserService.UserGroupPermissionsAssigned -= UserService_UserGroupPermissionsAssigned;
        }

        private void UserService_UserGroupPermissionsAssigned(IUserService sender, SaveEventArgs<EntityPermission> args)
            => UserServiceUserGroupPermissionsAssigned(args, _contentService);

        private void PublicAccessService_Saved(IPublicAccessService sender, SaveEventArgs<PublicAccessEntry> args)
            => PublicAccessServiceSaved(args, _contentService);

        private void UserServiceUserGroupPermissionsAssigned(SaveEventArgs<EntityPermission> args, IContentService contentService)
        {
            var entities = contentService.GetByIds(args.SavedEntities.Select(e => e.EntityId)).ToArray();
            if (entities.Any() == false)
            {
                return;
            }
            _notifier.Notify(_actions.GetAction<ActionRights>(), entities);
        }

        private void PublicAccessServiceSaved(SaveEventArgs<PublicAccessEntry> args, IContentService contentService)
        {
            var entities = contentService.GetByIds(args.SavedEntities.Select(e => e.ProtectedNodeId)).ToArray();
            if (entities.Any() == false)
            {
                return;
            }
            _notifier.Notify(_actions.GetAction<ActionProtect>(), entities);
        }

        /// <summary>
        /// This class is used to send the notifications
        /// </summary>
        public sealed class Notifier
        {
            private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
            private readonly IHostingEnvironment _hostingEnvironment;
            private readonly INotificationService _notificationService;
            private readonly IUserService _userService;
            private readonly ILocalizedTextService _textService;
            private readonly GlobalSettings _globalSettings;
            private readonly ILogger<Notifier> _logger;

            /// <summary>
            /// Initializes a new instance of the <see cref="Notifier"/> class.
            /// </summary>
            public Notifier(
                IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
                IHostingEnvironment hostingEnvironment,
                INotificationService notificationService,
                IUserService userService,
                ILocalizedTextService textService,
                IOptions<GlobalSettings> globalSettings,
                ILogger<Notifier> logger)
            {
                _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
                _hostingEnvironment = hostingEnvironment;
                _notificationService = notificationService;
                _userService = userService;
                _textService = textService;
                _globalSettings = globalSettings.Value;
                _logger = logger;
            }

            public void Notify(IAction action, params IContent[] entities)
            {
                var user = _backOfficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser;

                //if there is no current user, then use the admin
                if (user == null)
                {
                    _logger.LogDebug("There is no current Umbraco user logged in, the notifications will be sent from the administrator");
                    user = _userService.GetUserById(Constants.Security.SuperUserId);
                    if (user == null)
                    {
                        _logger.LogWarning("Notifications can not be sent, no admin user with id {SuperUserId} could be resolved", Constants.Security.SuperUserId);
                        return;
                    }
                }

                SendNotification(user, entities, action, _hostingEnvironment.ApplicationMainUrl);
            }

            private void SendNotification(IUser sender, IEnumerable<IContent> entities, IAction action, Uri siteUri)
            {
                if (sender == null) throw new ArgumentNullException(nameof(sender));
                if (siteUri == null)
                {
                    _logger.LogWarning("Notifications can not be sent, no site URL is set (might be during boot process?)");
                    return;
                }

                //group by the content type variation since the emails will be different
                foreach(var contentVariantGroup in entities.GroupBy(x => x.ContentType.Variations))
                {
                    _notificationService.SendNotifications(
                        sender,
                        contentVariantGroup,
                        action.Letter.ToString(CultureInfo.InvariantCulture),
                        _textService.Localize("actions", action.Alias),
                        siteUri,
                        ((IUser user, NotificationEmailSubjectParams subject) x)
                            => _textService.Localize(
                                    "notifications/mailSubject",
                                    x.user.GetUserCulture(_textService, _globalSettings),
                                    new[] { x.subject.SiteUrl, x.subject.Action, x.subject.ItemName }),
                        ((IUser user, NotificationEmailBodyParams body, bool isHtml) x)
                            => _textService.Localize(
                                    x.isHtml ? "notifications/mailBodyHtml" : "notifications/mailBody",
                                    x.user.GetUserCulture(_textService, _globalSettings),
                                    new[]
                                    {
                                        x.body.RecipientName,
                                        x.body.Action,
                                        x.body.ItemName,
                                        x.body.EditedUser,
                                        x.body.SiteUrl,
                                        x.body.ItemId,
                                        //format the summary depending on if it's variant or not
                                        contentVariantGroup.Key == ContentVariation.Culture
                                            ? (x.isHtml ? _textService.Localize("notifications/mailBodyVariantHtmlSummary", new[]{ x.body.Summary }) : _textService.Localize("notifications/mailBodyVariantSummary", new []{ x.body.Summary }))
                                            : x.body.Summary,
                                        x.body.ItemUrl
                                    }));
                }
            }

        }
    }


}
