using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public class NearestAncestorOrSelfDynamicRootQueryStep : IDynamicRootQueryStep
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDynamicRootRepository _nodeFilterRepository;

    public NearestAncestorOrSelfDynamicRootQueryStep(ICoreScopeProvider scopeProvider, IDynamicRootRepository nodeFilterRepository)
    {
        _scopeProvider = scopeProvider;
        _nodeFilterRepository = nodeFilterRepository;
    }

    public virtual string SupportedDirectionAlias { get; set; } = "NearestAncestorOrSelf";

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
        var result = (await _nodeFilterRepository.NearestAncestorOrSelfAsync(origins, filter)) is Guid key
            ? [key]
            : Array.Empty<Guid>();

        return Attempt<ICollection<Guid>>.Succeed(result);
    }
}
