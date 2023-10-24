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
    public IEnumerable<Guid> NearestAncestorOrSelf(IEnumerable<Guid> keys, StartNodeFilter filter)
    {
        Sql<ISqlContext> query = Database.SqlContext.SqlSyntax.SelectTop(
            Database.SqlContext.Sql()
            .Select<NodeDto>("n", n => n.UniqueId)
            .From<NodeDto>("norigin")
            .Append(            // TODO hack because npoco do not support this
                $"INNER JOIN {Database.SqlContext.SqlSyntax.GetQuotedTableName(NodeDto.TableName)} n ON {Database.SqlContext.SqlSyntax.Substring}(norigin.path, 1, {Database.SqlContext.SqlSyntax.Length}(n.path)) = n.path")
            .InnerJoin<ContentDto>("c")
            .On<ContentDto, NodeDto>((c, n) => c.NodeId == n.NodeId, "c", "n")
            .InnerJoin<ContentTypeDto>("ct").On<ContentDto, ContentTypeDto>((c, ct) => c.ContentTypeId == ct.NodeId, "c", "ct")
            .Where<ContentTypeDto>(ct => filter.AnyOfDocTypeAlias.Contains(ct.Alias), "ct")
            .Where<NodeDto>(norigin => keys.Contains(norigin.UniqueId), "norigin")
            .Append($"ORDER BY n.level DESC"), 1)
            ;

        return Database.Fetch<Guid>(query);
    }
}
