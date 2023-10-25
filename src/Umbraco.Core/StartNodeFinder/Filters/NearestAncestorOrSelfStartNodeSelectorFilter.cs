using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.StartNodeFinder.Filters;

public class NearestAncestorOrSelfStartNodeSelectorFilter : IStartNodeSelectorFilter
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IStartNodeFilterRepository _nodeFilterRepository;

    public NearestAncestorOrSelfStartNodeSelectorFilter(ICoreScopeProvider scopeProvider, IStartNodeFilterRepository nodeFilterRepository)
    {
        _scopeProvider = scopeProvider;
        _nodeFilterRepository = nodeFilterRepository;
    }

    protected virtual string SupportedDirectionAlias { get; set; } = "NearestAncestorOrSelf";
    public bool Filter(IEnumerable<Guid> origins, StartNodeFilter filter, [MaybeNullWhen(false)] out IEnumerable<Guid> result)
    {
        if (filter.DirectionAlias != SupportedDirectionAlias || origins.Any() is false)
        {
            result = null;
            return false;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        result = _nodeFilterRepository.NearestAncestorOrSelf(origins, filter)?.Yield() ?? Array.Empty<Guid>();

        return true;
    }
}
