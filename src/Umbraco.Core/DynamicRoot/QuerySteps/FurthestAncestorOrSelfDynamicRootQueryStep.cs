using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

public class FurthestAncestorOrSelfDynamicRootQueryStep : IDynamicRootQueryStep
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDynamicRootRepository _nodeFilterRepository;

    public FurthestAncestorOrSelfDynamicRootQueryStep(ICoreScopeProvider scopeProvider, IDynamicRootRepository nodeFilterRepository)
    {
        _scopeProvider = scopeProvider;
        _nodeFilterRepository = nodeFilterRepository;
    }

    protected virtual string SupportedDirectionAlias { get; set; } = "FurthestAncestorOrSelf";

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
        var result = (await _nodeFilterRepository.FurthestAncestorOrSelfAsync(origins, filter))?.ToSingleItemCollection() ?? Array.Empty<Guid>();

        return Attempt<ICollection<Guid>>.Succeed(result);
    }
}
