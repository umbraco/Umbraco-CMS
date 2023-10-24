using Microsoft.CodeAnalysis.CSharp.Syntax;
using NPoco;
using Umbraco.Cms.Core.StartNodeFinder.Filters;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class StartNodeFilterRepository: IStartNodeFilterRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public StartNodeFilterRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

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
    public Guid? NearestAncestorOrSelf(IEnumerable<Guid> origins, StartNodeFilter filter)
    {
        Sql<ISqlContext> query = Database.SqlContext.SqlSyntax.SelectTop(
            GetAncestorOrSelfBaseQuery(origins, filter)
            .Append($"ORDER BY n.level DESC"),
            1);

        return Database.Single<Guid>(query);
    }

    public Guid? FarthestAncestorOrSelf(IEnumerable<Guid> origins, StartNodeFilter filter) {
        Sql<ISqlContext> query = Database.SqlContext.SqlSyntax.SelectTop(
            GetAncestorOrSelfBaseQuery(origins, filter)
                .Append($"ORDER BY n.level ASC"),
            1);

        return Database.Single<Guid>(query);
    }

    private Sql<ISqlContext> GetAncestorOrSelfBaseQuery(IEnumerable<Guid> origins, StartNodeFilter filter) =>
            Database.SqlContext.Sql()
                .Select<NodeDto>("n", n => n.UniqueId)
                .From<NodeDto>("norigin")
                .Append(            // TODO hack because npoco do not support this
                    $"INNER JOIN {Database.SqlContext.SqlSyntax.GetQuotedTableName(NodeDto.TableName)} n ON {Database.SqlContext.SqlSyntax.Substring}(norigin.path, 1, {Database.SqlContext.SqlSyntax.Length}(n.path)) = n.path")
                .InnerJoin<ContentDto>("c")
                .On<ContentDto, NodeDto>((c, n) => c.NodeId == n.NodeId, "c", "n")
                .InnerJoin<ContentTypeDto>("ct").On<ContentDto, ContentTypeDto>((c, ct) => c.ContentTypeId == ct.NodeId, "c", "ct")
                .Where<ContentTypeDto>(ct => filter.AnyOfDocTypeAlias.Contains(ct.Alias), "ct")
                .Where<NodeDto>(norigin => origins.Contains(norigin.UniqueId), "norigin");


    public IEnumerable<Guid> NearestDescendantOrSelf(IEnumerable<Guid> origins, StartNodeFilter filter)
    {
        Sql<ISqlContext> query = GetDescendantOrSelfBaseQuery(origins, filter)
            .Append($"GROUP BY n.level HAVING MIN(n.level) = n.level");

        return Database.Fetch<Guid>(query);
    }



    public IEnumerable<Guid> FarthestDescendantOrSelf(IEnumerable<Guid> origins, StartNodeFilter filter)    {
        Sql<ISqlContext> query = GetDescendantOrSelfBaseQuery(origins, filter)
            .Append($"GROUP BY n.level HAVING MAX(n.level) = n.level");

        return Database.Fetch<Guid>(query);
    }
    private Sql<ISqlContext> GetDescendantOrSelfBaseQuery(IEnumerable<Guid> origins, StartNodeFilter filter) =>
        Database.SqlContext.Sql()
            .Select<NodeDto>("n", n => n.UniqueId)
            .From<NodeDto>("norigin")
            .Append( // TODO hack because npoco do not support this
                $"INNER JOIN {Database.SqlContext.SqlSyntax.GetQuotedTableName(NodeDto.TableName)} n ON {Database.SqlContext.SqlSyntax.Substring}(N.path, 1, {Database.SqlContext.SqlSyntax.Length}(norigin.path)) = norigin.path")
            .InnerJoin<ContentDto>("c")
            .On<ContentDto, NodeDto>((c, n) => c.NodeId == n.NodeId, "c", "n")
            .InnerJoin<ContentTypeDto>("ct")
            .On<ContentDto, ContentTypeDto>((c, ct) => c.ContentTypeId == ct.NodeId, "c", "ct")
            .Where<ContentTypeDto>(ct => filter.AnyOfDocTypeAlias.Contains(ct.Alias), "ct")
            .Where<NodeDto>(norigin => origins.Contains(norigin.UniqueId), "norigin");

}
