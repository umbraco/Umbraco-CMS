using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Provides methods for managing dynamic root entities in the persistence layer of Umbraco CMS.
/// This repository handles data access and operations related to dynamic root objects.
/// </summary>
public class DynamicRootRepository : IDynamicRootRepository
{
    private const string OriginAlias = "norigin";
    private const string RelativeAlias = "n";

    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicRootRepository"/> class, which provides data access for dynamic root entities.
    /// </summary>
    /// <param name="scopeAccessor">An <see cref="IScopeAccessor"/> used to manage the database scope for repository operations.</param>
    public DynamicRootRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    private IUmbracoDatabase Database
    {
        get
        {
            if (_scopeAccessor.AmbientScope is null)
            {
                throw new NotSupportedException("Need to be executed in a scope");
            }

            return _scopeAccessor.AmbientScope.Database;
        }
    }

    /// <inheritdoc/>
    [Obsolete("Use NearestAncestorsOrSelfAsync instead, which resolves an ancestor for each origin. Scheduled for removal in Umbraco 20.")]
    public Task<Guid?> NearestAncestorOrSelfAsync(IEnumerable<Guid> origins, DynamicRootQueryStep queryStep)
        => SingleAncestorOrSelfAsync(origins, queryStep, deepest: true);

    /// <inheritdoc/>
    [Obsolete("Use FurthestAncestorsOrSelfAsync instead, which resolves an ancestor for each origin. Scheduled for removal in Umbraco 20.")]
    public Task<Guid?> FurthestAncestorOrSelfAsync(IEnumerable<Guid> origins, DynamicRootQueryStep queryStep)
        => SingleAncestorOrSelfAsync(origins, queryStep, deepest: false);

    /// <summary>
    /// Finds the single deepest or shallowest matching ancestor (or self) across all of the origins.
    /// </summary>
    /// <remarks>
    /// Each batch of origins yields its own candidate, and the candidates are then compared by level, so the result
    /// does not depend on how the origins are batched.
    /// </remarks>
    private async Task<Guid?> SingleAncestorOrSelfAsync(IEnumerable<Guid> origins, DynamicRootQueryStep queryStep, bool deepest)
    {
        var direction = deepest ? "DESC" : "ASC";
        var docTypeKeys = queryStep.AnyOfDocTypeKeys.ToArray();
        var originsPerBatch = Math.Max(1, Constants.Sql.MaxParameterCount - docTypeKeys.Length);
        var candidates = new List<Guid>();

        foreach (IEnumerable<Guid> originBatch in origins.InGroupsOf(originsPerBatch))
        {
            IReadOnlyCollection<Guid> originKeys = originBatch.ToArray();
            Sql<ISqlContext> query = Database.SqlContext.SqlSyntax.SelectTop(
                GetRelativesQuery(QuotedColumn(RelativeAlias, NodeDto.KeyColumnName), docTypeKeys, IsAncestorOrSelf)
                    .Where<NodeDto>(norigin => originKeys.Contains(norigin.UniqueId), OriginAlias)
                    .Append($"ORDER BY {QuotedColumn(RelativeAlias, NodeDto.LevelColumnName)} {direction}"),
                1);

            Guid? candidate = await Database.SingleOrDefaultAsync<Guid?>(query);
            if (candidate.HasValue)
            {
                candidates.Add(candidate.Value);
            }
        }

        if (candidates.Count <= 1)
        {
            return candidates.Count == 1 ? candidates[0] : null;
        }

        Sql<ISqlContext> levelsQuery = Database.SqlContext.Sql()
            .Select<NodeDto>(n => n.UniqueId, n => n.Level)
            .From<NodeDto>()
            .WhereIn<NodeDto>(n => n.UniqueId, candidates);
        Dictionary<Guid, int> levels = (await Database.FetchAsync<NodeDto>(levelsQuery))
            .ToDictionary(n => n.UniqueId, n => (int)n.Level);

        return deepest
            ? candidates.OrderByDescending(key => levels[key]).First()
            : candidates.OrderBy(key => levels[key]).First();
    }

    /// <inheritdoc/>
    public Task<ICollection<Guid>> NearestAncestorsOrSelfAsync(ICollection<Guid> origins, DynamicRootQueryStep queryStep)
        => AncestorOrSelfAsync(origins, queryStep, "MAX");

    /// <inheritdoc/>
    public Task<ICollection<Guid>> FurthestAncestorsOrSelfAsync(ICollection<Guid> origins, DynamicRootQueryStep queryStep)
        => AncestorOrSelfAsync(origins, queryStep, "MIN");

