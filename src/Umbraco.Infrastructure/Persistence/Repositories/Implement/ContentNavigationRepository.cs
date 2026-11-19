using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Represents a repository responsible for managing and persisting content navigation structures within the Umbraco CMS.
/// This includes operations related to retrieving, storing, and updating navigation data for content items.
/// </summary>
public class ContentNavigationRepository : INavigationRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentNavigationRepository"/> class, which provides methods for content navigation persistence.
    /// </summary>
    /// <param name="scopeAccessor">An accessor for the current database scope, used to manage transactional operations within the repository.</param>
    public ContentNavigationRepository(IScopeAccessor scopeAccessor)
        => _scopeAccessor = scopeAccessor;

    private IScope? AmbientScope => _scopeAccessor.AmbientScope;

    /// <inheritdoc />
    public IEnumerable<INavigationModel> GetContentNodesByObjectType(Guid objectTypeKey)
        => GetContentNodesByObjectType([objectTypeKey]);

    /// <inheritdoc />
    public IEnumerable<INavigationModel> GetTrashedContentNodesByObjectType(Guid objectTypeKey)
        => GetTrashedContentNodesByObjectType([objectTypeKey]);

    /// <inheritdoc />
    public IEnumerable<INavigationModel> GetContentNodesByObjectType(IEnumerable<Guid> objectTypeKeys)
        => FetchNavigationDtos(objectTypeKeys, false);

    /// <inheritdoc />
    public IEnumerable<INavigationModel> GetTrashedContentNodesByObjectType(IEnumerable<Guid> objectTypeKeys)
        => FetchNavigationDtos(objectTypeKeys, true);

    /// <inheritdoc />
    public IEnumerable<INavigationModel> GetContentNodeWithAncestors(Guid key, Guid objectTypeKey)
    {
        if (AmbientScope is null)
        {
            return [];
        }

        Sql<ISqlContext> pathSql = AmbientScope.SqlContext.Sql()
            .Select<NodeDto>(x => x.Path)
            .From<NodeDto>()
            .Where<NodeDto>(x => x.UniqueId == key && x.NodeObjectType == objectTypeKey);
        var path = AmbientScope.Database.ExecuteScalar<string?>(pathSql);
        if (string.IsNullOrEmpty(path))
        {
            return [];
        }

        var pathIds = path.Split(',').Select(int.Parse).Where(id => id > 0).ToList();
        var positionById = pathIds.Select((id, index) => (id, index)).ToDictionary(x => x.id, x => x.index);

        var nodes = new List<NavigationDto>();
        foreach (IEnumerable<int> ids in pathIds.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            // The ancestors are selected by node ID, so they can be of any object type - including ones without
            // content rows - regardless of the object type of the node the chain was requested for.
            Sql<ISqlContext> sql = SelectNavigation(mayIncludeNodesWithoutContent: true)
                .WhereIn<NodeDto>(n => n.NodeId, ids, "n");
            nodes.AddRange(AmbientScope.Database.Fetch<NavigationDto>(sql));
        }

        return nodes.OrderBy(x => positionById[x.Id]);
    }

    private IEnumerable<INavigationModel> FetchNavigationDtos(IEnumerable<Guid> objectTypeKeys, bool trashed)
    {
        if (AmbientScope is null)
        {
            return Enumerable.Empty<NavigationDto>();
        }

        Guid[] objectTypeKeysArray = objectTypeKeys.ToArray();
        if (objectTypeKeysArray.Length == 0)
        {
            return Enumerable.Empty<NavigationDto>();
        }

        Sql<ISqlContext> sql = SelectNavigation(objectTypeKeysArray.Any(ObjectTypeHasNoContent))
            .WhereIn<NodeDto>(n => n.NodeObjectType, objectTypeKeysArray, "n")
            .Where<NodeDto>(n => n.Trashed == trashed, "n")
            .OrderBy<NodeDto>(n => n.Path, "n"); // make sure that we get the parent items first

        return AmbientScope.Database.Fetch<NavigationDto>(sql);
    }

    // Containers (e.g. element containers) don't have umbracoContent rows.
    private static bool ObjectTypeHasNoContent(Guid objectTypeKey)
        => objectTypeKey == Constants.ObjectTypes.ElementContainer;

    private Sql<ISqlContext> SelectNavigation(bool mayIncludeNodesWithoutContent)
    {
        ISqlSyntaxProvider syntax = AmbientScope!.SqlContext.SqlSyntax;

        Sql<ISqlContext> sql = AmbientScope.SqlContext.Sql()
            .Select(
                $"n.{syntax.GetQuotedColumnName(NodeDto.IdColumnName)} as {syntax.GetQuotedColumnName(NodeDto.IdColumnName)}",
                $"n.{syntax.GetQuotedColumnName(NodeDto.KeyColumnName)} as {syntax.GetQuotedColumnName(NodeDto.KeyColumnName)}",
                $"ctn.{syntax.GetQuotedColumnName(NodeDto.KeyColumnName)} as {syntax.GetQuotedColumnName(NavigationDto.ContentTypeKeyColumnName)}",
                $"n.{syntax.GetQuotedColumnName(NodeDto.ParentIdColumnName)}  as  {syntax.GetQuotedColumnName(NodeDto.ParentIdColumnName)}",
                $"n.{syntax.GetQuotedColumnName(NodeDto.SortOrderColumnName)}  as  {syntax.GetQuotedColumnName(NodeDto.SortOrderColumnName)}",
                $"n.{syntax.GetQuotedColumnName(NodeDto.TrashedColumnName)}  as  {syntax.GetQuotedColumnName(NodeDto.TrashedColumnName)}")
            .From<NodeDto>("n");

        // Nodes without a content row are only returned by a LEFT JOIN. Where the query cannot match such a node,
        // an INNER JOIN gives the query optimizer more information.
        return mayIncludeNodesWithoutContent
            ? sql
                .LeftJoin<ContentDto>("c").On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId, "n", "c")
                .LeftJoin<NodeDto>("ctn").On<ContentDto, NodeDto>((c, ctn) => c.ContentTypeId == ctn.NodeId, "c", "ctn")
            : sql
                .InnerJoin<ContentDto>("c").On<NodeDto, ContentDto>((n, c) => n.NodeId == c.NodeId, "n", "c")
                .InnerJoin<NodeDto>("ctn").On<ContentDto, NodeDto>((c, ctn) => c.ContentTypeId == ctn.NodeId, "c", "ctn");
    }
}
