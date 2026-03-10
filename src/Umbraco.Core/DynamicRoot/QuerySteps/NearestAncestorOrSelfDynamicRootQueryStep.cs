using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.DynamicRoot.QuerySteps;

/// <summary>
///     A query step that finds the nearest (closest) ancestor or self matching the specified document type criteria.
///     This step traverses up the content tree from the origin and returns the first matching node.
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
        var result = (await _nodeFilterRepository.NearestAncestorOrSelfAsync(origins, filter)) is Guid key
            ? [key]
            : Array.Empty<Guid>();

        return Attempt<ICollection<Guid>>.Succeed(result);
    }
}
