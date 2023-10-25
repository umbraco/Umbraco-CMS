using System.Diagnostics.CodeAnalysis;
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
    public bool Filter(IEnumerable<Guid> origins, StartNodeFilter filter, [MaybeNullWhen(false)] out IEnumerable<Guid> result)
    {
        if (filter.DirectionAlias != SupportedDirectionAlias || origins.Any() is false)
        {
            result = null;
            return false;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        result = _nodeFilterRepository.FarthestDescendantOrSelf(origins, filter);

        return true;
    }
}
