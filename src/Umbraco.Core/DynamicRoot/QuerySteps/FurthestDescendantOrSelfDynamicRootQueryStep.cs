using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

/// <summary>
///     A query step that finds the furthest (deepest) descendants or self matching the specified document type criteria.
///     This step traverses down the content tree from the origin and returns all matching nodes at the greatest depth.
/// </summary>
public class FurthestDescendantOrSelfDynamicRootQueryStep : IDynamicRootQueryStep
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDynamicRootRepository _nodeFilterRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FurthestDescendantOrSelfDynamicRootQueryStep"/> class.
    /// </summary>
    /// <param name="scopeProvider">The scope provider for database operations.</param>
    /// <param name="nodeFilterRepository">The repository used to query for descendants matching the filter criteria.</param>
    public FurthestDescendantOrSelfDynamicRootQueryStep(ICoreScopeProvider scopeProvider, IDynamicRootRepository nodeFilterRepository)
    {
        _scopeProvider = scopeProvider;
        _nodeFilterRepository = nodeFilterRepository;
    }

    /// <inheritdoc/>
    public virtual string SupportedDirectionAlias { get; set; } = "FurthestDescendantOrSelf";

    /// <inheritdoc/>
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
