using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Components;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Web.Actions;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using System.Linq;
using Umbraco.Core.Models.Membership;
using System;
using System.Globalization;

namespace Umbraco.Web.Components
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public sealed class NotificationsComponent : UmbracoComponentBase, IUmbracoCoreComponent
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);
            composition.RegisterSingleton<Notifier>();
        }

        public void Initialize(INotificationService notificationService, Notifier notifier, ActionCollection actions)
        {
            //Send notifications for the send to publish action
            ContentService.SentToPublish += (sender, args) => notifier.Notify(actions.GetAction<ActionToPublish>(), args.Entity);

            //Send notifications for the published action
            ContentService.Published += (sender, args) => notifier.Notify(actions.GetAction<ActionPublish>(), args.PublishedEntities.ToArray());

            //Send notifications for the saved action
            ContentService.Sorted += (sender, args) => ContentServiceSorted(notifier, sender, args, actions);

            //Send notifications for the update and created actions
            ContentService.Saved += (sender, args) => ContentServiceSaved(notifier, sender, args, actions);

            //Send notifications for the delete action
            ContentService.Deleted += (sender, args) => notifier.Notify(actions.GetAction<ActionDelete>(), args.DeletedEntities.ToArray());

            //Send notifications for the unpublish action
            ContentService.Unpublished += (sender, args) => notifier.Notify(actions.GetAction<ActionUnpublish>(), args.PublishedEntities.ToArray());
        }

        private void ContentServiceSorted(Notifier notifier, IContentService sender, Core.Events.SaveEventArgs<IContent> args, ActionCollection actions)
        {
            var parentId = args.SavedEntities.Select(x => x.ParentId).Distinct().ToList();
            if (parentId.Count != 1) return; // this shouldn't happen, for sorting all entities will have the same parent id

            // in this case there's nothing to report since if the root is sorted we can't report on a fake entity.
            // this is how it was in v7, we can't report on root changes because you can't subscribe to root changes.
            if (parentId[0] <= 0) return;

            var parent = sender.GetById(parentId[0]);
            if (parent == null) return; // this shouldn't happen

            notifier.Notify(actions.GetAction<ActionSort>(), new[] { parent });
        }

        private void ContentServiceSaved(Notifier notifier, IContentService sender, Core.Events.SaveEventArgs<IContent> args, ActionCollection actions)
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
            notifier.Notify(actions.GetAction<ActionNew>(), newEntities.ToArray());
            notifier.Notify(actions.GetAction<ActionUpdate>(), updatedEntities.ToArray());
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
                if (siteUri == null) throw new ArgumentNullException(nameof(siteUri));

                //group by the content type variation since the emails will be different
                foreach(var contentVariantGroup in entities.GroupBy(x => x.ContentType.Variations))
                {
                    if (contentVariantGroup.Key == ContentVariation.CultureAndSegment || contentVariantGroup.Key == ContentVariation.Segment)
                        throw new NotSupportedException("Segments are not yet supported in Umbraco");

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
