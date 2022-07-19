using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Infrastructure.Services.Implement;

public class NodeCountService : INodeCountService
{
    private readonly INodeCountRepository _nodeCountRepository;
    private readonly ICoreScopeProvider _scopeProvider;

    public NodeCountService(INodeCountRepository nodeCountRepository, ICoreScopeProvider scopeProvider)
    {
        _nodeCountRepository = nodeCountRepository;
        _scopeProvider = scopeProvider;
    }

    public int GetNodeCount(Guid nodeType)
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return _nodeCountRepository.GetNodeCount(nodeType);
    }

    public int GetMediaCount()
    {
        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return _nodeCountRepository.GetMediaCount();
    }
}
