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
    Task<IRelation?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its Id.
    /// </summary>
    Task<IRelationType?> GetRelationTypeByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its key.
    /// </summary>
    Task<IRelationType?> GetRelationTypeByKeyAsync(Guid key, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a <see cref="IRelationType" /> by its Alias.
    /// </summary>
    Task<IRelationType?> GetRelationTypeByAliasAsync(string alias, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all <see cref="IRelation" /> objects.
    /// </summary>
    /// <param name="ids">Optional array of identifiers to filter on. Pass an empty array for all relations.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<IRelation>> GetAllRelationsAsync(int[] ids, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all <see cref="IRelation" /> objects for the given <see cref="IRelationType" /> id.
    /// </summary>
    Task<IEnumerable<IRelation>> GetAllRelationsByRelationTypeAsync(int relationTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all <see cref="IRelationType" /> objects.
    /// </summary>
    /// <param name="ids">Optional array of identifiers to filter on. Pass an empty array for all relation types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<IRelationType>> GetAllRelationTypesAsync(int[] ids, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their parent id, optionally filtered by relation type alias.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByParentIdAsync(int id, string? relationTypeAlias = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by their child id, optionally filtered by relation type alias.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByChildIdAsync(int id, string? relationTypeAlias = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by parent or child id, optionally filtered by relation type alias.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByParentOrChildIdAsync(int id, string? relationTypeAlias = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a relation by the unique combination of parent id, child id, and relation type.
    /// </summary>
    Task<IRelation?> GetByParentAndChildIdAsync(int parentId, int childId, IRelationType relationType, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the name of the relation type.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByRelationTypeNameAsync(string relationTypeName, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the alias of the relation type.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByRelationTypeAliasAsync(string relationTypeAlias, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a list of <see cref="IRelation" /> objects by the relation type id.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByRelationTypeIdAsync(int relationTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a paged result of <see cref="IRelation" /> filtered by relation type id.
    /// </summary>
    Task<PagedModel<IRelation>> GetPagedByRelationTypeIdAsync(int relationTypeId, int skip, int take, Ordering? ordering = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets a paged result of <see cref="IRelation" />.
    /// </summary>
    Task<Attempt<PagedModel<IRelation>, RelationOperationStatus>> GetPagedByRelationTypeKeyAsync(Guid key, int skip, int take, Ordering? ordering = null);

    /// <summary>
    ///     Returns paged parent entities for a related child id.
    /// </summary>
    Task<PagedModel<IUmbracoEntity>> GetPagedParentEntitiesByChildIdAsync(int id, int skip, int take, UmbracoObjectTypes[]? entityTypes = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns paged child entities for a related parent id.
    /// </summary>
    Task<PagedModel<IUmbracoEntity>> GetPagedChildEntitiesByParentIdAsync(int id, int skip, int take, UmbracoObjectTypes[]? entityTypes = null, CancellationToken cancellationToken = default);

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
    Task<IRelation> RelateAsync(int parentId, int childId, IRelationType relationType, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Relates two objects by their entity ids using a relation type alias.
    /// </summary>
    Task<IRelation> RelateAsync(int parentId, int childId, string relationTypeAlias, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether any relations exist for the passed in <see cref="IRelationType" />.
    /// </summary>
    Task<bool> HasRelationsAsync(IRelationType relationType, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether any relations exist for the passed in id and direction.
    /// </summary>
    Task<bool> IsRelatedAsync(int id, RelationDirectionFilter directionFilter, int[]? includeRelationTypeIds = null, int[]? excludeRelationTypeIds = null, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether two items are related.
    /// </summary>
    Task<bool> AreRelatedAsync(int parentId, int childId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether two items are related under a specific relation type alias.
    /// </summary>
    Task<bool> AreRelatedAsync(int parentId, int childId, string relationTypeAlias, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Checks whether two items are related under a specific relation type.
    /// </summary>
    Task<bool> AreRelatedAsync(int parentId, int childId, IRelationType relationType, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves a <see cref="IRelation" />.
    /// </summary>
    Task SaveAsync(IRelation relation, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves a collection of <see cref="IRelation" /> objects.
    /// </summary>
    Task SaveAsync(IEnumerable<IRelation> relations, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Saves a <see cref="IRelationType" />.
    /// </summary>
    Task SaveAsync(IRelationType relationType, CancellationToken cancellationToken = default);

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
    Task DeleteRelationAsync(IRelation relation, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a <see cref="IRelationType" />.
    /// </summary>
    Task DeleteRelationTypeAsync(IRelationType relationType, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a <see cref="IRelationType" /> by key with user-attributed audit.
    /// </summary>
    Task<Attempt<IRelationType?, RelationTypeOperationStatus>> DeleteAsync(Guid key, Guid userKey);

    /// <summary>
    ///     Deletes all <see cref="IRelation" /> objects of the passed in <see cref="IRelationType" />.
    /// </summary>
    Task DeleteRelationsOfTypeAsync(IRelationType relationType, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the total count of relation types.
    /// </summary>
    Task<int> CountRelationTypesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the relation types in a paged manner.
    /// </summary>
    Task<PagedModel<IRelationType>> GetPagedRelationTypesAsync(int skip, int take, params int[] ids);

    /// <summary>
    ///     Gets all allowed parent/child object types for a given <see cref="IRelationType" />.
    /// </summary>
    IEnumerable<UmbracoObjectTypes> GetAllowedObjectTypes();
}
