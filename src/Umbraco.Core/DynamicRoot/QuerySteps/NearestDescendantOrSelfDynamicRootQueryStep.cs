using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public class NearestDescendantOrSelfDynamicRootQueryStep : IDynamicRootQueryStep
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDynamicRootRepository _nodeFilterRepository;

    public NearestDescendantOrSelfDynamicRootQueryStep(ICoreScopeProvider scopeProvider, IDynamicRootRepository nodeFilterRepository)
    {
        _scopeProvider = scopeProvider;
        _nodeFilterRepository = nodeFilterRepository;
    }

    protected virtual string SupportedDirectionAlias { get; set; } = "NearestDescendantOrSelf";

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
        var result = await _nodeFilterRepository.NearestDescendantOrSelfAsync(origins, filter);

        return Attempt<ICollection<Guid>>.Succeed(result);
    }
}
