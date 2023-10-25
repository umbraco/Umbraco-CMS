using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public class FarthestAncestorOrSelfDynamicRootQueryStep : IDynamicRootQueryStep
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDynamicRootRepository _nodeFilterRepository;

    public FarthestAncestorOrSelfDynamicRootQueryStep(ICoreScopeProvider scopeProvider, IDynamicRootRepository nodeFilterRepository)
    {
        _scopeProvider = scopeProvider;
        _nodeFilterRepository = nodeFilterRepository;
    }

    protected virtual string SupportedDirectionAlias { get; set; } = "FarthestAncestorOrSelf";
    public bool Execute(IEnumerable<Guid> origins, DynamicRootQueryStep filter, [MaybeNullWhen(false)] out IEnumerable<Guid> result)
    {
        if (filter.Alias != SupportedDirectionAlias || origins.Any() is false)
        {
            result = null;
            return false;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        result = _nodeFilterRepository.FarthestAncestorOrSelf(origins, filter)?.Yield() ?? Array.Empty<Guid>();

        return true;
    }
}
