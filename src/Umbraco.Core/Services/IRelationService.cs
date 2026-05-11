using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides functionality for managing relations and relation types between entities.
/// </summary>
/// <remarks>
///     Relations allow entities like content, media, and members to be linked together
///     through various relationship types.
/// </remarks>
public interface IRelationService : IService
{
    /// <summary>
    ///     Gets a <see cref="IRelation" /> by its Id.
    /// </summary>
    Task<IRelation?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its Id.
    /// </summary>
    Task<IRelationType?> GetRelationTypeByIdAsync(int id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its key.
    /// </summary>
    Task<IRelationType?> GetRelationTypeByKeyAsync(Guid key, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its Alias.
    /// </summary>
    Task<IRelationType?> GetRelationTypeByAliasAsync(string alias, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets all <see cref="IRelation" /> objects.
    /// </summary>
    /// <param name="ids">Optional array of identifiers to filter on. Pass an empty array for all relations.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<IRelation>> GetAllRelationsAsync(int[] ids, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets all <see cref="IRelation" /> objects for the given <see cref="IRelationType" /> id.
    /// </summary>
    Task<IEnumerable<IRelation>> GetAllRelationsByRelationTypeAsync(int relationTypeId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets all <see cref="IRelationType" /> objects.
    /// </summary>
    /// <param name="ids">Optional array of identifiers to filter on. Pass an empty array for all relation types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<IRelationType>> GetAllRelationTypesAsync(int[] ids, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their parent id, optionally filtered by relation type alias.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByParentIdAsync(int id, string? relationTypeAlias = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child id, optionally filtered by relation type alias.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByChildIdAsync(int id, string? relationTypeAlias = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by parent or child id, optionally filtered by relation type alias.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByParentOrChildIdAsync(int id, string? relationTypeAlias = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a relation by the unique combination of parent id, child id, and relation type.
    /// </summary>
    Task<IRelation?> GetByParentAndChildIdAsync(int parentId, int childId, IRelationType relationType, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the name of the relation type.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByRelationTypeNameAsync(string relationTypeName, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the alias of the relation type.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByRelationTypeAliasAsync(string relationTypeAlias, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the relation type id.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByRelationTypeIdAsync(int relationTypeId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a paged result of <see cref="IRelation" /> filtered by relation type id.
    /// </summary>
    Task<PagedModel<IRelation>> GetPagedByRelationTypeIdAsync(int relationTypeId, int skip, int take, Ordering? ordering = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a paged result of <see cref="IRelation" />.
    /// </summary>
    Task<Attempt<PagedModel<IRelation>, RelationOperationStatus>> GetPagedByRelationTypeKeyAsync(Guid key, int skip, int take, Ordering? ordering = null);

    /// <summary>
    ///     Returns paged parent entities for a related child id.
    /// </summary>
    Task<PagedModel<IUmbracoEntity>> GetPagedParentEntitiesByChildIdAsync(int id, int skip, int take, UmbracoObjectTypes[]? entityTypes = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Returns paged child entities for a related parent id.
    /// </summary>
    Task<PagedModel<IUmbracoEntity>> GetPagedChildEntitiesByParentIdAsync(int id, int skip, int take, UmbracoObjectTypes[]? entityTypes = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets a paged result of <see cref="IRelation" /> objects by child key.
    /// </summary>
    Task<PagedModel<IRelation>> GetPagedByChildKeyAsync(Guid childKey, int skip, int take, string? relationTypeAlias);

    /// <summary>
    ///     Gets the Child object from a Relation as an <see cref="IUmbracoEntity" />.
    /// </summary>
    IUmbracoEntity? GetChildEntityFromRelation(IRelation relation);

    /// <summary>
    ///     Gets the Parent object from a Relation as an <see cref="IUmbracoEntity" />.
    /// </summary>
    IUmbracoEntity? GetParentEntityFromRelation(IRelation relation);

    /// <summary>
    ///     Gets the Parent and Child objects from a Relation as a tuple.
    /// </summary>
    Tuple<IUmbracoEntity, IUmbracoEntity>? GetEntitiesFromRelation(IRelation relation);

    /// <summary>
    ///     Gets the Child objects from a list of Relations.
    /// </summary>
    IEnumerable<IUmbracoEntity> GetChildEntitiesFromRelations(IEnumerable<IRelation> relations);

    /// <summary>
    ///     Gets the Parent objects from a list of Relations.
    /// </summary>
    IEnumerable<IUmbracoEntity> GetParentEntitiesFromRelations(IEnumerable<IRelation> relations);

    /// <summary>
    ///     Gets the Parent and Child objects from a list of Relations.
    /// </summary>
    IEnumerable<Tuple<IUmbracoEntity, IUmbracoEntity>> GetEntitiesFromRelations(IEnumerable<IRelation> relations);

    /// <summary>
    ///     Relates two objects by their entity ids.
    /// </summary>
    Task<IRelation> RelateAsync(int parentId, int childId, IRelationType relationType, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Relates two objects by their entity ids using a relation type alias.
    /// </summary>
    Task<IRelation> RelateAsync(int parentId, int childId, string relationTypeAlias, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Checks whether any relations exist for the passed in <see cref="IRelationType" />.
    /// </summary>
    Task<bool> HasRelationsAsync(IRelationType relationType, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Checks whether any relations exist for the passed in id and direction.
    /// </summary>
    Task<bool> IsRelatedAsync(int id, RelationDirectionFilter directionFilter, int[]? includeRelationTypeIds = null, int[]? excludeRelationTypeIds = null, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Checks whether two items are related.
    /// </summary>
    Task<bool> AreRelatedAsync(int parentId, int childId, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Checks whether two items are related under a specific relation type alias.
    /// </summary>
    Task<bool> AreRelatedAsync(int parentId, int childId, string relationTypeAlias, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Checks whether two items are related under a specific relation type.
    /// </summary>
    Task<bool> AreRelatedAsync(int parentId, int childId, IRelationType relationType, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Saves a <see cref="IRelation" />.
    /// </summary>
    Task SaveAsync(IRelation relation, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Saves a collection of <see cref="IRelation" /> objects.
    /// </summary>
    Task SaveAsync(IEnumerable<IRelation> relations, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Saves a <see cref="IRelationType" />.
    /// </summary>
    Task SaveAsync(IRelationType relationType, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Saves a <see cref="IRelationType" /> with user-attributed audit.
    /// </summary>
    Task<Attempt<IRelationType, RelationTypeOperationStatus>> CreateAsync(IRelationType relationType, Guid userKey);

    /// <summary>
    ///     Updates a <see cref="IRelationType" /> with user-attributed audit.
    /// </summary>
    Task<Attempt<IRelationType, RelationTypeOperationStatus>> UpdateAsync(IRelationType relationType, Guid userKey);

    /// <summary>
    ///     Deletes a <see cref="IRelation" />.
    /// </summary>
    Task DeleteRelationAsync(IRelation relation, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Deletes a <see cref="IRelationType" />.
    /// </summary>
    Task DeleteRelationTypeAsync(IRelationType relationType, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Deletes a <see cref="IRelationType" /> by key with user-attributed audit.
    /// </summary>
    Task<Attempt<IRelationType?, RelationTypeOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Deletes all <see cref="IRelation" /> objects of the passed in <see cref="IRelationType" />.
    /// </summary>
    Task DeleteRelationsOfTypeAsync(IRelationType relationType, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets the total count of relation types.
    /// </summary>
    Task<int> CountRelationTypesAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    /// <summary>
    ///     Gets the relation types in a paged manner.
    /// </summary>
    Task<PagedModel<IRelationType>> GetPagedRelationTypesAsync(int skip, int take, params int[] ids);

    /// <summary>
    ///     Gets all allowed parent/child object types for a given <see cref="IRelationType" />.
    /// </summary>
    IEnumerable<UmbracoObjectTypes> GetAllowedObjectTypes();

    // -------------------------------------------------------------------------
    // Legacy synchronous surface. All members below are obsolete and will be
    // removed in Umbraco 19. Use the *Async counterparts above.
    // -------------------------------------------------------------------------

    /// <summary>
    ///     Gets a <see cref="IRelation" /> by its Id.
    /// </summary>
    [Obsolete("Use GetByIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IRelation? GetById(int id);

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its Id.
    /// </summary>
    [Obsolete("Use GetRelationTypeByIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IRelationType? GetRelationTypeById(int id);

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its key.
    /// </summary>
    [Obsolete("Use GetRelationTypeByKeyAsync() instead. Scheduled for removal in Umbraco 19.")]
    IRelationType? GetRelationTypeById(Guid id);

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its alias.
    /// </summary>
    [Obsolete("Use GetRelationTypeByAliasAsync() instead. Scheduled for removal in Umbraco 19.")]
    IRelationType? GetRelationTypeByAlias(string alias);

    /// <summary>
    ///     Gets all <see cref="IRelation" /> objects.
    /// </summary>
    [Obsolete("Use GetAllRelationsAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation> GetAllRelations(params int[] ids);

    /// <summary>
    ///     Gets all <see cref="IRelation" /> objects for the given <see cref="IRelationType" />.
    /// </summary>
    [Obsolete("Use GetAllRelationsByRelationTypeAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation>? GetAllRelationsByRelationType(IRelationType relationType);

    /// <summary>
    ///     Gets all <see cref="IRelation" /> objects for the given <see cref="IRelationType" /> id.
    /// </summary>
    [Obsolete("Use GetAllRelationsByRelationTypeAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation>? GetAllRelationsByRelationType(int relationTypeId);

    /// <summary>
    ///     Gets all <see cref="IRelationType" /> objects.
    /// </summary>
    [Obsolete("Use GetAllRelationTypesAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelationType> GetAllRelationTypes(params int[] ids);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their parent id.
    /// </summary>
    [Obsolete("Use GetByParentIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation>? GetByParentId(int id);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their parent id, filtered by relation type alias.
    /// </summary>
    [Obsolete("Use GetByParentIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation>? GetByParentId(int id, string relationTypeAlias);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their parent entity.
    /// </summary>
    [Obsolete("Use GetByParentIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation>? GetByParent(IUmbracoEntity parent);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their parent entity, filtered by relation type alias.
    /// </summary>
    [Obsolete("Use GetByParentIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation> GetByParent(IUmbracoEntity parent, string relationTypeAlias);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child id.
    /// </summary>
    [Obsolete("Use GetByChildIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation> GetByChildId(int id);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child id, filtered by relation type alias.
    /// </summary>
    [Obsolete("Use GetByChildIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation> GetByChildId(int id, string relationTypeAlias);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child entity.
    /// </summary>
    [Obsolete("Use GetByChildIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation> GetByChild(IUmbracoEntity child);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child entity, filtered by relation type alias.
    /// </summary>
    [Obsolete("Use GetByChildIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation> GetByChild(IUmbracoEntity child, string relationTypeAlias);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child or parent id.
    /// </summary>
    [Obsolete("Use GetByParentOrChildIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation> GetByParentOrChildId(int id);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child or parent id and relation type alias.
    /// </summary>
    [Obsolete("Use GetByParentOrChildIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation> GetByParentOrChildId(int id, string relationTypeAlias);

    /// <summary>
    ///     Gets a relation by the unique combination of parent id, child id, and relation type.
    /// </summary>
    [Obsolete("Use GetByParentAndChildIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IRelation? GetByParentAndChildId(int parentId, int childId, IRelationType relationType);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the name of the relation type.
    /// </summary>
    [Obsolete("Use GetByRelationTypeNameAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation> GetByRelationTypeName(string relationTypeName);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the alias of the relation type.
    /// </summary>
    [Obsolete("Use GetByRelationTypeAliasAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation> GetByRelationTypeAlias(string relationTypeAlias);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the relation type id.
    /// </summary>
    [Obsolete("Use GetByRelationTypeIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation>? GetByRelationTypeId(int relationTypeId);

    /// <summary>
    ///     Gets a paged result of <see cref="IRelation" /> filtered by relation type id.
    /// </summary>
    [Obsolete("Use GetPagedByRelationTypeIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IRelation> GetPagedByRelationTypeId(int relationTypeId, long pageIndex, int pageSize, out long totalRecords, Ordering? ordering = null);

    /// <summary>
    ///     Returns paged parent entities for a related child id.
    /// </summary>
    [Obsolete("Use GetPagedParentEntitiesByChildIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IUmbracoEntity> GetPagedParentEntitiesByChildId(int id, long pageIndex, int pageSize, out long totalChildren, params UmbracoObjectTypes[] entityTypes);

    /// <summary>
    ///     Returns paged child entities for a related parent id.
    /// </summary>
    [Obsolete("Use GetPagedChildEntitiesByParentIdAsync() instead. Scheduled for removal in Umbraco 19.")]
    IEnumerable<IUmbracoEntity> GetPagedChildEntitiesByParentId(int id, long pageIndex, int pageSize, out long totalChildren, params UmbracoObjectTypes[] entityTypes);

    /// <summary>
    ///     Relates two objects by their entity ids.
    /// </summary>
    [Obsolete("Use RelateAsync() instead. Scheduled for removal in Umbraco 19.")]
    IRelation Relate(int parentId, int childId, IRelationType relationType);

    /// <summary>
    ///     Relates two entities.
    /// </summary>
    [Obsolete("Use RelateAsync() instead. Scheduled for removal in Umbraco 19.")]
    IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, IRelationType relationType);

    /// <summary>
    ///     Relates two objects by their entity ids using a relation type alias.
    /// </summary>
    [Obsolete("Use RelateAsync() instead. Scheduled for removal in Umbraco 19.")]
    IRelation Relate(int parentId, int childId, string relationTypeAlias);

    /// <summary>
    ///     Relates two entities using a relation type alias.
    /// </summary>
    [Obsolete("Use RelateAsync() instead. Scheduled for removal in Umbraco 19.")]
    IRelation Relate(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias);

    /// <summary>
    ///     Checks whether any relations exist for the passed in <see cref="IRelationType" />.
    /// </summary>
    [Obsolete("Use HasRelationsAsync() instead. Scheduled for removal in Umbraco 19.")]
    bool HasRelations(IRelationType relationType);

    /// <summary>
    ///     Checks whether any relations exist for the passed in id and direction.
    /// </summary>
    [Obsolete("Use IsRelatedAsync() instead. Scheduled for removal in Umbraco 19.")]
    bool IsRelated(int id, RelationDirectionFilter directionFilter, int[]? includeRelationTypeIds = null, int[]? excludeRelationTypeIds = null);

    /// <summary>
    ///     Checks whether two items are related.
    /// </summary>
    [Obsolete("Use AreRelatedAsync() instead. Scheduled for removal in Umbraco 19.")]
    bool AreRelated(int parentId, int childId);

    /// <summary>
    ///     Checks whether two entities are related.
    /// </summary>
    [Obsolete("Use AreRelatedAsync() instead. Scheduled for removal in Umbraco 19.")]
    bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child);

    /// <summary>
    ///     Checks whether two entities are related under a specific relation type alias.
    /// </summary>
    [Obsolete("Use AreRelatedAsync() instead. Scheduled for removal in Umbraco 19.")]
    bool AreRelated(IUmbracoEntity parent, IUmbracoEntity child, string relationTypeAlias);

    /// <summary>
    ///     Checks whether two items are related under a specific relation type alias.
    /// </summary>
    [Obsolete("Use AreRelatedAsync() instead. Scheduled for removal in Umbraco 19.")]
    bool AreRelated(int parentId, int childId, string relationTypeAlias);

    /// <summary>
    ///     Saves a <see cref="IRelation" />.
    /// </summary>
    [Obsolete("Use SaveAsync() instead. Scheduled for removal in Umbraco 19.")]
    void Save(IRelation relation);

    /// <summary>
    ///     Saves a collection of <see cref="IRelation" /> objects.
    /// </summary>
    [Obsolete("Use SaveAsync() instead. Scheduled for removal in Umbraco 19.")]
    void Save(IEnumerable<IRelation> relations);

    /// <summary>
    ///     Saves a <see cref="IRelationType" />.
    /// </summary>
    [Obsolete("Use SaveAsync() instead. Scheduled for removal in Umbraco 19.")]
    void Save(IRelationType relationType);

    /// <summary>
    ///     Deletes a <see cref="IRelation" />.
    /// </summary>
    [Obsolete("Use DeleteRelationAsync() instead. Scheduled for removal in Umbraco 19.")]
    void Delete(IRelation relation);

    /// <summary>
    ///     Deletes a <see cref="IRelationType" />.
    /// </summary>
    [Obsolete("Use DeleteRelationTypeAsync() instead. Scheduled for removal in Umbraco 19.")]
    void Delete(IRelationType relationType);

    /// <summary>
    ///     Deletes all <see cref="IRelation" /> objects of the passed in <see cref="IRelationType" />.
    /// </summary>
    [Obsolete("Use DeleteRelationsOfTypeAsync() instead. Scheduled for removal in Umbraco 19.")]
    void DeleteRelationsOfType(IRelationType relationType);

    /// <summary>
    ///     Checks whether two items are related under a specific relation type.
    /// </summary>
    [Obsolete("Use AreRelatedAsync() instead. Scheduled for removal in Umbraco 19.")]
    bool AreRelated(int parentId, int childId, IRelationType relationType);

    /// <summary>
    ///     Gets the total count of relation types.
    /// </summary>
    [Obsolete("Use CountRelationTypesAsync() instead. Scheduled for removal in Umbraco 19.")]
    int CountRelationTypes();
}
