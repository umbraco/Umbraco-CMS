using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Scoping.EFCore;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing relations and relation types between entities.
/// </summary>
/// <remarks>
///     Relations allow entities like content, media, and members to be linked together
///     through various relationship types. This service handles CRUD operations for both
///     relations and relation types.
/// </remarks>
public class RelationService : AsyncRepositoryService, IRelationService
{
    private readonly IAuditService _auditService;
    private readonly IUserIdKeyResolver _userIdKeyResolver;
    private readonly IEntityService _entityService;
    private readonly IRelationRepository _relationRepository;
    private readonly IRelationTypeRepository _relationTypeRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationService" /> class.
    /// </summary>
    public RelationService(
        IScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IEntityService entityService,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        IAuditService auditService,
        IUserIdKeyResolver userIdKeyResolver)
        : base(provider, loggerFactory, eventMessagesFactory)
    {
        _relationRepository = relationRepository;
        _relationTypeRepository = relationTypeRepository;
        _auditService = auditService;
        _userIdKeyResolver = userIdKeyResolver;
        _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationService" /> class.
    /// </summary>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public RelationService(
        IScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IEntityService entityService,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        IAuditRepository auditRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            entityService,
            relationRepository,
            relationTypeRepository,
            StaticServiceProvider.Instance.GetRequiredService<IAuditService>(),
            userIdKeyResolver)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationService" /> class.
    /// </summary>
    [Obsolete("Use the non-obsolete constructor instead. Scheduled for removal in Umbraco 19.")]
    public RelationService(
        IScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IEntityService entityService,
        IRelationRepository relationRepository,
        IRelationTypeRepository relationTypeRepository,
        IAuditService auditService,
        IAuditRepository auditRepository,
        IUserIdKeyResolver userIdKeyResolver)
        : this(
            provider,
            loggerFactory,
            eventMessagesFactory,
            entityService,
            relationRepository,
            relationTypeRepository,
            auditService,
            userIdKeyResolver)
    {
    }

