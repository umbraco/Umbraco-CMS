using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

/// <summary>
///     A query step that finds the nearest (closest) ancestors or self matching the specified document type criteria.
///     This step traverses up the content tree from each origin and returns the closest matching nodes, one for each
///     origin.
/// </summary>
public class NearestAncestorOrSelfDynamicRootQueryStep : IDynamicRootQueryStep
{
    private readonly ICoreScopeProvider _scopeProvider;
    private readonly IDynamicRootRepository _nodeFilterRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="NearestAncestorOrSelfDynamicRootQueryStep"/> class.
    /// </summary>
    /// <param name="scopeProvider">The scope provider for database operations.</param>
    /// <param name="nodeFilterRepository">The repository used to query for ancestors matching the filter criteria.</param>
    public NearestAncestorOrSelfDynamicRootQueryStep(ICoreScopeProvider scopeProvider, IDynamicRootRepository nodeFilterRepository)
    {
        _scopeProvider = scopeProvider;
        _nodeFilterRepository = nodeFilterRepository;
    }

    /// <inheritdoc/>
    public virtual string SupportedDirectionAlias { get; set; } = "NearestAncestorOrSelf";

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
        ICollection<Guid> result = await _nodeFilterRepository.NearestAncestorsOrSelfAsync(origins, filter);

        return Attempt<ICollection<Guid>>.Succeed(result);
    }
}