    /// <summary>
    /// Asynchronously finds, for each origin node, the nearest descendant (including the origin itself) that matches the specified filter, and returns their unique IDs.
    /// </summary>
    /// <param name="origins">A collection of unique IDs representing the origin nodes from which to start the search.</param>
    /// <param name="queryStep">The filter criteria used to determine matching descendant or self nodes.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of unique IDs for the nearest matching descendant or self node for each origin.
    /// </returns>
    public Task<ICollection<Guid>> NearestDescendantOrSelfAsync(ICollection<Guid> origins, DynamicRootQueryStep queryStep)
        => DescendantOrSelfAsync(origins, queryStep, "MIN");

    /// <summary>
    /// Asynchronously finds, for each origin node, the unique identifiers of the deepest (furthest) descendant nodes or the origin node itself,
    /// according to the specified dynamic root query filter.
    /// </summary>
    /// <param name="origins">A collection of unique identifiers representing the origin nodes from which to start the search.</param>
    /// <param name="queryStep">The dynamic root query step used to constrain or filter the descendant search.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of unique identifiers for the furthest descendant or self nodes found for each origin.</returns>
    public Task<ICollection<Guid>> FurthestDescendantOrSelfAsync(ICollection<Guid> origins, DynamicRootQueryStep queryStep)
        => DescendantOrSelfAsync(origins, queryStep, "MAX");

    /// <summary>
    /// Finds, for each origin, the matching ancestor (or the origin itself) nearest to or furthest from that origin,
    /// and returns the union of those.
    /// </summary>
    /// <remarks>
    /// As with the descendant queries, the matching level is resolved per origin rather than once across all of
    /// them, so every origin contributes its own ancestor instead of a single one being picked for the whole set
    /// (https://github.com/umbraco/Umbraco-CMS/issues/23600). An ancestor chain holds one node per level, so this
    /// yields at most one key per origin.
    /// </remarks>
    private async Task<ICollection<Guid>> AncestorOrSelfAsync(
        ICollection<Guid> origins,
        DynamicRootQueryStep queryStep,
        string levelAggregate)
    {
        ICollection<Guid> keys = await RelativesAtMatchingLevelAsync(origins, queryStep, levelAggregate, IsAncestorOrSelf);

        // Origins sharing an ancestor resolve to the same key.
        return keys.Distinct().ToArray();
    }

    /// <summary>
    /// Finds, for each origin, the matching descendants (or the origin itself) at its own shallowest or deepest
    /// matching level, and returns the union of those.
    /// </summary>
    /// <remarks>
    /// The matching level is resolved per origin, relative to that origin, rather than once across all of them:
    /// otherwise an origin whose nearest match sits deeper than another's would contribute nothing at all
    /// (https://github.com/umbraco/Umbraco-CMS/issues/23600). Because the origins are independent of each other,
    /// they can also safely be queried in batches.
    /// </remarks>
    private async Task<ICollection<Guid>> DescendantOrSelfAsync(
        ICollection<Guid> origins,
        DynamicRootQueryStep queryStep,
        string levelAggregate)
    {
        ICollection<Guid> keys = await RelativesAtMatchingLevelAsync(origins, queryStep, levelAggregate, IsDescendantOrSelf);

        // A node descending from more than one origin can match under each of them.
        return keys.Distinct().ToArray();
    }

    /// <summary>
    /// How many origins can be bound to one query alongside the document type keys. Resolving the relatives binds,
    /// besides the origins, one level per distinct matching level, and there are never more of those than origins.
    /// A batch holds at least one origin however many document types there are.
    /// </summary>
    private static int OriginsPerBatch(Guid[] docTypeKeys)
        => Math.Max(1, (Constants.Sql.MaxParameterCount - docTypeKeys.Length) / 2);

