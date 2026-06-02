using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Persistence.Repositories;

/// <summary>
///     Represents a repository for <see cref="IRelation" /> entities.
/// </summary>
public interface IRelationRepository : IAsyncReadWriteRepository<int, IRelation>
{
    /// <summary>
    ///     Gets all relations for the specified parent identifier, optionally filtered by relation type.
    /// </summary>
    /// <param name="parentId">The parent identifier.</param>
    /// <param name="relationTypeId">The relation type identifier, or <c>null</c> to include all types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<IRelation>> GetByParentIdAsync(
        int parentId,
        int? relationTypeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all relations for the specified child identifier, optionally filtered by relation type.
    /// </summary>
    /// <param name="childId">The child identifier.</param>
    /// <param name="relationTypeId">The relation type identifier, or <c>null</c> to include all types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<IRelation>> GetByChildIdAsync(
        int childId,
        int? relationTypeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all relations where the specified id is either the parent or the child, optionally filtered by relation type.
    /// </summary>
    /// <param name="id">The parent or child identifier.</param>
    /// <param name="relationTypeId">The relation type identifier, or <c>null</c> to include all types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IEnumerable<IRelation>> GetByParentOrChildIdAsync(
        int id,
        int? relationTypeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the relation matching the specified parent, child and relation type, or <c>null</c> if no match exists.
    /// </summary>
    Task<IRelation?> GetByParentAndChildIdAsync(
        int parentId,
        int childId,
        int relationTypeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets all relations of the specified relation type.
    /// </summary>
    Task<IEnumerable<IRelation>> GetByRelationTypeIdAsync(
        int relationTypeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns whether the specified id has any relations matching the given direction filter and relation type filters.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <param name="directionFilter">The direction filter (parent, child, or any).</param>
    /// <param name="includeRelationTypeIds">Relation types to include, or <c>null</c>/empty for all.</param>
    /// <param name="excludeRelationTypeIds">Relation types to exclude, or <c>null</c>/empty for none.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<bool> IsRelatedAsync(
        int id,
        RelationDirectionFilter directionFilter,
        int[]? includeRelationTypeIds = null,
        int[]? excludeRelationTypeIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Returns whether two entities are related, optionally filtered by relation type.
    /// </summary>
    Task<bool> AreRelatedAsync(
        int parentId,
        int childId,
        int? relationTypeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets paged relations for the specified relation type.
    /// </summary>
    /// <param name="relationTypeId">The relation type identifier.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <param name="ordering">The ordering to apply, or <c>null</c> for default ordering by id.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged model of relations.</returns>
    Task<PagedModel<IRelation>> GetPagedByRelationTypeIdAsync(
        int relationTypeId,
        int skip,
        int take,
        Ordering? ordering = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets paged relations by child entity key.
    /// </summary>
    /// <param name="childKey">The unique key of the child entity.</param>
    /// <param name="skip">The number of records to skip.</param>
    /// <param name="take">The number of records to take.</param>
    /// <param name="relationTypeAlias">The relation type alias to filter by, or <c>null</c> for all types.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A paged model of relations.</returns>
    Task<PagedModel<IRelation>> GetPagedByChildKeyAsync(
        Guid childKey,
        int skip,
        int take,
        string? relationTypeAlias,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Persist multiple <see cref="IRelation" /> at once.
    /// </summary>
    Task SaveManyAsync(IEnumerable<IRelation> relations, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Persist multiple <see cref="ReadOnlyRelation" /> at once but Ids are not returned on created relations.
    /// </summary>
    Task SaveBulkAsync(IEnumerable<ReadOnlyRelation> relations, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes all relations for a parent, optionally filtered by relation type aliases.
    /// </summary>
    /// <param name="parentId">The parent entity identifier.</param>
    /// <param name="relationTypeAliases">
    ///     Relation type aliases to match for deletion, or <c>null</c>/empty to delete all relations for the parent.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task DeleteByParentAsync(
        int parentId,
        string[]? relationTypeAliases = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes all relations of the specified relation type.
    /// </summary>
    Task DeleteRelationsOfTypeAsync(int relationTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets paged parent entities for a child entity.
    /// </summary>
    Task<PagedModel<IUmbracoEntity>> GetPagedParentEntitiesByChildIdAsync(
        int childId,
        int skip,
        int take,
        Guid[]? entityTypes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets paged child entities for a parent entity.
    /// </summary>
    Task<PagedModel<IUmbracoEntity>> GetPagedChildEntitiesByParentIdAsync(
        int parentId,
        int skip,
        int take,
        Guid[]? entityTypes = null,
        CancellationToken cancellationToken = default);
}
