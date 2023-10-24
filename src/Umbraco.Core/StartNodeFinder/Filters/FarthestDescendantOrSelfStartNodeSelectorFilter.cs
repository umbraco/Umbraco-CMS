using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.StartNodeFinder.Filters;

public class FarthestDescendantOrSelfStartNodeSelectorFilter : IStartNodeSelectorFilter
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IStartNodeFilterRepository _nodeFilterRepository;

    public FarthestDescendantOrSelfStartNodeSelectorFilter(ICoreScopeProvider scopeProvider, IStartNodeFilterRepository nodeFilterRepository)
    {
        _scopeProvider = scopeProvider;
        _nodeFilterRepository = nodeFilterRepository;
    }

    protected virtual string SupportedDirectionAlias { get; set; } = "FarthestDescendantOrSelf";
    public IEnumerable<Guid>? Filter(IEnumerable<Guid> origins, StartNodeFilter filter)
    {
        if (filter.DirectionAlias != SupportedDirectionAlias || origins.Any() is false)
        {
            return null;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        return _nodeFilterRepository.FarthestDescendantOrSelf(origins, filter);
    }
}
