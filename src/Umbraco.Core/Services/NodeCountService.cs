using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

/// <summary>
///     Provides services for counting nodes in the Umbraco content tree.
/// </summary>
/// <remarks>
///     This service is typically used for dashboard statistics, licensing checks,
///     and other scenarios where total counts of content, media, or other node types are needed.
/// </remarks>
public class NodeCountService : INodeCountService
{
    private readonly INodeCountRepository _nodeCountRepository;
    private readonly ICoreScopeProvider _scopeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NodeCountService" /> class.
    /// </summary>
    /// <param name="nodeCountRepository">The repository for node count operations.</param>
    /// <param name="scopeProvider">The core scope provider for database operations.</param>
    public NodeCountService(INodeCountRepository nodeCountRepository, ICoreScopeProvider scopeProvider)
    {
        _nodeCountRepository = nodeCountRepository;
        _scopeProvider = scopeProvider;
    }

    /// <inheritdoc />
    public int GetNodeCount(Guid nodeType)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return _nodeCountRepository.GetNodeCount(nodeType);
    }

    /// <inheritdoc />
    public int GetMediaCount()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return _nodeCountRepository.GetMediaCount();
    }
}
