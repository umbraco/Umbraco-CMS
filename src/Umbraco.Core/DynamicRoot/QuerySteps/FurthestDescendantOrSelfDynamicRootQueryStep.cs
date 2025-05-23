using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public class FurthestDescendantOrSelfDynamicRootQueryStep : IDynamicRootQueryStep
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDynamicRootRepository _nodeFilterRepository;

    public FurthestDescendantOrSelfDynamicRootQueryStep(ICoreScopeProvider scopeProvider, IDynamicRootRepository nodeFilterRepository)
    {
        _scopeProvider = scopeProvider;
        _nodeFilterRepository = nodeFilterRepository;
    }

    public virtual string SupportedDirectionAlias { get; set; } = "FurthestDescendantOrSelf";

    public async Task<Attempt<ICollection<Guid>>> ExecuteAsync(ICollection<Guid> origins, DynamicRootQueryStep filter)
    {
        if (filter.Alias != SupportedDirectionAlias)
        {
            return Attempt<ICollection<Guid>>.Fail();
        }

        if (origins.Count < 1)
        {
            return Attempt<ICollection<Guid>>.Succeed(Array.Empty<Guid>());
        }

        using ICoreScope scope = _scopeProvider.CreateCoreScope(autoComplete: true);
        var result = await _nodeFilterRepository.FurthestDescendantOrSelfAsync(origins, filter);

        return Attempt<ICollection<Guid>>.Succeed(result);
    }
}
