using Microsoft.CodeAnalysis.CSharp.Syntax;
using NPoco;
using Umbraco.Cms.Core.DynamicRoot.QuerySteps;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class DynamicRootRepository: IDynamicRootRepository
{
    private readonly IScopeAccessor _scopeAccessor;

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
    public Guid? NearestAncestorOrSelf(IEnumerable<Guid> origins, DynamicRootQueryStep filter)
    {
        Sql<ISqlContext> query = Database.SqlContext.SqlSyntax.SelectTop(
            GetAncestorOrSelfBaseQuery(origins, filter)
            .Append($"ORDER BY n.level DESC"),
            1);

        return Database.SingleOrDefault<Guid?>(query);
    }

    public Guid? FarthestAncestorOrSelf(IEnumerable<Guid> origins, DynamicRootQueryStep filter) {
        Sql<ISqlContext> query = Database.SqlContext.SqlSyntax.SelectTop(
            GetAncestorOrSelfBaseQuery(origins, filter)
                .Append($"ORDER BY n.level ASC"),
            1);

        return Database.SingleOrDefault<Guid?>(query);
    }

    private Sql<ISqlContext> GetAncestorOrSelfBaseQuery(IEnumerable<Guid> origins, DynamicRootQueryStep filter) =>
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


    public IEnumerable<Guid> NearestDescendantOrSelf(IEnumerable<Guid> origins, DynamicRootQueryStep filter)
    {
        var level = Database.Single<int>(Database.SqlContext.Sql()
            .Select("MIN(n.level)")
            .DescendantOrSelfBaseQuery(origins, filter));

        Sql<ISqlContext> query =
            Database.SqlContext.Sql()
                .Select<NodeDto>("n", n => n.UniqueId)
                .DescendantOrSelfBaseQuery(origins, filter)
                .Where<NodeDto>(n => n.Level == level, "n");

        return Database.Fetch<Guid>(query);
    }

    public IEnumerable<Guid> FarthestDescendantOrSelf(IEnumerable<Guid> origins, DynamicRootQueryStep filter)
    {

        var level = Database.Single<int>(Database.SqlContext.Sql()
            .Select("MAX(n.level)")
            .DescendantOrSelfBaseQuery(origins, filter));

        Sql<ISqlContext> query =
            Database.SqlContext.Sql()
                .Select<NodeDto>("n", n => n.UniqueId)
                .DescendantOrSelfBaseQuery(origins, filter)
                .Where<NodeDto>(n => n.Level == level, "n");

        return Database.Fetch<Guid>(query);
    }

}

internal static class HelperExtensions
{
    internal static Sql<ISqlContext> DescendantOrSelfBaseQuery(this Sql<ISqlContext> sql, IEnumerable<Guid> origins, DynamicRootQueryStep filter) =>
        //sql. Database.SqlContext.Sql().Select<NodeDto>("n", n => n.UniqueId)
        sql
            .From<NodeDto>("norigin")
            .Append( // TODO hack because npoco do not support this
                $"INNER JOIN {sql.SqlContext.SqlSyntax.GetQuotedTableName(NodeDto.TableName)} n ON {sql.SqlContext.SqlSyntax.Substring}(N.path, 1, {sql.SqlContext.SqlSyntax.Length}(norigin.path)) = norigin.path")
            .InnerJoin<ContentDto>("c")
            .On<ContentDto, NodeDto>((c, n) => c.NodeId == n.NodeId, "c", "n")
            .InnerJoin<ContentTypeDto>("ct")
            .On<ContentDto, ContentTypeDto>((c, ct) => c.ContentTypeId == ct.NodeId, "c", "ct")
            .Where<ContentTypeDto>(ct => filter.AnyOfDocTypeAlias.Contains(ct.Alias), "ct")
            .Where<NodeDto>(norigin => origins.Contains(norigin.UniqueId), "norigin");
}
