using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Actions;

namespace Umbraco.Web.Compose
{
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
            //Send notifications for the send to publish action
            ContentService.SentToPublish += ContentService_SentToPublish;
            //Send notifications for the published action
            ContentService.Published += ContentService_Published;
            //Send notifications for the saved action
            ContentService.Sorted += ContentService_Sorted;
            //Send notifications for the update and created actions
            ContentService.Saved += ContentService_Saved;
            //Send notifications for the unpublish action
            ContentService.Unpublished += ContentService_Unpublished;
            //Send notifications for the move/move to recycle bin and restore actions
            ContentService.Moved += ContentService_Moved;
            //Send notifications for the delete action when content is moved to the recycle bin
            ContentService.Trashed += ContentService_Trashed;
            //Send notifications for the copy action
            ContentService.Copied += ContentService_Copied;
            //Send notifications for the rollback action
            ContentService.RolledBack += ContentService_RolledBack;
            //Send notifications for the public access changed action
            PublicAccessService.Saved += PublicAccessService_Saved;

            UserService.UserGroupPermissionsAssigned += UserService_UserGroupPermissionsAssigned;
        }

        public void Terminate()
        {
            ContentService.SentToPublish -= ContentService_SentToPublish;
            ContentService.Published -= ContentService_Published;
            ContentService.Sorted -= ContentService_Sorted;
            ContentService.Saved -= ContentService_Saved;
            ContentService.Unpublished -= ContentService_Unpublished;
            ContentService.Moved -= ContentService_Moved;
            ContentService.Trashed -= ContentService_Trashed;
            ContentService.Copied -= ContentService_Copied;
            ContentService.RolledBack -= ContentService_RolledBack;
            PublicAccessService.Saved -= PublicAccessService_Saved;
            UserService.UserGroupPermissionsAssigned -= UserService_UserGroupPermissionsAssigned;
        }

        private void UserService_UserGroupPermissionsAssigned(IUserService sender, Core.Events.SaveEventArgs<EntityPermission> args)
            => UserServiceUserGroupPermissionsAssigned(args, _contentService);

        private void PublicAccessService_Saved(IPublicAccessService sender, Core.Events.SaveEventArgs<PublicAccessEntry> args)
            => PublicAccessServiceSaved(args, _contentService);

        private void ContentService_RolledBack(IContentService sender, Core.Events.RollbackEventArgs<IContent> args)
            => _notifier.Notify(_actions.GetAction<ActionRollback>(), args.Entity);

        private void ContentService_Copied(IContentService sender, Core.Events.CopyEventArgs<IContent> args)
            => _notifier.Notify(_actions.GetAction<ActionCopy>(), args.Original);

        private void ContentService_Trashed(IContentService sender, Core.Events.MoveEventArgs<IContent> args)
            => _notifier.Notify(_actions.GetAction<ActionDelete>(), args.MoveInfoCollection.Select(m => m.Entity).ToArray());

        private void ContentService_Moved(IContentService sender, Core.Events.MoveEventArgs<IContent> args)
            => ContentServiceMoved(args);

        private void ContentService_Unpublished(IContentService sender, Core.Events.PublishEventArgs<IContent> args)
            => _notifier.Notify(_actions.GetAction<ActionUnpublish>(), args.PublishedEntities.ToArray());

        private void ContentService_Saved(IContentService sender, Core.Events.ContentSavedEventArgs args)
            => ContentServiceSaved(args);

        private void ContentService_Sorted(IContentService sender, Core.Events.SaveEventArgs<IContent> args)
            => ContentServiceSorted(sender, args);

        private void ContentService_Published(IContentService sender, Core.Events.ContentPublishedEventArgs args)
            => _notifier.Notify(_actions.GetAction<ActionPublish>(), args.PublishedEntities.ToArray());

        private void ContentService_SentToPublish(IContentService sender, Core.Events.SendToPublishEventArgs<IContent> args)
            => _notifier.Notify(_actions.GetAction<ActionToPublish>(), args.Entity);

        private void ContentServiceSorted(IContentService sender, Core.Events.SaveEventArgs<IContent> args)
        {
            var parentId = args.SavedEntities.Select(x => x.ParentId).Distinct().ToList();
            if (parentId.Count != 1) return; // this shouldn't happen, for sorting all entities will have the same parent id

            // in this case there's nothing to report since if the root is sorted we can't report on a fake entity.
            // this is how it was in v7, we can't report on root changes because you can't subscribe to root changes.
            if (parentId[0] <= 0) return;

            var parent = sender.GetById(parentId[0]);
            if (parent == null) return; // this shouldn't happen

            _notifier.Notify(_actions.GetAction<ActionSort>(), new[] { parent });
        }

        private void ContentServiceSaved(Core.Events.SaveEventArgs<IContent> args)
        {
            var newEntities = new List<IContent>();
            var updatedEntities = new List<IContent>();

            //need to determine if this is updating or if it is new
            foreach (var entity in args.SavedEntities)
            {
                var dirty = (IRememberBeingDirty)entity;
                if (dirty.WasPropertyDirty("Id"))
                {
                    //it's new
                    newEntities.Add(entity);
                }
                else
                {
                    //it's updating
                    updatedEntities.Add(entity);
                }
            }
            _notifier.Notify(_actions.GetAction<ActionNew>(), newEntities.ToArray());
            _notifier.Notify(_actions.GetAction<ActionUpdate>(), updatedEntities.ToArray());
        }

        private void UserServiceUserGroupPermissionsAssigned(Core.Events.SaveEventArgs<EntityPermission> args, IContentService contentService)
        {
            var entities = contentService.GetByIds(args.SavedEntities.Select(e => e.EntityId)).ToArray();
            if (entities.Any() == false)
            {
                return;
            }
            _notifier.Notify(_actions.GetAction<ActionRights>(), entities);
        }

        private void ContentServiceMoved(Core.Events.MoveEventArgs<IContent> args)
        {
            // notify about the move for all moved items
            _notifier.Notify(_actions.GetAction<ActionMove>(), args.MoveInfoCollection.Select(m => m.Entity).ToArray());

            // for any items being moved from the recycle bin (restored), explicitly notify about that too
            var restoredEntities = args.MoveInfoCollection
                .Where(m => m.OriginalPath.Contains(Constants.System.RecycleBinContentString))
                .Select(m => m.Entity)
                .ToArray();
            if (restoredEntities.Any())
            {
                _notifier.Notify(_actions.GetAction<ActionRestore>(), restoredEntities);
            }
        }

        private void PublicAccessServiceSaved(Core.Events.SaveEventArgs<PublicAccessEntry> args, IContentService contentService)
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
            private readonly IUmbracoContextAccessor _umbracoContextAccessor;
            private readonly IRuntimeState _runtimeState;
            private readonly INotificationService _notificationService;
            private readonly IUserService _userService;
            private readonly ILocalizedTextService _textService;
            private readonly IGlobalSettings _globalSettings;
            private readonly IContentSection _contentConfig;
            private readonly ILogger _logger;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="umbracoContextAccessor"></param>
            /// <param name="notificationService"></param>
            /// <param name="userService"></param>
            /// <param name="textService"></param>
            /// <param name="globalSettings"></param>
            /// <param name="contentConfig"></param>
            /// <param name="logger"></param>
            public Notifier(IUmbracoContextAccessor umbracoContextAccessor, IRuntimeState runtimeState, INotificationService notificationService, IUserService userService, ILocalizedTextService textService, IGlobalSettings globalSettings, IContentSection contentConfig, ILogger logger)
            {
                _umbracoContextAccessor = umbracoContextAccessor;
                _runtimeState = runtimeState;
                _notificationService = notificationService;
                _userService = userService;
                _textService = textService;
                _globalSettings = globalSettings;
                _contentConfig = contentConfig;
                _logger = logger;
            }

            public void Notify(IAction action, params IContent[] entities)
            {
                IUser user = null;
                if (_umbracoContextAccessor.UmbracoContext != null)
                {
                    user = _umbracoContextAccessor.UmbracoContext.Security.CurrentUser;
                }

                //if there is no current user, then use the admin
                if (user == null)
                {
                    _logger.Debug(typeof(Notifier), "There is no current Umbraco user logged in, the notifications will be sent from the administrator");
                    user = _userService.GetUserById(Constants.Security.SuperUserId);
                    if (user == null)
                    {
                        _logger.Warn(typeof(Notifier), "Notifications can not be sent, no admin user with id {SuperUserId} could be resolved", Constants.Security.SuperUserId);
                        return;
                    }
                }

                SendNotification(user, entities, action, _runtimeState.ApplicationUrl);
            }

            private void SendNotification(IUser sender, IEnumerable<IContent> entities, IAction action, Uri siteUri)
            {
                if (sender == null) throw new ArgumentNullException(nameof(sender));
                if (siteUri == null)
                {
                    _logger.Warn(typeof(Notifier), "Notifications can not be sent, no site URL is set (might be during boot process?)");
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
                                    "notifications", "mailSubject",
                                    x.user.GetUserCulture(_textService, _globalSettings),
                                    new[] { x.subject.SiteUrl, x.subject.Action, x.subject.ItemName }),
                        ((IUser user, NotificationEmailBodyParams body, bool isHtml) x)
                            => _textService.Localize("notifications",
                                    x.isHtml ? "mailBodyHtml" : "mailBody",
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
                                            ? (x.isHtml ? _textService.Localize("notifications","mailBodyVariantHtmlSummary", new[]{ x.body.Summary }) : _textService.Localize("notifications","mailBodyVariantSummary", new []{ x.body.Summary }))
                                            : x.body.Summary,
                                        x.body.ItemUrl
                                    }));
                }
            }

        }
    }


}
