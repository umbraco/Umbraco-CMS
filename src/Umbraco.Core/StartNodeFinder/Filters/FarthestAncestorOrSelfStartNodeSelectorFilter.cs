using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.StartNodeFinder.Filters;

public class FarthestAncestorOrSelfStartNodeSelectorFilter : IStartNodeSelectorFilter
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IStartNodeFilterRepository _nodeFilterRepository;

    public FarthestAncestorOrSelfStartNodeSelectorFilter(ICoreScopeProvider scopeProvider, IStartNodeFilterRepository nodeFilterRepository)
    {
        _scopeProvider = scopeProvider;
        _nodeFilterRepository = nodeFilterRepository;
    }

    protected virtual string SupportedDirectionAlias { get; set; } = "FarthestAncestorOrSelf";
    public IEnumerable<Guid>? Filter(IEnumerable<Guid> origins, StartNodeFilter filter)
    {
        if (filter.DirectionAlias != SupportedDirectionAlias || origins.Any() is false)
        {
            return null;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return _nodeFilterRepository.FarthestAncestorOrSelf(origins, filter)?.Yield();
    }
}
