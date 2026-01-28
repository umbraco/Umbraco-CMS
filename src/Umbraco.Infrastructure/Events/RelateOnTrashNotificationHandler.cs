// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Events;

public sealed class RelateOnTrashNotificationHandler :
    INotificationHandler<ContentMovedNotification>,
    INotificationHandler<ContentMovedToRecycleBinNotification>,
    INotificationAsyncHandler<ContentMovedToRecycleBinNotification>,
    INotificationHandler<MediaMovedNotification>,
    INotificationHandler<MediaMovedToRecycleBinNotification>,
    INotificationAsyncHandler<MediaMovedToRecycleBinNotification>,
    INotificationHandler<ElementMovedNotification>,
    INotificationAsyncHandler<ElementMovedToRecycleBinNotification>,
    INotificationHandler<EntityContainerMovedNotification>,
    INotificationAsyncHandler<EntityContainerMovedToRecycleBinNotification>
{
    private readonly IAuditService _auditService;
    private readonly IEntityService _entityService;
    private readonly IRelationService _relationService;
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly ILocalizedTextService _textService;

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

    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in V19.")]
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

    /// <inheritdoc />
    public void Handle(ContentMovedNotification notification)
        => DeleteOriginalParentRelationsOnRestore(
            notification.MoveInfoCollection,
            Constants.System.RecycleBinContentString,
            Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias);

    /// <inheritdoc />
    public async Task HandleAsync(ContentMovedToRecycleBinNotification notification, CancellationToken cancellationToken)
        => await CreateOriginalParentRelationOnTrashAsync(
            notification.MoveInfoCollection,
            Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteAlias,
            Constants.Conventions.RelationTypes.RelateParentDocumentOnDeleteName,
            Constants.ObjectTypes.Document,
            Constants.ObjectTypes.Document);

    /// <inheritdoc />
    [Obsolete("Use the INotificationAsyncHandler.HandleAsync implementation instead. Scheduled for removal in V19.")]
    public void Handle(ContentMovedToRecycleBinNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public void Handle(MediaMovedNotification notification)
        => DeleteOriginalParentRelationsOnRestore(
            notification.MoveInfoCollection,
            Constants.System.RecycleBinMediaString,
            Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias);

    /// <inheritdoc />
    public async Task HandleAsync(MediaMovedToRecycleBinNotification notification, CancellationToken cancellationToken)
        => await CreateOriginalParentRelationOnTrashAsync(
            notification.MoveInfoCollection,
            Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteAlias,
            Constants.Conventions.RelationTypes.RelateParentMediaFolderOnDeleteName,
            Constants.ObjectTypes.Media,
            Constants.ObjectTypes.Media);

    /// <inheritdoc />
    [Obsolete("Use the INotificationAsyncHandler.HandleAsync implementation instead. Scheduled for removal in V19.")]
    public void Handle(MediaMovedToRecycleBinNotification notification)
        => HandleAsync(notification, CancellationToken.None).GetAwaiter().GetResult();

    /// <inheritdoc />
    public void Handle(ElementMovedNotification notification)
        => DeleteOriginalParentRelationsOnRestore(
            notification.MoveInfoCollection,
            Constants.System.RecycleBinElementString,
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnElementDeleteAlias);

    /// <inheritdoc />
    public async Task HandleAsync(ElementMovedToRecycleBinNotification notification, CancellationToken cancellationToken)
        => await CreateOriginalParentRelationOnTrashAsync(
            notification.MoveInfoCollection,
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnElementDeleteAlias,
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnElementDeleteName,
            Constants.ObjectTypes.ElementContainer,
            Constants.ObjectTypes.Element);

    /// <inheritdoc />
    public void Handle(EntityContainerMovedNotification notification)
    {
        // Only handle Element containers
        IEnumerable<MoveEventInfo<EntityContainer>> elementContainerItems = notification.MoveInfoCollection
            .Where(x => x.Entity.ContainedObjectType == Constants.ObjectTypes.Element);

        DeleteOriginalParentRelationsOnRestore(
            elementContainerItems,
            Constants.System.RecycleBinElementString,
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnContainerDeleteAlias);
    }

    /// <inheritdoc />
    public async Task HandleAsync(
        EntityContainerMovedToRecycleBinNotification notification,
        CancellationToken cancellationToken)
    {
        // Only handle Element containers
        IEnumerable<MoveToRecycleBinEventInfo<EntityContainer>> elementContainerItems = notification.MoveInfoCollection
            .Where(x => x.Entity.ContainedObjectType == Constants.ObjectTypes.Element)
            .ToArray();

        if (elementContainerItems.Any() is false)
        {
            return;
        }

        await CreateOriginalParentRelationOnTrashAsync(
            elementContainerItems,
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnContainerDeleteAlias,
            Constants.Conventions.RelationTypes.RelateParentElementContainerOnContainerDeleteName,
            Constants.ObjectTypes.ElementContainer,
            Constants.ObjectTypes.ElementContainer);
    }

    private void DeleteOriginalParentRelationsOnRestore<T>(
        IEnumerable<MoveEventInfo<T>> moveInfoCollection,
        string recycleBinPathString,
        string originalParentRelationTypeAlias)
        where T : IEntity
    {
        foreach (MoveEventInfo<T> item in moveInfoCollection.Where(x => x.OriginalPath.Contains(recycleBinPathString)))
        {
            IEnumerable<IRelation> relations = _relationService.GetByChildId(item.Entity.Id);
            foreach (IRelation relation in relations.Where(x => x.RelationType.Alias.InvariantEquals(originalParentRelationTypeAlias)))
            {
                _relationService.Delete(relation);
            }
        }
    }

    private async Task CreateOriginalParentRelationOnTrashAsync<T>(
        IEnumerable<MoveToRecycleBinEventInfo<T>> moveInfoCollection,
        string originalParentRelationTypeAlias,
        string originalParentRelationTypeName,
        Guid parentObjectType,
        Guid childObjectType)
        where T : ITreeEntity
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope();

        IRelationType? relationType = _relationService.GetRelationTypeByAlias(originalParentRelationTypeAlias);

        // Check that the relation-type exists, if not, then recreate it
        if (relationType == null)
        {
            relationType = new RelationType(originalParentRelationTypeName, originalParentRelationTypeAlias, false, parentObjectType, childObjectType, false);
            _relationService.Save(relationType);
        }

        UmbracoObjectTypes entityObjectType = ObjectTypes.GetUmbracoObjectType(childObjectType);
        var localizationKey = GetLocalizationKey(entityObjectType);

        foreach (MoveToRecycleBinEventInfo<T> item in moveInfoCollection)
        {
            IList<string> originalPath = item.OriginalPath.ToDelimitedList();
            var originalParentId = originalPath.Count > 2
                ? int.Parse(originalPath[^2], CultureInfo.InvariantCulture)
                : Constants.System.Root;

            // Before we can create this relation, we need to ensure that the original parent still exists which
            // may not be the case if the encompassing transaction also deleted it when this item was moved to the bin
            if (!_entityService.Exists(originalParentId))
            {
                continue;
            }

            // Add a relation for the item being deleted, so that we can know the original parent for if we need to restore later
            IRelation relation =
                _relationService.GetByParentAndChildId(originalParentId, item.Entity.Id, relationType) ??
                new Relation(originalParentId, item.Entity.Id, relationType);
            _relationService.Save(relation);

            Guid userKey = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser switch
            {
                { } user => user.Key,
                null when item.Entity is IContentBase contentBase => await _userIdKeyResolver.GetAsync(contentBase.WriterId),
                _ => Constants.Security.SuperUserKey, // TODO: Not sure what to default to here (when deleting a container)? Shouldn't these audits be in the service itself where we have better user context?
            };
            await _auditService.AddAsync(
                AuditType.Delete,
                userKey,
                item.Entity.Id,
                entityObjectType.GetName(),
                string.Format(_textService.Localize("recycleBin", localizationKey), item.Entity.Id, originalParentId));
        }

        scope.Complete();
    }

    private static string GetLocalizationKey(UmbracoObjectTypes entityObjectType)
        => entityObjectType switch
        {
            UmbracoObjectTypes.Document => "contentTrashed",
            UmbracoObjectTypes.Media => "mediaTrashed",
            UmbracoObjectTypes.Element => "elementTrashed",
            UmbracoObjectTypes.ElementContainer => "elementContainerTrashed",
            _ => throw new ArgumentOutOfRangeException(nameof(entityObjectType), entityObjectType, "Unsupported entity object type for recycle bin localization."),
        };
}
