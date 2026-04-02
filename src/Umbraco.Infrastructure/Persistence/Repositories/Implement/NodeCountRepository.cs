using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
/// Repository responsible for managing and persisting node count data within the Umbraco CMS.
/// Provides methods for retrieving and updating the count of content nodes.
/// </summary>
public class NodeCountRepository : INodeCountRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="NodeCountRepository"/> class with the specified scope accessor.
    /// </summary>
    /// <param name="scopeAccessor">Provides access to the current database scope for repository operations.</param>
    public NodeCountRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    /// <inheritdoc />
    public int GetNodeCount(Guid nodeType)
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return 0;
        }

        Sql<ISqlContext> query = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
            .SelectCount()
            .From<NodeDto>()
            .Where<NodeDto>(x => x.NodeObjectType == nodeType && x.Trashed == false);

        return _scopeAccessor.AmbientScope.Database.ExecuteScalar<int>(query);
    }

    /// <summary>
    /// Gets the count of media nodes that are not trashed and are not of the folder media type.
    /// </summary>
    /// <returns>
    /// The total number of media nodes that are not in the recycle bin and are not folders.
    /// </returns>
    public int GetMediaCount()
    {
        if (_scopeAccessor.AmbientScope is null)
        {
            return 0;
        }

        Sql<ISqlContext> query = _scopeAccessor.AmbientScope.Database.SqlContext.Sql()
            .SelectCount()
            .From<NodeDto>()
            .InnerJoin<ContentDto>()
            .On<NodeDto, ContentDto>(left => left.NodeId, right => right.NodeId)
            .InnerJoin<ContentTypeDto>()
            .On<ContentDto, ContentTypeDto>(left => left.ContentTypeId, right => right.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == Constants.ObjectTypes.Media)
            .Where<NodeDto>(x => !x.Trashed)
            .Where<ContentTypeDto>(x => x.Alias != Constants.Conventions.MediaTypes.Folder);

        return _scopeAccessor.AmbientScope.Database.ExecuteScalar<int>(query);
    }
}
