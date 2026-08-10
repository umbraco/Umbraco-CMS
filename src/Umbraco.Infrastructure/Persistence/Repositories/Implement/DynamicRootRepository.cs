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
    // The descendant queries can match nodes under several parents, so order per parent to follow the
    // backoffice tree order within each sibling group; the identifier only breaks remaining ties. Without
    // this the order of multiple resolved roots is undefined (https://github.com/umbraco/Umbraco-CMS/issues/23600).
    private const string TreeOrderBy =
        $"ORDER BY n.{NodeDto.ParentIdColumnName}, n.{NodeDto.SortOrderColumnName}, n.{NodeDto.IdColumnName}";

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

    /// <summary>
    /// Asynchronously finds the nearest ancestor or self node, starting from the specified origin node IDs, that matches the provided filter criteria.
    /// </summary>
    /// <param name="origins">A collection of node IDs to use as starting points for the search.</param>
    /// <param name="filter">A <see cref="DynamicRootQueryStep"/> that defines the criteria for matching nodes.</param>
    /// <returns>A task representing the asynchronous operation. The result is the ID of the nearest matching ancestor or self node, or <c>null</c> if none is found.</returns>
    public async Task<Guid?> NearestAncestorOrSelfAsync(IEnumerable<Guid> origins, DynamicRootQueryStep filter)
    {
        Sql<ISqlContext> query = Database.SqlContext.SqlSyntax.SelectTop(
            GetAncestorOrSelfBaseQuery(origins, filter)
            .Append($"ORDER BY n.level DESC"),
            1);

        return await Database.SingleOrDefaultAsync<Guid?>(query);
    }

    /// <summary>
    /// Asynchronously finds the topmost ancestor (or the origin itself) from the specified starting node IDs that matches the provided filter.
    /// </summary>
    /// <param name="origins">A collection of node IDs to use as starting points for the search.</param>
    /// <param name="filter">A filter that determines which nodes qualify as ancestors or self.</param>
    /// <returns>A task representing the asynchronous operation. The result is the ID of the furthest ancestor or self that matches the filter, or <c>null</c> if none is found.</returns>
    public async Task<Guid?> FurthestAncestorOrSelfAsync(IEnumerable<Guid> origins, DynamicRootQueryStep filter)
    {
        Sql<ISqlContext> query = Database.SqlContext.SqlSyntax.SelectTop(
            GetAncestorOrSelfBaseQuery(origins, filter)
                .Append($"ORDER BY n.level ASC"),
            1);

        return await Database.SingleOrDefaultAsync<Guid?>(query);
    }

    private Sql<ISqlContext> GetAncestorOrSelfBaseQuery(IEnumerable<Guid> origins, DynamicRootQueryStep filter)
    {
        Sql<ISqlContext> query = Database.SqlContext.Sql()
            .Select<NodeDto>("n", n => n.UniqueId)
            .From<NodeDto>("norigin")
            .Append( // hack because npoco do not support this
                $"INNER JOIN {Database.SqlContext.SqlSyntax.GetQuotedTableName(NodeDto.TableName)} n ON {Database.SqlContext.SqlSyntax.Substring}(norigin.path, 1, {Database.SqlContext.SqlSyntax.Length}(n.path)) = n.path")
            .InnerJoin<ContentDto>("c")
            .On<ContentDto, NodeDto>((c, n) => c.NodeId == n.NodeId, "c", "n")
            .InnerJoin<ContentTypeDto>("ct")
            .On<ContentDto, ContentTypeDto>((c, ct) => c.ContentTypeId == ct.NodeId, "c", "ct")
            .InnerJoin<NodeDto>("ctn")
            .On<ContentTypeDto, NodeDto>((ct, ctn) => ct.NodeId == ctn.NodeId, "ct", "ctn")
            .Where<NodeDto>(norigin => origins.Contains(norigin.UniqueId), "norigin");

        if (filter.AnyOfDocTypeKeys.Any())
        {
            query = query.Where<NodeDto>(ctn => filter.AnyOfDocTypeKeys.Contains(ctn.UniqueId), "ctn");
        }

        return query;
    }


    /// <summary>
    /// Asynchronously finds, for each origin node, the nearest descendant (including the origin itself) that matches the specified filter, and returns their unique IDs.
    /// </summary>
    /// <param name="origins">A collection of unique IDs representing the origin nodes from which to start the search.</param>
    /// <param name="filter">The filter criteria used to determine matching descendant or self nodes.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of unique IDs for the nearest matching descendant or self node for each origin.
    /// </returns>
    public Task<ICollection<Guid>> NearestDescendantOrSelfAsync(ICollection<Guid> origins, DynamicRootQueryStep filter)
        => DescendantOrSelfAsync(origins, filter, "MIN");

    /// <summary>
    /// Asynchronously finds, for each origin node, the unique identifiers of the deepest (furthest) descendant nodes or the origin node itself,
    /// according to the specified dynamic root query filter.
    /// </summary>
    /// <param name="origins">A collection of unique identifiers representing the origin nodes from which to start the search.</param>
    /// <param name="filter">The dynamic root query step used to constrain or filter the descendant search.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of unique identifiers for the furthest descendant or self nodes found for each origin.</returns>
    public Task<ICollection<Guid>> FurthestDescendantOrSelfAsync(ICollection<Guid> origins, DynamicRootQueryStep filter)
        => DescendantOrSelfAsync(origins, filter, "MAX");

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
        DynamicRootQueryStep filter,
        string levelAggregate)
    {
        var docTypeKeys = filter.AnyOfDocTypeKeys.ToArray();

        // The document type keys are bound once for the outer query and once for the correlated sub query.
        var originsPerBatch = Constants.Sql.MaxParameterCount - (docTypeKeys.Length * 2);

        var keys = new List<Guid>();

        foreach (IEnumerable<Guid> originBatch in origins.InGroupsOf(originsPerBatch))
        {
            Sql<ISqlContext> query = Database.SqlContext.Sql()
                .Select<NodeDto>("n", n => n.UniqueId)
                .DescendantOrSelfBaseQuery(originBatch, filter);

            query = AppendMatchingLevelForOrigin(query, levelAggregate, docTypeKeys).Append(TreeOrderBy);

            keys.AddRange(await Database.FetchAsync<Guid>(query));
        }

        // A node descending from more than one origin can match under each of them.
        return keys.Distinct().ToArray();
    }

    private Sql<ISqlContext> AppendMatchingLevelForOrigin(Sql<ISqlContext> sql, string levelAggregate, Guid[] docTypeKeys)
    {
        ISqlSyntaxProvider syntax = Database.SqlContext.SqlSyntax;
        var nodeTable = syntax.GetQuotedTableName(NodeDto.TableName);
        var contentTable = syntax.GetQuotedTableName(ContentDto.TableName);
        var contentTypeTable = syntax.GetQuotedTableName(ContentTypeDto.TableName);

        var docTypeFilter = docTypeKeys.Length > 0 ? $"AND ctn2.{NodeDto.KeyColumnName} IN (@0)" : string.Empty;

        // Correlated on the outer origin, so the aggregate is over that origin's own matching descendants.
        var matchingLevel = $"""
            AND n.{NodeDto.LevelColumnName} = (
                SELECT {levelAggregate}(n2.{NodeDto.LevelColumnName})
                FROM {nodeTable} n2
                INNER JOIN {contentTable} c2 ON c2.{ContentDto.PrimaryKeyColumnName} = n2.{NodeDto.IdColumnName}
                INNER JOIN {contentTypeTable} ct2 ON ct2.{ContentTypeDto.NodeIdColumnName} = c2.{ContentDto.ContentTypeIdColumnName}
                INNER JOIN {nodeTable} ctn2 ON ctn2.{NodeDto.IdColumnName} = ct2.{ContentTypeDto.NodeIdColumnName}
                WHERE {syntax.Substring}(n2.{NodeDto.PathColumnName}, 1, {syntax.Length}(norigin.{NodeDto.PathColumnName})) = norigin.{NodeDto.PathColumnName}
                {docTypeFilter})
            """;

        return docTypeKeys.Length > 0 ? sql.Append(matchingLevel, docTypeKeys) : sql.Append(matchingLevel);
    }
}

