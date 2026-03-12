// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Events;

// TODO: lots of duplicate code in this one, refactor

/// <summary>
/// Handles notifications triggered when content is moved to the recycle bin ("trash") in Umbraco,
/// and manages the relationships between the trashed content and other entities accordingly.
/// </summary>
public sealed class RelateOnTrashNotificationHandler :
    INotificationHandler<ContentMovedNotification>,
    INotificationHandler<ContentMovedToRecycleBinNotification>,
    INotificationAsyncHandler<ContentMovedToRecycleBinNotification>,
    INotificationHandler<MediaMovedNotification>,
    INotificationHandler<MediaMovedToRecycleBinNotification>,
    INotificationAsyncHandler<MediaMovedToRecycleBinNotification>
{
    private readonly IAuditService _auditService;
    private readonly IEntityService _entityService;
    private readonly IRelationService _relationService;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ILocalizedTextService _textService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelateOnTrashNotificationHandler"/> class.
    /// </summary>
    /// <param name="relationService">Service used to manage relations between entities.</param>
    /// <param name="entityService">Service used to manage entities within Umbraco.</param>
    /// <param name="textService">Service for retrieving localized text resources.</param>
    /// <param name="auditService">Service for logging audit events.</param>
    /// <param name="scopeProvider">Provider for managing database scopes.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    /// <param name="userIdKeyResolver">Resolves user ID keys for operations.</param>
    public RelateOnTrashNotificationHandler(
        IRelationService relationService,
        IEntityService entityService,
        ILocalizedTextService textService,
        IAuditService auditService,
        IScopeProvider scopeProvider,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IUserIdKeyResolver userIdKeyResolver)
    {
        _relationService = relationService;
        _entityService = entityService;
        _textService = textService;
        _auditService = auditService;
        _scopeProvider = scopeProvider;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _userIdKeyResolver = userIdKeyResolver;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Core.Events.RelateOnTrashNotificationHandler"/> class.
    /// Handles the creation of relations when entities are moved to the recycle bin (trashed).
    /// </summary>
    /// <param name="relationService">Service used to manage relations between entities.</param>
    /// <param name="entityService">Service for accessing and managing entities.</param>
    /// <param name="textService">Service for retrieving localized text strings.</param>
    /// <param name="auditService">Service for logging audit events.</param>
    /// <param name="scopeProvider">Provides scope management for database operations.</param>
    /// <param name="backOfficeSecurityAccessor">Accessor for back office security context.</param>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public RelateOnTrashNotificationHandler(
        IRelationService relationService,
        IEntityService entityService,
        ILocalizedTextService textService,
        IAuditService auditService,
        IScopeProvider scopeProvider,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor)
        : this(
            relationService,
            entityService,
            textService,
            auditService,
            scopeProvider,
            backOfficeSecurityAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IUserIdKeyResolver>())
    {
    }

    /// <summary>
    /// Handles a <see cref="ContentMovedNotification"/> by removing parent-child relations for content items that have been moved to the recycle bin.
    /// </summary>
    /// <param name="notification">The notification containing information about the moved content items.</param>
    public void Handle(ContentMovedNotification notification)
    {
        foreach (MoveEventInfo<IContent> item in notification.MoveInfoCollection.Where(x =>
                     x.OriginalPath.Contains(Constants.System.RecycleBinContentString)))
        {
            const string relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias;
            IEnumerable<IRelation> relations = _relationService.GetByChildId(item.Entity.Id);

            foreach (IRelation relation in
                     relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)))
            {
                _relationService.Delete(relation);
            }
        }
    }

    /// <inheritdoc />
    public async Task HandleAsync(ContentMovedToRecycleBinNotification notification, CancellationToken cancellationToken)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            const string relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias;
            IRelationType? relationType = _relationService.GetRelationTypeByAlias(relationTypeAlias);

            // check that the relation-type exists, if not, then recreate it
            if (relationType == null)
            {
                Guid documentObjectType = Constants.ObjectTypes.Document;
                const string relationTypeName = Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteName;

                relationType = new RelationType(relationTypeName, relationTypeAlias, false, documentObjectType, documentObjectType, false);
                _relationService.Save(relationType);
            }

            foreach (MoveToRecycleBinEventInfo<IContent> item in notification.MoveInfoCollection)
            {
                IList<string> originalPath = item.OriginalPath.ToDelimitedList();
                var originalParentId = originalPath.Count > 2
                    ? int.Parse(originalPath[originalPath.Count - 2], CultureInfo.InvariantCulture)
                    : Constants.System.Root;

                // before we can create this relation, we need to ensure that the original parent still exists which
                // may not be the case if the encompassing transaction also deleted it when this item was moved to the bin
                if (_entityService.Exists(originalParentId))
                {
                    // Add a relation for the item being deleted, so that we can know the original parent for if we need to restore later
                    IRelation relation =
                        _relationService.GetByParentAndChildId(originalParentId, item.Entity.Id, relationType) ??
                        new Relation(originalParentId, item.Entity.Id, relationType);
                    _relationService.Save(relation);

                    await _auditService.AddAsync(
                        AuditType.Delete,
                        _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Key ?? await _userIdKeyResolver.GetAsync(item.Entity.WriterId),
                        item.Entity.Id,
                        UmbracoObjectTypes.Document.GetName(),
                        string.Format(_textService.Localize("recycleBin", "contentTrashed"), item.Entity.Id, originalParentId));
                }
            }

            scope.Complete();
        }
    }

    /// <summary>
    /// Handles a notification when content is moved to the recycle bin by removing any parent-child relations
    /// of type 'Relate Parent Document On Delete' for the affected content items.
    /// </summary>
    /// <param name="notification">The notification containing details about the content items that have been moved to the recycle bin.</param>
    [Obsolete("Use the INotificationAsyncHandler.HandleAsync implementation instead. Scheduled for removal in Umbraco 19.")]
    public void Handle(ContentMovedToRecycleBinNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <summary>
    /// Handles a <see cref="ContentMovedNotification"/> by removing parent-child relations for content items that have been moved to the recycle bin.
    /// </summary>
    /// <param name="notification">The notification containing information about the moved content items.</param>
    public void Handle(MediaMovedNotification notification)
    {
        foreach (MoveEventInfo<IMedia> item in notification.MoveInfoCollection.Where(x =>
                     x.OriginalPath.Contains(Constants.System.RecycleBinMediaString)))
        {
            const string relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;
            IEnumerable<IRelation> relations = _relationService.GetByChildId(item.Entity.Id);
            foreach (IRelation relation in
                     relations.Where(x => x.RelationType.Alias.InvariantEquals(relationTypeAlias)))
            {
                _relationService.Delete(relation);
            }
        }
    }

    /// <inheritdoc />
    public async Task HandleAsync(MediaMovedToRecycleBinNotification notification, CancellationToken cancellationToken)
    {
        using (ICoreScope scope = _scopeProvider.CreateCoreScope())
        {
            const string relationTypeAlias = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias;
            IRelationType? relationType = _relationService.GetRelationTypeByAlias(relationTypeAlias);

            // check that the relation-type exists, if not, then recreate it
            if (relationType == null)
            {
                Guid documentObjectType = Constants.ObjectTypes.Document;
                const string relationTypeName = Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName;
                relationType = new RelationType(relationTypeName, relationTypeAlias, false, documentObjectType, documentObjectType, false);
                _relationService.Save(relationType);
            }

            foreach (MoveToRecycleBinEventInfo<IMedia> item in notification.MoveInfoCollection)
            {
                IList<string> originalPath = item.OriginalPath.ToDelimitedList();
                var originalParentId = originalPath.Count > 2
                    ? int.Parse(originalPath[originalPath.Count - 2], CultureInfo.InvariantCulture)
                    : Constants.System.Root;

                // before we can create this relation, we need to ensure that the original parent still exists which
                // may not be the case if the encompassing transaction also deleted it when this item was moved to the bin
                if (_entityService.Exists(originalParentId))
                {
                    // Add a relation for the item being deleted, so that we can know the original parent for if we need to restore later
                    IRelation relation =
                        _relationService.GetByParentAndChildId(originalParentId, item.Entity.Id, relationType) ??
                        new Relation(originalParentId, item.Entity.Id, relationType);
                    _relationService.Save(relation);
                    await _auditService.AddAsync(
                        AuditType.Delete,
                        _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Key ?? await _userIdKeyResolver.GetAsync(item.Entity.WriterId),
                        item.Entity.Id,
                        UmbracoObjectTypes.Media.GetName(),
                        string.Format(_textService.Localize("recycleBin", "mediaTrashed"), item.Entity.Id, originalParentId));
                }
            }

            scope.Complete();
        }
    }

    /// <summary>
    /// Handles a <see cref="MediaMovedToRecycleBinNotification"/> by removing relations when media is moved to the recycle bin.
    /// </summary>
    /// <param name="notification">The notification containing information about the media items that were moved to the recycle bin.</param>
    [Obsolete("Use the INotificationAsyncHandler.HandleAsync implementation instead. Scheduled for removal in Umbraco 19.")]
    public void Handle(MediaMovedToRecycleBinNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();
}
