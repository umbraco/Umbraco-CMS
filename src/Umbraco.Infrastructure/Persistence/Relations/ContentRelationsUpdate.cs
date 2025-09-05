using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
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
    IDistributedCacheNotificationHandler<MemberSavedNotification>
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
            // Delete all relations using the automatic relation type aliases.
            _relationRepository.DeleteByParent(entity.Id, automaticRelationTypeAliases.ToArray());

            // No need to add new references/relations
            return;
        }

        // Lookup all relation type IDs.
        var relationTypeLookup = _relationTypeRepository.GetMany(Array.Empty<int>())
            .Where(x => automaticRelationTypeAliases.Contains(x.Alias))
            .ToDictionary(x => x.Alias, x => x.Id);

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
            else if (!automaticRelationTypeAliases.Contains(reference.RelationTypeAlias))
            {
                // Returning a reference that doesn't use an automatic relation type is an issue that should be fixed in code.
                _logger.LogError("The reference to {Udi} uses a relation type {RelationTypeAlias} that is not an automatic relation type.", reference.Udi, reference.RelationTypeAlias);
            }
            else if (!relationTypeLookup.TryGetValue(reference.RelationTypeAlias, out int relationTypeId))
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
                relations.Add((id, relationTypeId));
            }
        }

        // Get all existing relations (optimize for adding new and keeping existing relations).
        IQuery<IRelation> query = scope.SqlContext.Query<IRelation>().Where(x => x.ParentId == entity.Id).WhereIn(x => x.RelationTypeId, relationTypeLookup.Values);
        var existingRelations = _relationRepository.GetPagedRelationsByQuery(query, 0, int.MaxValue, out _, null)
            .ToDictionary(x => (x.ChildId, x.RelationTypeId)); // Relations are unique by parent ID, child ID and relation type ID.

        // Add relations that don't exist yet.
        IEnumerable<ReadOnlyRelation> relationsToAdd = relations.Except(existingRelations.Keys).Select(x => new ReadOnlyRelation(entity.Id, x.ChildId, x.RelationTypeId));
        _relationRepository.SaveBulk(relationsToAdd);

        // Delete relations that don't exist anymore.
        foreach (IRelation relation in existingRelations.Where(x => !relations.Contains(x.Key)).Select(x => x.Value))
        {
            _relationRepository.Delete(relation);
        }
    }

    private sealed class NodeIdKey
    {
        [Column("id")]
        public int NodeId { get; set; }

        [Column("uniqueId")]
        public Guid UniqueId { get; set; }
    }
}
