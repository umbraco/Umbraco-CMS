using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Relations;

/// <summary>
/// Defines a notification handler for content saved operations that persists relations.
/// </summary>
internal sealed class ContentRelationsUpdate :
    IDistributedCacheNotificationHandler<ContentSavedNotification>,
    IDistributedCacheNotificationHandler<ContentPublishedNotification>,
    IDistributedCacheNotificationHandler<MediaSavedNotification>,
    IDistributedCacheNotificationHandler<MemberSavedNotification>,
    IDistributedCacheNotificationHandler<ElementSavedNotification>,
    IDistributedCacheNotificationHandler<ElementPublishedNotification>
{
    private readonly IScopeProvider _scopeProvider;
    private readonly DataValueReferenceFactoryCollection _dataValueReferenceFactories;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IRelationRepository _relationRepository;
    private readonly IRelationTypeRepository _relationTypeRepository;
    private readonly ILogger<ContentRelationsUpdate> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentRelationsUpdate"/> class.
    /// </summary>
    public ContentRelationsUpdate(
        IScopeProvider scopeProvider,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        PropertyEditorCollection propertyEditors,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        ILogger<ContentRelationsUpdate> logger)
    {
        _scopeProvider = scopeProvider;
        _dataValueReferenceFactories = dataValueReferenceFactories;
        _propertyEditors = propertyEditors;
        _relationRepository = relationRepository;
        _relationTypeRepository = relationTypeRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public void Handle(ContentSavedNotification notification) => PersistRelations(notification.SavedEntities);

    /// <inheritdoc/>
    public void Handle(IEnumerable<ContentSavedNotification> notifications) => PersistRelations(notifications.SelectMany(x => x.SavedEntities));

    /// <inheritdoc/>
    public void Handle(ContentPublishedNotification notification) => PersistRelations(notification.PublishedEntities);

    /// <inheritdoc/>
    public void Handle(IEnumerable<ContentPublishedNotification> notifications) => PersistRelations(notifications.SelectMany(x => x.PublishedEntities));

    /// <inheritdoc/>
    public void Handle(MediaSavedNotification notification) => PersistRelations(notification.SavedEntities);

    /// <inheritdoc/>
    public void Handle(IEnumerable<MediaSavedNotification> notifications) => PersistRelations(notifications.SelectMany(x => x.SavedEntities));

    /// <inheritdoc/>
    public void Handle(MemberSavedNotification notification) => PersistRelations(notification.SavedEntities);

    /// <inheritdoc/>
    public void Handle(IEnumerable<MemberSavedNotification> notifications) => PersistRelations(notifications.SelectMany(x => x.SavedEntities));

    /// <inheritdoc/>
    public void Handle(ElementSavedNotification notification) => PersistRelations(notification.SavedEntities);

    /// <inheritdoc/>
    public void Handle(IEnumerable<ElementSavedNotification> notifications) => PersistRelations(notifications.SelectMany(x => x.SavedEntities));

    /// <inheritdoc/>
    public void Handle(ElementPublishedNotification notification) => PersistRelations(notification.PublishedEntities);

    /// <inheritdoc/>
    public void Handle(IEnumerable<ElementPublishedNotification> notifications) => PersistRelations(notifications.SelectMany(x => x.PublishedEntities));

    private void PersistRelations(IEnumerable<IContentBase> entities)
    {
        using IScope scope = _scopeProvider.CreateScope();
        foreach (IContentBase entity in entities)
        {
            PersistRelations(scope, entity);
        }

        scope.Complete();
    }

    private void PersistRelations(IScope scope, IContentBase entity)
    {
        // Get all references and automatic relation type aliases.
        ISet<UmbracoEntityReference> references = _dataValueReferenceFactories.GetAllReferences(entity.Properties, _propertyEditors);
        ISet<string> automaticRelationTypeAliases = _dataValueReferenceFactories.GetAllAutomaticRelationTypesAliases(_propertyEditors);

        if (references.Count == 0)
        {
            // Query existing relations before deleting, so we can publish notifications.
            IRelation[] deletedRelations = GetExistingAutomaticRelations(scope, entity.Id, automaticRelationTypeAliases);

            // Delete all relations using the automatic relation type aliases.
            _relationRepository.DeleteByParent(entity.Id, automaticRelationTypeAliases.ToArray());

            if (deletedRelations.Length > 0)
            {
                scope.Notifications.Publish(new RelationDeletedNotification(deletedRelations, new EventMessages()) { IsAutomatic = true });
            }

            return;
        }

        // Lookup all relation types.
        var relationTypeLookup = _relationTypeRepository.GetMany(Array.Empty<int>())
            .Where(x => automaticRelationTypeAliases.Contains(x.Alias))
            .ToDictionary(x => x.Alias);

        if (relationTypeLookup.Count == 0)
        {
            return;
        }

        // Lookup node IDs for all GUID based UDIs.
        IEnumerable<Guid> keys = references.Select(x => x.Udi).OfType<GuidUdi>().Select(x => x.Guid);
        var keysLookup = scope.Database.FetchByGroups<NodeIdKey, Guid>(keys, Constants.Sql.MaxParameterCount, guids =>
        {
            return scope.SqlContext.Sql()
                .Select<NodeDto>(x => x.NodeId, x => x.UniqueId)
                .From<NodeDto>()
                .WhereIn<NodeDto>(x => x.UniqueId, guids);
        }).ToDictionary(x => x.UniqueId, x => x.NodeId);

        // Get all valid relations.
        var relations = new List<(int ChildId, int RelationTypeId)>(references.Count);
        foreach (UmbracoEntityReference reference in references)
        {
            if (string.IsNullOrEmpty(reference.RelationTypeAlias))
            {
                // Reference does not specify a relation type alias, so skip adding a relation.
                _logger.LogDebug("The reference to {Udi} does not specify a relation type alias, so it will not be saved as relation.", reference.Udi);
            }
            else if (automaticRelationTypeAliases.Contains(reference.RelationTypeAlias) is false)
            {
                // Returning a reference that doesn't use an automatic relation type is an issue that should be fixed in code.
                _logger.LogError("The reference to {Udi} uses a relation type {RelationTypeAlias} that is not an automatic relation type.", reference.Udi, reference.RelationTypeAlias);
            }
            else if (relationTypeLookup.TryGetValue(reference.RelationTypeAlias, out IRelationType? relationType) is false)
            {
                // A non-existent relation type could be caused by an environment issue (e.g. it was manually removed).
                _logger.LogWarning("The reference to {Udi} uses a relation type {RelationTypeAlias} that does not exist.", reference.Udi, reference.RelationTypeAlias);
            }
            else if (reference.Udi is not GuidUdi udi || !keysLookup.TryGetValue(udi.Guid, out var id))
            {
                // Relations only support references to items that are stored in the NodeDto table (because of foreign key constraints).
                _logger.LogInformation("The reference to {Udi} can not be saved as relation, because it doesn't have a node ID.", reference.Udi);
            }
            else
            {
                relations.Add((id, relationType.Id));
            }
        }

        // Get all existing relations (optimize for adding new and keeping existing relations).
        IQuery<IRelation> query = scope.SqlContext.Query<IRelation>()
            .Where(x => x.ParentId == entity.Id)
            .WhereIn(x => x.RelationTypeId, relationTypeLookup.Values
            .Select(x => x.Id));
        var existingRelations = _relationRepository.GetPagedRelationsByQuery(query, 0, int.MaxValue, out _, null)
            .ToDictionary(x => (x.ChildId, x.RelationTypeId)); // Relations are unique by parent ID, child ID and relation type ID.

        // Add relations that don't exist yet.
        var newRelationKeys = relations.Except(existingRelations.Keys).ToList();
        IEnumerable<ReadOnlyRelation> relationsToAdd = newRelationKeys.Select(x => new ReadOnlyRelation(entity.Id, x.ChildId, x.RelationTypeId));
        _relationRepository.SaveBulk(relationsToAdd);

        // Publish saved notification for the newly added relations.
        // NOTE: Only RelationSavedNotification is published (not RelationSavingNotification) because
        // SaveBulk is used for performance and does not support cancellation.
        // The Relation objects will have Id = 0 because SaveBulk does not return database identities.
        // ParentId, ChildId, and RelationType are fully populated.
        if (newRelationKeys.Count > 0)
        {
            var relationTypeById = relationTypeLookup.Values.ToDictionary(x => x.Id);
            IRelation[] savedRelations = newRelationKeys
                .Where(x => relationTypeById.ContainsKey(x.RelationTypeId))
                .Select(x => new Relation(entity.Id, x.ChildId, relationTypeById[x.RelationTypeId]))
                .ToArray<IRelation>();

            scope.Notifications.Publish(new RelationSavedNotification(savedRelations, new EventMessages()) { IsAutomatic = true });
        }

        // Delete relations that don't exist anymore.
        IRelation[] relationsToDelete = existingRelations.Where(x => !relations.Contains(x.Key)).Select(x => x.Value).ToArray();
        foreach (IRelation relation in relationsToDelete)
        {
            _relationRepository.Delete(relation);
        }

        // Publish deleted notification for the removed relations.
        if (relationsToDelete.Length > 0)
        {
            scope.Notifications.Publish(new RelationDeletedNotification(relationsToDelete, new EventMessages()) { IsAutomatic = true });
        }
    }

    private IRelation[] GetExistingAutomaticRelations(IScope scope, int entityId, ISet<string> automaticRelationTypeAliases)
    {
        int[] relationTypeIds = _relationTypeRepository.GetMany(Array.Empty<int>())
            .Where(x => automaticRelationTypeAliases.Contains(x.Alias))
            .Select(x => x.Id)
            .ToArray();

        if (relationTypeIds.Length == 0)
        {
            return [];
        }

        IQuery<IRelation> query = scope.SqlContext.Query<IRelation>()
            .Where(x => x.ParentId == entityId)
            .WhereIn(x => x.RelationTypeId, relationTypeIds);

        return _relationRepository.GetPagedRelationsByQuery(query, 0, int.MaxValue, out _, null).ToArray();
    }

    private sealed class NodeIdKey
    {
        /// <summary>
        /// Gets or sets the unique identifier for the node.
        /// </summary>
        [Column("id")]
        public int NodeId { get; set; }

        /// <summary>
        /// Gets or sets the unique GUID that identifies the node in the database.
        /// </summary>
        [Column("uniqueId")]
        public Guid UniqueId { get; set; }
    }
}
