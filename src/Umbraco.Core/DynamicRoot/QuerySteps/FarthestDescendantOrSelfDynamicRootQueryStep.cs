using System.Diagnostics.CodeAnalysis;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public class FarthestDescendantOrSelfDynamicRootQueryStep : IDynamicRootQueryStep
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDynamicRootRepository _nodeFilterRepository;

    public FarthestDescendantOrSelfDynamicRootQueryStep(ICoreScopeProvider scopeProvider, IDynamicRootRepository nodeFilterRepository)
    {
        _scopeProvider = scopeProvider;
        _nodeFilterRepository = nodeFilterRepository;
    }

    protected virtual string SupportedDirectionAlias { get; set; } = "FarthestDescendantOrSelf";
    public bool Execute(IEnumerable<Guid> origins, DynamicRootQueryStep filter, [MaybeNullWhen(false)] out IEnumerable<Guid> result)
    {
        if (filter.Alias != SupportedDirectionAlias || origins.Any() is false)
        {
            result = null;
            return false;
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        result = _nodeFilterRepository.FarthestDescendantOrSelf(origins, filter);

        return true;
    }
}
