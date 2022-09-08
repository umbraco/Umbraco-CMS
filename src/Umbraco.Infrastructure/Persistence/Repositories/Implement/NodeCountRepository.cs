using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class NodeCountRepository : INodeCountRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public NodeCountRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    /// <inheritdoc />
    public int GetNodeCount(Guid nodeType)
    {
        Sql<ISqlContext>? query = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
            .SelectCount()
            .From<NodeDto>()
            .Where<NodeDto>(x => x.NodeObjectType == nodeType && x.Trashed == false);

        return _scopeAccessor.AmbientScope?.Database.ExecuteScalar<int>(query) ?? 0;
    }

    public int GetMediaCount()
    {
        Sql<ISqlContext>? query = _scopeAccessor.AmbientScope?.Database.SqlContext.Sql()
            .SelectCount()
            .From<NodeDto>()
            .InnerJoin<ContentDto>()
            .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<ContentTypeDto>()
            .On<ContentDto, ContentTypeDto>(left => left.ContentTypeId, right => right.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Media)
            .Where<NodeDto>(x => !x.Trashed)
            .Where<ContentTypeDto>(x => x.Alias != Constants.Conventions.MediaTypes.Folder);

        return _scopeAccessor.AmbientScope?.Database.ExecuteScalar<int>(query) ?? 0;
    }
}