    /// <summary>
    /// Resolves, per origin, the relatives at the level produced by <paramref name="levelAggregate"/> over that
    /// origin's own matching relatives, where <paramref name="isRelative"/> builds the predicate relating a relative to
    /// an origin from their aliases.
    /// </summary>
    /// <remarks>
    /// The matching levels are fetched first and the relatives at them second. Expressing the per origin level within
    /// a single query, whether as a correlated sub query or a joined derived table, lets the database re-evaluate the
    /// aggregate for every candidate row, which does not scale to large trees.
    /// </remarks>
    private async Task<ICollection<Guid>> RelativesAtMatchingLevelAsync(
        ICollection<Guid> origins,
        DynamicRootQueryStep queryStep,
        string levelAggregate,
        Func<string, string, string> isRelative)
    {
        ISqlSyntaxProvider syntax = Database.SqlContext.SqlSyntax;
        var docTypeKeys = queryStep.AnyOfDocTypeKeys.ToArray();
        var originId = QuotedColumn(OriginAlias, NodeDto.IdColumnName);
        var relativeLevel = QuotedColumn(RelativeAlias, NodeDto.LevelColumnName);
        var keys = new List<Guid>();

        foreach (IEnumerable<Guid> originBatch in origins.InGroupsOf(OriginsPerBatch(docTypeKeys)))
        {
            IReadOnlyCollection<Guid> originKeys = originBatch.ToArray();

            Sql<ISqlContext> levelQuery = GetRelativesQuery(
                    $"{originId} AS {syntax.GetQuotedName(nameof(OriginLevel.OriginId))}, {levelAggregate}({relativeLevel}) AS {syntax.GetQuotedName(nameof(OriginLevel.Level))}",
                    docTypeKeys,
                    isRelative)
                .Where<NodeDto>(norigin => originKeys.Contains(norigin.UniqueId), OriginAlias)
                .Append($"GROUP BY {originId}");
            List<OriginLevel> originLevels = await Database.FetchAsync<OriginLevel>(levelQuery);
            if (originLevels.Count == 0)
            {
                continue;
            }

            var conditions = new List<string>();
            var args = new List<object>();
            foreach (IGrouping<int, OriginLevel> originsAtLevel in originLevels.GroupBy(x => x.Level))
            {
                conditions.Add($"({originId} IN (@{args.Count}) AND {relativeLevel} = @{args.Count + 1})");
                args.Add(originsAtLevel.Select(x => x.OriginId).ToArray());
                args.Add(originsAtLevel.Key);
            }

            Sql<ISqlContext> keyQuery = GetRelativesQuery(QuotedColumn(RelativeAlias, NodeDto.KeyColumnName), docTypeKeys, isRelative)
                .Where($"({string.Join(" OR ", conditions)})", args.ToArray())
                .Append(TreeOrderBy());
            keys.AddRange(await Database.FetchAsync<Guid>(keyQuery));
        }

        return keys;
    }

    /// <summary>
    /// Selects <paramref name="columns"/> over the origins joined to their relatives of the given document types.
    /// </summary>
    private Sql<ISqlContext> GetRelativesQuery(
        string columns,
        IReadOnlyCollection<Guid> docTypeKeys,
        Func<string, string, string> isRelative)
    {
        ISqlSyntaxProvider syntax = Database.SqlContext.SqlSyntax;
        Sql<ISqlContext> query = Database.SqlContext.Sql()
            .Select(columns)
            .From<NodeDto>(OriginAlias)
            .Append($"INNER JOIN {syntax.GetQuotedTableName(NodeDto.TableName)} {syntax.GetQuotedName(RelativeAlias)} ON {isRelative(OriginAlias, RelativeAlias)}") // hack because npoco do not support this
            .InnerJoin<ContentDto>("c")
            .On<ContentDto, NodeDto>((c, n) => c.NodeId == n.NodeId, "c", RelativeAlias)
            .InnerJoin<ContentTypeDto>("ct")
            .On<ContentDto, ContentTypeDto>((c, ct) => c.ContentTypeId == ct.NodeId, "c", "ct")
            .InnerJoin<NodeDto>("ctn")
            .On<ContentTypeDto, NodeDto>((ct, ctn) => ct.NodeId == ctn.NodeId, "ct", "ctn");

        if (docTypeKeys.Count > 0)
        {
            query = query.Where<NodeDto>(ctn => docTypeKeys.Contains(ctn.UniqueId), "ctn");
        }

        return query;
    }

    // The queries can match nodes under several parents, so order per parent to follow the backoffice tree order
    // within each sibling group; the identifier only breaks remaining ties. Without this the order of multiple
    // resolved roots is undefined (https://github.com/umbraco/Umbraco-CMS/issues/23600).
    private string TreeOrderBy()
        => $"ORDER BY {QuotedColumn(RelativeAlias, NodeDto.ParentIdColumnName)}, {QuotedColumn(RelativeAlias, NodeDto.SortOrderColumnName)}, {QuotedColumn(RelativeAlias, NodeDto.IdColumnName)}";

    private string IsAncestorOrSelf(string origin, string relative)
    {
        ISqlSyntaxProvider syntax = Database.SqlContext.SqlSyntax;
        var originPath = QuotedColumn(origin, NodeDto.PathColumnName);
        var relativePath = QuotedColumn(relative, NodeDto.PathColumnName);
        return $"{syntax.Substring}({originPath}, 1, {syntax.Length}({relativePath})) = {relativePath}";
    }

    private string IsDescendantOrSelf(string origin, string relative)
    {
        ISqlSyntaxProvider syntax = Database.SqlContext.SqlSyntax;
        var originPath = QuotedColumn(origin, NodeDto.PathColumnName);
        var relativePath = QuotedColumn(relative, NodeDto.PathColumnName);
        return $"{syntax.Substring}({relativePath}, 1, {syntax.Length}({originPath})) = {originPath}";
    }

    private string QuotedColumn(string alias, string columnName)
        => Database.SqlContext.SqlSyntax.GetQuotedColumn(alias, columnName);

    private sealed class OriginLevel
    {
        public int OriginId { get; set; }

        public int Level { get; set; }
    }
}
