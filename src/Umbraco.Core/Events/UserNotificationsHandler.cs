// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Events;

public sealed class UserNotificationsHandler :
    INotificationHandler<ContentSavedNotification>,
    INotificationHandler<ContentSortedNotification>,
    INotificationHandler<ContentPublishedNotification>,
    INotificationHandler<ContentMovedNotification>,
    INotificationHandler<ContentMovedToRecycleBinNotification>,
    INotificationHandler<ContentCopiedNotification>,
    INotificationHandler<ContentRolledBackNotification>,
    INotificationHandler<ContentSentToPublishNotification>,
    INotificationHandler<ContentUnpublishedNotification>,
    INotificationHandler<AssignedUserGroupPermissionsNotification>,
    INotificationHandler<PublicAccessEntrySavedNotification>
{
    private readonly ActionCollection _actions;
    private readonly IContentService _contentService;
    private readonly Notifier _notifier;

    public UserNotificationsHandler(Notifier notifier, ActionCollection actions, IContentService contentService)
    {
        _notifier = notifier;
        _actions = actions;
        _contentService = contentService;
    }

    public void Handle(AssignedUserGroupPermissionsNotification notification)
    {
        IContent[]? entities = _contentService.GetByIds(notification.EntityPermissions.Select(e => e.EntityId)).ToArray();
        if (entities?.Any() == false)
        {
            return;
        }

        _notifier.Notify(_actions.GetAction<ActionRights>(), entities!);
    }

    public void Handle(ContentCopiedNotification notification) =>
        _notifier.Notify(_actions.GetAction<ActionCopy>(), notification.Original);

    public void Handle(ContentMovedNotification notification)
    {
        // notify about the move for all moved items
        _notifier.Notify(
            _actions.GetAction<ActionMove>(),
            notification.MoveInfoCollection.Select(m => m.Entity).ToArray());

        // for any items being moved from the recycle bin (restored), explicitly notify about that too
        IContent[] restoredEntities = notification.MoveInfoCollection
            .Where(m => m.OriginalPath.Contains(Constants.System.RecycleBinContentString))
            .Select(m => m.Entity)
            .ToArray();
        if (restoredEntities.Any())
        {
            _notifier.Notify(_actions.GetAction<ActionRestore>(), restoredEntities);
        }
    }

    public void Handle(ContentMovedToRecycleBinNotification notification) => _notifier.Notify(
        _actions.GetAction<ActionDelete>(), notification.MoveInfoCollection.Select(m => m.Entity).ToArray());

    public void Handle(ContentPublishedNotification notification) =>
        _notifier.Notify(_actions.GetAction<ActionPublish>(), notification.PublishedEntities.ToArray());

    public void Handle(ContentRolledBackNotification notification) =>
        _notifier.Notify(_actions.GetAction<ActionRollback>(), notification.Entity);

    public void Handle(ContentSavedNotification notification)
    {
        var newEntities = new List<IContent>();
        var updatedEntities = new List<IContent>();

        // need to determine if this is updating or if it is new
        foreach (IContent entity in notification.SavedEntities)
        {
            var dirty = (IRememberBeingDirty)entity;
            if (dirty.WasPropertyDirty("Id"))
            {
                // it's new
                newEntities.Add(entity);
            }
            else
            {
                // it's updating
                updatedEntities.Add(entity);
            }
        }

        _notifier.Notify(_actions.GetAction<ActionNew>(), newEntities.ToArray());
        _notifier.Notify(_actions.GetAction<ActionUpdate>(), updatedEntities.ToArray());
    }

    public void Handle(ContentSentToPublishNotification notification) =>
        _notifier.Notify(_actions.GetAction<ActionToPublish>(), notification.Entity);

    public void Handle(ContentSortedNotification notification)
    {
        var parentId = notification.SortedEntities.Select(x => x.ParentId).Distinct().ToList();
        if (parentId.Count != 1)
        {
            return; // this shouldn't happen, for sorting all entities will have the same parent id
        }

        // in this case there's nothing to report since if the root is sorted we can't report on a fake entity.
        // this is how it was in v7, we can't report on root changes because you can't subscribe to root changes.
        if (parentId[0] <= 0)
        {
            return;
        }

        IContent? parent = _contentService.GetById(parentId[0]);
        if (parent == null)
        {
            return; // this shouldn't happen
        }

        _notifier.Notify(_actions.GetAction<ActionSort>(), parent);
    }

    public void Handle(ContentUnpublishedNotification notification) =>
        _notifier.Notify(_actions.GetAction<ActionUnpublish>(), notification.UnpublishedEntities.ToArray());

    public void Handle(PublicAccessEntrySavedNotification notification)
    {
        IContent[] entities = _contentService.GetByIds(notification.SavedEntities.Select(e => e.ProtectedNodeId)).ToArray();
        if (entities.Any() == false)
        {
            return;
        }

        _notifier.Notify(_actions.GetAction<ActionProtect>(), entities);
    }

    /// <summary>
    ///     This class is used to send the notifications
    /// </summary>
    public sealed class Notifier
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ILogger<Notifier> _logger;
        private readonly INotificationService _notificationService;
        private readonly ILocalizedTextService _textService;
        private readonly IUserService _userService;
        private GlobalSettings _globalSettings;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Notifier" /> class.
        /// </summary>
        public Notifier(
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IHostingEnvironment hostingEnvironment,
            INotificationService notificationService,
            IUserService userService,
            ILocalizedTextService textService,
            IOptionsMonitor<GlobalSettings> globalSettings,
            ILogger<Notifier> logger)
        {
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _hostingEnvironment = hostingEnvironment;
            _notificationService = notificationService;
            _userService = userService;
            _textService = textService;
            _globalSettings = globalSettings.CurrentValue;
            _logger = logger;

            globalSettings.OnChange(x => _globalSettings = x);
        }

        public void Notify(IAction? action, params IContent[] entities)
        {
            IUser? user = _backOfficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser;

            // if there is no current user, then use the admin
            if (user == null)
            {
                _logger.LogDebug(
                    "There is no current Umbraco user logged in, the notifications will be sent from the administrator");
                user = _userService.GetUserById(Constants.Security.SuperUserId);
                if (user == null)
                {
                    _logger.LogWarning(
                        "Notifications can not be sent, no admin user with id {SuperUserId} could be resolved",
                        Constants.Security.SuperUserId);
                    return;
                }
            }

            SendNotification(user, entities, action, _hostingEnvironment.ApplicationMainUrl);
        }

        private void SendNotification(IUser sender, IEnumerable<IContent> entities, IAction? action, Uri? siteUri)
        {
            if (sender == null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            if (siteUri == null)
            {
                _logger.LogWarning("Notifications can not be sent, no site URL is set (might be during boot process?)");
                return;
            }

            // group by the content type variation since the emails will be different
            foreach (IGrouping<ContentVariation, IContent> contentVariantGroup in entities.GroupBy(x =>
                         x.ContentType.Variations))
            {
                _notificationService.SendNotifications(
                    sender,
                    contentVariantGroup,
                    action?.Letter.ToString(CultureInfo.InvariantCulture),
                    _textService.Localize("actions", action?.Alias),
                    siteUri,
                    x
                        => _textService.Localize(
                            "notifications", "mailSubject", x.user.GetUserCulture(_textService, _globalSettings), new[] { x.subject.SiteUrl, x.subject.Action, x.subject.ItemName }),
                    x
                        => _textService.Localize(
                            "notifications",
                            x.isHtml ? "mailBodyHtml" : "mailBody",
                            x.user.GetUserCulture(_textService, _globalSettings),
                            new[]
                            {
                                x.body.RecipientName, x.body.Action, x.body.ItemName, x.body.EditedUser, x.body.SiteUrl,
                                x.body.ItemId,

                                // format the summary depending on if it's variant or not
                                contentVariantGroup.Key == ContentVariation.Culture
                                    ? x.isHtml
                                        ? _textService.Localize("notifications", "mailBodyVariantHtmlSummary", new[] { x.body.Summary })
                                        : _textService.Localize("notifications", "mailBodyVariantSummary", new[] { x.body.Summary })
                                    : x.body.Summary,
                                x.body.ItemUrl,
                            }));
            }
        }
    }
}