    /// <inheritdoc />
    public async Task<IRelation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRelation? result = await _relationRepository.GetAsync(id, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IRelationType?> GetRelationTypeByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRelationType? result = await _relationTypeRepository.GetAsync(id, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IRelationType?> GetRelationTypeByKeyAsync(Guid key, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRelationType? result = await _relationTypeRepository.GetAsync(key, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IRelationType?> GetRelationTypeByAliasAsync(string alias, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRelationType? result = await _relationTypeRepository.GetByAliasAsync(alias, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IRelation>> GetAllRelationsAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IEnumerable<IRelation> result = await _relationRepository.GetManyAsync(ids, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IRelation>> GetAllRelationsByRelationTypeAsync(int relationTypeId, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IEnumerable<IRelation> result = await _relationRepository.GetByRelationTypeIdAsync(relationTypeId, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IRelationType>> GetAllRelationTypesAsync(int[] ids, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IEnumerable<IRelationType> result = await _relationTypeRepository.GetManyAsync(ids, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IRelation>> GetByParentIdAsync(int id, string? relationTypeAlias = null, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        try
        {
            if (relationTypeAlias.IsNullOrWhiteSpace())
            {
                return await _relationRepository.GetByParentIdAsync(id, cancellationToken: cancellationToken);
            }

            IRelationType? relationType = await _relationTypeRepository.GetByAliasAsync(relationTypeAlias!, cancellationToken);
            if (relationType is null)
            {
                return Enumerable.Empty<IRelation>();
            }

            return await _relationRepository.GetByParentIdAsync(id, relationType.Id, cancellationToken);
        }
        finally
        {
            scope.Complete();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IRelation>> GetByChildIdAsync(int id, string? relationTypeAlias = null, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        try
        {
            if (relationTypeAlias.IsNullOrWhiteSpace())
            {
                return await _relationRepository.GetByChildIdAsync(id, cancellationToken: cancellationToken);
            }

            IRelationType? relationType = await _relationTypeRepository.GetByAliasAsync(relationTypeAlias!, cancellationToken);
            if (relationType is null)
            {
                return Enumerable.Empty<IRelation>();
            }

            return await _relationRepository.GetByChildIdAsync(id, relationType.Id, cancellationToken);
        }
        finally
        {
            scope.Complete();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IRelation>> GetByParentOrChildIdAsync(int id, string? relationTypeAlias = null, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        try
        {
            if (relationTypeAlias.IsNullOrWhiteSpace())
            {
                return await _relationRepository.GetByParentOrChildIdAsync(id, cancellationToken: cancellationToken);
            }

            IRelationType? relationType = await _relationTypeRepository.GetByAliasAsync(relationTypeAlias!, cancellationToken);
            if (relationType is null)
            {
                return Enumerable.Empty<IRelation>();
            }

            return await _relationRepository.GetByParentOrChildIdAsync(id, relationType.Id, cancellationToken);
        }
        finally
        {
            scope.Complete();
        }
    }

    /// <inheritdoc />
    public async Task<IRelation?> GetByParentAndChildIdAsync(int parentId, int childId, IRelationType relationType, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRelation? result = await _relationRepository.GetByParentAndChildIdAsync(parentId, childId, relationType.Id, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IRelation>> GetByRelationTypeNameAsync(string relationTypeName, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        // The relation type table is small (typically <50 rows) and fully cached by the repository's
        // FullDataSet cache policy, so filtering in memory is cheap and avoids a per-name SQL query.
        IEnumerable<IRelationType> relationTypes = await _relationTypeRepository.GetAllAsync(cancellationToken);
        List<int> relationTypeIds = relationTypes.Where(x => x.Name == relationTypeName).Select(x => x.Id).ToList();
        if (relationTypeIds.Count == 0)
        {
            scope.Complete();
            return Array.Empty<IRelation>();
        }

        IEnumerable<IRelation> result = await GetRelationsByListOfTypeIdsAsync(relationTypeIds, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IRelation>> GetByRelationTypeAliasAsync(string relationTypeAlias, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRelationType? relationType = await _relationTypeRepository.GetByAliasAsync(relationTypeAlias, cancellationToken);
        if (relationType is null)
        {
            scope.Complete();
            return Array.Empty<IRelation>();
        }

        IEnumerable<IRelation> result = await GetRelationsByListOfTypeIdsAsync([relationType.Id], cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IRelation>> GetByRelationTypeIdAsync(int relationTypeId, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IEnumerable<IRelation> result = await _relationRepository.GetByRelationTypeIdAsync(relationTypeId, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IRelation>> GetPagedByRelationTypeIdAsync(int relationTypeId, int skip, int take, Ordering? ordering = null, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        PagedModel<IRelation> result = await _relationRepository.GetPagedByRelationTypeIdAsync(relationTypeId, skip, take, ordering, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IRelation>> GetPagedByChildKeyAsync(Guid childKey, int skip, int take, string? relationTypeAlias)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        PagedModel<IRelation> result = await _relationRepository.GetPagedByChildKeyAsync(childKey, skip, take, relationTypeAlias, CancellationToken.None);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<Attempt<PagedModel<IRelation>, RelationOperationStatus>> GetPagedByRelationTypeKeyAsync(Guid key, int skip, int take, Ordering? ordering = null)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRelationType? relationType = await _relationTypeRepository.GetAsync(key);
        if (relationType is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus<PagedModel<IRelation>, RelationOperationStatus>(RelationOperationStatus.RelationTypeNotFound, null!);
        }

        PagedModel<IRelation> result = await _relationRepository.GetPagedByRelationTypeIdAsync(relationType.Id, skip, take, ordering);
        scope.Complete();
        return Attempt.SucceedWithStatus(RelationOperationStatus.Success, result);
    }

    /// <inheritdoc />
    public async Task<PagedModel<IUmbracoEntity>> GetPagedParentEntitiesByChildIdAsync(int id, int skip, int take, UmbracoObjectTypes[]? entityTypes = null, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        Guid[] typeGuids = (entityTypes ?? []).Select(x => x.GetGuid()).ToArray();
        PagedModel<IUmbracoEntity> result = await _relationRepository.GetPagedParentEntitiesByChildIdAsync(id, skip, take, typeGuids, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IUmbracoEntity>> GetPagedChildEntitiesByParentIdAsync(int id, int skip, int take, UmbracoObjectTypes[]? entityTypes = null, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        Guid[] typeGuids = (entityTypes ?? []).Select(x => x.GetGuid()).ToArray();
        PagedModel<IUmbracoEntity> result = await _relationRepository.GetPagedChildEntitiesByParentIdAsync(id, skip, take, typeGuids, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<PagedModel<IRelationType>> GetPagedRelationTypesAsync(int skip, int take, params int[] ids)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRelationType[] items = (await _relationTypeRepository.GetManyAsync(ids, CancellationToken.None)).ToArray();
        scope.Complete();

        if (take == 0)
        {
            return new PagedModel<IRelationType>(items.Length, Enumerable.Empty<IRelationType>());
        }

        return new PagedModel<IRelationType>(
            items.Length,
            items.OrderBy(relationType => relationType.Name)
                .Skip(skip)
                .Take(take));
    }

    /// <inheritdoc />
    public async Task<int> CountRelationTypesAsync(CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        int count = (await _relationTypeRepository.GetAllAsync(cancellationToken)).Count();
        scope.Complete();
        return count;
    }

    /// <inheritdoc />
    public async Task<IRelation> RelateAsync(int parentId, int childId, IRelationType relationType, CancellationToken cancellationToken = default)
    {
        // Ensure that the RelationType has an identity before using it to relate two entities.
        if (relationType.HasIdentity == false)
        {
            await SaveAsync(relationType, cancellationToken);
        }

        // NOTE: We don't check if this exists first, it will throw a data integrity exception if it already exists.
        var relation = new Relation(parentId, childId, relationType);

        using ICoreScope scope = ScopeProvider.CreateScope();
        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new RelationSavingNotification(relation, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return relation;
        }

        await _relationRepository.SaveAsync(relation, cancellationToken);
        scope.Notifications.Publish(
            new RelationSavedNotification(relation, eventMessages).WithStateFrom(savingNotification));
        scope.Complete();
        return relation;
    }

    /// <inheritdoc />
    public async Task<IRelation> RelateAsync(int parentId, int childId, string relationTypeAlias, CancellationToken cancellationToken = default)
    {
        IRelationType? relationType = await GetRelationTypeByAliasAsync(relationTypeAlias, cancellationToken);
        if (relationType is null || string.IsNullOrEmpty(relationType.Alias))
        {
            throw new ArgumentException($"No RelationType with Alias '{relationTypeAlias}' exists.", nameof(relationTypeAlias));
        }

        return await RelateAsync(parentId, childId, relationType, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasRelationsAsync(IRelationType relationType, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        bool result = (await _relationRepository.GetByRelationTypeIdAsync(relationType.Id, cancellationToken)).Any();
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> IsRelatedAsync(int id, RelationDirectionFilter directionFilter, int[]? includeRelationTypeIds = null, int[]? excludeRelationTypeIds = null, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        bool result = await _relationRepository.IsRelatedAsync(id, directionFilter, includeRelationTypeIds, excludeRelationTypeIds, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> AreRelatedAsync(int parentId, int childId, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        bool result = await _relationRepository.AreRelatedAsync(parentId, childId, cancellationToken: cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> AreRelatedAsync(int parentId, int childId, IRelationType relationType, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        bool result = await _relationRepository.AreRelatedAsync(parentId, childId, relationType.Id, cancellationToken);
        scope.Complete();
        return result;
    }

    /// <inheritdoc />
    public async Task<bool> AreRelatedAsync(int parentId, int childId, string relationTypeAlias, CancellationToken cancellationToken = default)
    {
        IRelationType? relationType = await GetRelationTypeByAliasAsync(relationTypeAlias, cancellationToken);
        if (relationType is null)
        {
            return false;
        }

        return await AreRelatedAsync(parentId, childId, relationType, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SaveAsync(IRelation relation, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new RelationSavingNotification(relation, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return;
        }

        await _relationRepository.SaveAsync(relation, cancellationToken);
        scope.Complete();
        scope.Notifications.Publish(
            new RelationSavedNotification(relation, eventMessages).WithStateFrom(savingNotification));
    }

    /// <inheritdoc />
    public async Task SaveAsync(IEnumerable<IRelation> relations, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRelation[] relationsA = relations.ToArray();

        EventMessages messages = EventMessagesFactory.Get();
        var savingNotification = new RelationSavingNotification(relationsA, messages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return;
        }

        await _relationRepository.SaveManyAsync(relationsA, cancellationToken);
        scope.Complete();
        scope.Notifications.Publish(
            new RelationSavedNotification(relationsA, messages).WithStateFrom(savingNotification));
    }

    /// <inheritdoc />
    public async Task SaveAsync(IRelationType relationType, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        EventMessages eventMessages = EventMessagesFactory.Get();
        var savingNotification = new RelationTypeSavingNotification(relationType, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(savingNotification))
        {
            scope.Complete();
            return;
        }

        await _relationTypeRepository.SaveAsync(relationType, cancellationToken);
        await AuditAsync(AuditType.Save, Constants.Security.SuperUserId, relationType.Id, $"Saved relation type: {relationType.Name}");
        scope.Complete();
        scope.Notifications.Publish(
            new RelationTypeSavedNotification(relationType, eventMessages).WithStateFrom(savingNotification));
    }

    /// <inheritdoc />
    public async Task<Attempt<IRelationType, RelationTypeOperationStatus>> CreateAsync(IRelationType relationType, Guid userKey)
    {
        if (relationType.Id != 0)
        {
            return Attempt.FailWithStatus(RelationTypeOperationStatus.InvalidId, relationType);
        }

        return await SaveWithAuditAsync(
            relationType,
            async () => await _relationTypeRepository.GetAsync(relationType.Key) is not null ? RelationTypeOperationStatus.KeyAlreadyExists : RelationTypeOperationStatus.Success,
            AuditType.New,
            $"Created relation type: {relationType.Name}",
            userKey);
    }

    /// <inheritdoc />
    public async Task<Attempt<IRelationType, RelationTypeOperationStatus>> UpdateAsync(IRelationType relationType, Guid userKey) =>
        await SaveWithAuditAsync(
            relationType,
            async () => await _relationTypeRepository.GetAsync(relationType.Key) is null ? RelationTypeOperationStatus.NotFound : RelationTypeOperationStatus.Success,
            AuditType.Save,
            $"Created relation type: {relationType.Name}",
            userKey);

    /// <inheritdoc />
    public async Task DeleteRelationAsync(IRelation relation, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        EventMessages eventMessages = EventMessagesFactory.Get();
        var deletingNotification = new RelationDeletingNotification(relation, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            scope.Complete();
            return;
        }

        await _relationRepository.DeleteAsync(relation, cancellationToken);
        scope.Complete();
        scope.Notifications.Publish(
            new RelationDeletedNotification(relation, eventMessages).WithStateFrom(deletingNotification));
    }

    /// <inheritdoc />
    public async Task DeleteRelationTypeAsync(IRelationType relationType, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        EventMessages eventMessages = EventMessagesFactory.Get();
        var deletingNotification = new RelationTypeDeletingNotification(relationType, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            scope.Complete();
            return;
        }

        await _relationTypeRepository.DeleteAsync(relationType, cancellationToken);
        scope.Complete();
        scope.Notifications.Publish(
            new RelationTypeDeletedNotification(relationType, eventMessages).WithStateFrom(deletingNotification));
    }

    /// <inheritdoc />
    public async Task<Attempt<IRelationType?, RelationTypeOperationStatus>> DeleteAsync(Guid key, Guid userKey)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        IRelationType? relationType = await _relationTypeRepository.GetAsync(key);
        if (relationType is null)
        {
            scope.Complete();
            return Attempt.FailWithStatus<IRelationType?, RelationTypeOperationStatus>(RelationTypeOperationStatus.NotFound, null);
        }

        EventMessages eventMessages = EventMessagesFactory.Get();
        var deletingNotification = new RelationTypeDeletingNotification(relationType, eventMessages);
        if (await scope.Notifications.PublishCancelableAsync(deletingNotification))
        {
            scope.Complete();
            return Attempt.FailWithStatus<IRelationType?, RelationTypeOperationStatus>(RelationTypeOperationStatus.CancelledByNotification, null);
        }

        await _relationTypeRepository.DeleteAsync(relationType, CancellationToken.None);
        await AuditAsync(AuditType.Delete, userKey, relationType.Id, "Deleted relation type");
        scope.Notifications.Publish(new RelationTypeDeletedNotification(relationType, eventMessages).WithStateFrom(deletingNotification));
        scope.Complete();
        return Attempt.SucceedWithStatus<IRelationType?, RelationTypeOperationStatus>(RelationTypeOperationStatus.Success, relationType);
    }

    /// <inheritdoc />
    public async Task DeleteRelationsOfTypeAsync(IRelationType relationType, CancellationToken cancellationToken = default)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();
        List<IRelation> relations = (await _relationRepository.GetByRelationTypeIdAsync(relationType.Id, cancellationToken)).ToList();
        await _relationRepository.DeleteRelationsOfTypeAsync(relationType.Id, cancellationToken);
        scope.Complete();

        scope.Notifications.Publish(new RelationDeletedNotification(relations, EventMessagesFactory.Get()));
    }

    /// <inheritdoc />
    public IUmbracoEntity? GetChildEntityFromRelation(IRelation relation)
    {
        UmbracoObjectTypes objectType = ObjectTypes.GetUmbracoObjectType(relation.ChildObjectType);
        return _entityService.Get(relation.ChildId, objectType);
    }

    /// <inheritdoc />
    public IUmbracoEntity? GetParentEntityFromRelation(IRelation relation)
    {
        UmbracoObjectTypes objectType = ObjectTypes.GetUmbracoObjectType(relation.ParentObjectType);
        return _entityService.Get(relation.ParentId, objectType);
    }

    /// <inheritdoc />
    public Tuple<IUmbracoEntity, IUmbracoEntity>? GetEntitiesFromRelation(IRelation relation)
    {
        UmbracoObjectTypes childObjectType = ObjectTypes.GetUmbracoObjectType(relation.ChildObjectType);
        UmbracoObjectTypes parentObjectType = ObjectTypes.GetUmbracoObjectType(relation.ParentObjectType);

        IEntitySlim? child = _entityService.Get(relation.ChildId, childObjectType);
        IEntitySlim? parent = _entityService.Get(relation.ParentId, parentObjectType);

        if (parent is null || child is null)
        {
            return null;
        }

        return new Tuple<IUmbracoEntity, IUmbracoEntity>(parent, child);
    }

    /// <inheritdoc />
    public IEnumerable<IUmbracoEntity> GetChildEntitiesFromRelations(IEnumerable<IRelation> relations)
    {
        // Avoid N+1 lookups: group by object type and batch-load each group.
        foreach (IGrouping<UmbracoObjectTypes, IRelation> groupedRelations in relations.GroupBy(x =>
                     ObjectTypes.GetUmbracoObjectType(x.ChildObjectType)))
        {
            UmbracoObjectTypes objectType = groupedRelations.Key;
            var ids = groupedRelations.Select(x => x.ChildId).ToArray();
            foreach (IEntitySlim e in _entityService.GetAll(objectType, ids))
            {
                yield return e;
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<IUmbracoEntity> GetParentEntitiesFromRelations(IEnumerable<IRelation> relations)
    {
        foreach (IGrouping<UmbracoObjectTypes, IRelation> groupedRelations in relations.GroupBy(x =>
                     ObjectTypes.GetUmbracoObjectType(x.ParentObjectType)))
        {
            UmbracoObjectTypes objectType = groupedRelations.Key;
            var ids = groupedRelations.Select(x => x.ParentId).ToArray();
            foreach (IEntitySlim e in _entityService.GetAll(objectType, ids))
            {
                yield return e;
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<Tuple<IUmbracoEntity, IUmbracoEntity>> GetEntitiesFromRelations(IEnumerable<IRelation> relations)
    {
        foreach (IRelation relation in relations)
        {
            UmbracoObjectTypes childObjectType = ObjectTypes.GetUmbracoObjectType(relation.ChildObjectType);
            UmbracoObjectTypes parentObjectType = ObjectTypes.GetUmbracoObjectType(relation.ParentObjectType);

            IEntitySlim? child = _entityService.Get(relation.ChildId, childObjectType);
            IEntitySlim? parent = _entityService.Get(relation.ParentId, parentObjectType);

            if (parent is not null && child is not null)
            {
                yield return new Tuple<IUmbracoEntity, IUmbracoEntity>(parent, child);
            }
        }
    }

    /// <inheritdoc />
    public IEnumerable<UmbracoObjectTypes> GetAllowedObjectTypes() =>
        [
            UmbracoObjectTypes.Document,
            UmbracoObjectTypes.Media,
            UmbracoObjectTypes.Member,
            UmbracoObjectTypes.DocumentType,
            UmbracoObjectTypes.MediaType,
            UmbracoObjectTypes.MemberType,
            UmbracoObjectTypes.DataType,
            UmbracoObjectTypes.MemberGroup,
            UmbracoObjectTypes.ROOT,
            UmbracoObjectTypes.RecycleBin,
        ];

    private RelationTypeOperationStatus? ValidateObjectTypes(IRelationType relationType)
    {
        UmbracoObjectTypes[] allowedObjectTypes = GetAllowedObjectTypes().ToArray();

        bool IsAllowed(Guid? objectType) =>
            objectType is null || allowedObjectTypes.Any(x => x.GetGuid() == objectType);

        if (IsAllowed(relationType.ChildObjectType) is false)
        {
            return RelationTypeOperationStatus.InvalidChildObjectType;
        }

        if (IsAllowed(relationType.ParentObjectType) is false)
        {
            return RelationTypeOperationStatus.InvalidParentObjectType;
        }

        return null;
    }

    private async Task<IEnumerable<IRelation>> GetRelationsByListOfTypeIdsAsync(IEnumerable<int> relationTypeIds, CancellationToken cancellationToken)
    {
        var relations = new List<IRelation>();
        foreach (var relationTypeId in relationTypeIds)
        {
            relations.AddRange(await _relationRepository.GetByRelationTypeIdAsync(relationTypeId, cancellationToken));
        }

        return relations;
    }

    private async Task<Attempt<IRelationType, RelationTypeOperationStatus>> SaveWithAuditAsync(
        IRelationType relationType,
        Func<Task<RelationTypeOperationStatus>> operationValidation,
        AuditType auditType,
        string auditMessage,
        Guid userKey)
    {
        RelationTypeOperationStatus? objectTypeValidationError = ValidateObjectTypes(relationType);
        if (objectTypeValidationError is not null)
        {
            return Attempt.FailWithStatus(objectTypeValidationError.Value, relationType);
        }

        using (ICoreScope scope = ScopeProvider.CreateScope())
        {
            RelationTypeOperationStatus status = await operationValidation();
            if (status != RelationTypeOperationStatus.Success)
            {
                return Attempt.FailWithStatus(status, relationType);
            }

            EventMessages eventMessages = EventMessagesFactory.Get();
            var savingNotification = new RelationTypeSavingNotification(relationType, eventMessages);
            if (await scope.Notifications.PublishCancelableAsync(savingNotification))
            {
                scope.Complete();
                return Attempt.FailWithStatus(RelationTypeOperationStatus.CancelledByNotification, relationType);
            }

            await _relationTypeRepository.SaveAsync(relationType, CancellationToken.None);
            await AuditAsync(auditType, userKey, relationType.Id, auditMessage);
            scope.Complete();
            scope.Notifications.Publish(
                new RelationTypeSavedNotification(relationType, eventMessages).WithStateFrom(savingNotification));
        }

        return Attempt.SucceedWithStatus(RelationTypeOperationStatus.Success, relationType);
    }

    private async Task AuditAsync(AuditType type, int userId, int objectId, string? message = null, string? parameters = null)
    {
        Guid userKey = await _userIdKeyResolver.GetAsync(userId);
        await AuditAsync(type, userKey, objectId, message, parameters);
    }

    private async Task AuditAsync(AuditType type, Guid userKey, int objectId, string? message = null, string? parameters = null) =>
        await _auditService.AddAsync(
            type,
            userKey,
            objectId,
            UmbracoObjectTypes.RelationType.GetName(),
            message,
            parameters);
}
