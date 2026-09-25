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

/// <summary>
///     Handles content-related notifications to send user notifications.
/// </summary>
public sealed class UserNotificationsHandler :
    INotificationAsyncHandler<ContentSavedNotification>,
    INotificationAsyncHandler<ContentSortedNotification>,
    INotificationAsyncHandler<ContentPublishedNotification>,
    INotificationAsyncHandler<ContentMovedNotification>,
    INotificationAsyncHandler<ContentMovedToRecycleBinNotification>,
    INotificationAsyncHandler<ContentCopiedNotification>,
    INotificationAsyncHandler<ContentRolledBackNotification>,
    INotificationAsyncHandler<ContentUnpublishedNotification>,
    INotificationAsyncHandler<AssignedUserGroupPermissionsNotification>,
    INotificationAsyncHandler<PublicAccessEntrySavedNotification>
{
    private readonly ActionCollection _actions;
    private readonly IContentService _contentService;
    private readonly IIdKeyMap _idKeyMap;
    private readonly Notifier _notifier;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UserNotificationsHandler" /> class.
    /// </summary>
    /// <param name="notifier">The notifier service.</param>
    /// <param name="actions">The action collection.</param>
    /// <param name="contentService">The content service.</param>
    /// <param name="idKeyMap">The id-key map used to resolve content ids to keys.</param>
    public UserNotificationsHandler(Notifier notifier, ActionCollection actions, IContentService contentService, IIdKeyMap idKeyMap)
    {
        _notifier = notifier;
        _actions = actions;
        _contentService = contentService;
        _idKeyMap = idKeyMap;
    }

    /// <inheritdoc />
    public async Task HandleAsync(AssignedUserGroupPermissionsNotification notification, CancellationToken cancellationToken)
    {
        Guid[] keys = await ResolveKeysAsync(notification.EntityPermissions.Select(e => e.EntityId));
        IContent[] entities = (await _contentService.GetByIdsAsync(keys, cancellationToken)).ToArray();
        if (entities.Any() == false)
        {
            return;
        }

        _notifier.Notify(_actions.GetAction<ActionRights>(), entities);
    }

    /// <inheritdoc />
    public Task HandleAsync(ContentCopiedNotification notification, CancellationToken cancellationToken)
    {
        _notifier.Notify(_actions.GetAction<ActionCopy>(), notification.Original);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task HandleAsync(ContentMovedNotification notification, CancellationToken cancellationToken)
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

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task HandleAsync(ContentMovedToRecycleBinNotification notification, CancellationToken cancellationToken)
    {
        _notifier.Notify(
            _actions.GetAction<ActionDelete>(),
            notification.MoveInfoCollection.Select(m => m.Entity).ToArray());
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task HandleAsync(ContentPublishedNotification notification, CancellationToken cancellationToken)
    {
        _notifier.Notify(_actions.GetAction<ActionPublish>(), notification.PublishedEntities.ToArray());
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task HandleAsync(ContentRolledBackNotification notification, CancellationToken cancellationToken)
    {
        _notifier.Notify(_actions.GetAction<ActionRollback>(), notification.Entity);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task HandleAsync(ContentSavedNotification notification, CancellationToken cancellationToken)
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
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task HandleAsync(ContentSortedNotification notification, CancellationToken cancellationToken)
    {
        var parentKeys = notification.SortedEntities.Select(x => x.ParentKey).Distinct().ToList();
        if (parentKeys.Count != 1)
        {
            return; // this shouldn't happen, for sorting all entities will have the same parent id
        }

        // in this case there's nothing to report since if the root (or recycle bin) is sorted we can't
        // report on a fake entity. this is how it was in v7, we can't report on root changes because you
        // can't subscribe to root changes.
        Guid? parentKey = parentKeys[0];
        if (parentKey is null || parentKey == Constants.System.RecycleBinContentKey)
        {
            return;
        }

        IContent? parent = await _contentService.GetByIdAsync(parentKey.Value, cancellationToken);
        if (parent == null)
        {
            return; // this shouldn't happen
        }

        _notifier.Notify(_actions.GetAction<ActionSort>(), parent);
    }

    /// <inheritdoc />
    public Task HandleAsync(ContentUnpublishedNotification notification, CancellationToken cancellationToken)
    {
        _notifier.Notify(_actions.GetAction<ActionUnpublish>(), notification.UnpublishedEntities.ToArray());
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task HandleAsync(PublicAccessEntrySavedNotification notification, CancellationToken cancellationToken)
    {
        Guid[] keys = await ResolveKeysAsync(notification.SavedEntities.Select(e => e.ProtectedNodeId));
        IContent[] entities = (await _contentService.GetByIdsAsync(keys, cancellationToken)).ToArray();
        if (entities.Any() == false)
        {
            return;
        }

        _notifier.Notify(_actions.GetAction<ActionProtect>(), entities);
    }

    private async Task<Guid[]> ResolveKeysAsync(IEnumerable<int> ids)
    {
        var keys = new List<Guid>();
        foreach (int id in ids)
        {
            Attempt<Guid> attempt = await _idKeyMap.GetKeyForIdAsync(id, UmbracoObjectTypes.Document);
            if (attempt.Success)
            {
                keys.Add(attempt.Result);
            }
        }

        return keys.ToArray();
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

        /// <summary>
        ///     Sends notifications for the specified action and entities.
        /// </summary>
        /// <param name="action">The action that was performed.</param>
        /// <param name="entities">The entities that were affected.</param>
        public void Notify(IAction? action, params IContent[] entities)
        {
            IUser? user = _backOfficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser;

            // if there is no current user, then use the admin
            if (user == null)
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                {
                    _logger.LogDebug(
                    "There is no current Umbraco user logged in, the notifications will be sent from the administrator");
                }
                user = _userService.GetAsync(Constants.Security.SuperUserKey).GetAwaiter().GetResult();
                if (user == null)
                {
                    _logger.LogWarning(
                        "Notifications can not be sent, no admin user with key {SuperUserKey} could be resolved",
                        Constants.Security.SuperUserKey);
                    return;
                }
            }

            SendNotification(user, entities, action, _hostingEnvironment.ApplicationMainUrl);
        }

        /// <summary>
        ///     Sends notification emails for the specified entities.
        /// </summary>
        /// <param name="sender">The user who performed the action.</param>
        /// <param name="entities">The entities that were affected.</param>
        /// <param name="action">The action that was performed.</param>
        /// <param name="siteUri">The site URI.</param>
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