internal static class HelperExtensions
{
    internal static Sql<ISqlContext> DescendantOrSelfBaseQuery(this Sql<ISqlContext> sql, IEnumerable<Guid> origins, DynamicRootQueryStep filter)
    {
        Sql<ISqlContext> query = sql
            .From<NodeDto>("norigin")
            .Append(// hack because npoco do not support this
                $"INNER JOIN {sql.SqlContext.SqlSyntax.GetQuotedTableName(NodeDto.TableName)} n ON {sql.SqlContext.SqlSyntax.Substring}(N.path, 1, {sql.SqlContext.SqlSyntax.Length}(norigin.path)) = norigin.path")
            .InnerJoin<ContentDto>("c")
            .On<ContentDto, NodeDto>((c, n) => c.NodeId == n.NodeId, "c", "n")
            .InnerJoin<ContentTypeDto>("ct")
            .On<ContentDto, ContentTypeDto>((c, ct) => c.ContentTypeId == ct.NodeId, "c", "ct")
            .InnerJoin<NodeDto>("ctn")
            .On<ContentTypeDto, NodeDto>((ct, ctn) => ct.NodeId == ctn.NodeId, "ct", "ctn")
            .Where<NodeDto>(norigin => origins.Contains(norigin.UniqueId), "norigin");

        if (filter.AnyOfDocTypeKeys.Any())
        {
            query = query.Where<NodeDto>(ctn => filter.AnyOfDocTypeKeys.Contains(ctn.UniqueId), "ctn");
        }

        return query;
    }
}
