using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <summary>
/// Base class for entity access filters that gate server event delivery by the recipient's
/// start-node access to the entity's tree path. Only entity types that have a start node
/// use this base (in practice documents and media), and every such type has a recycle bin.
/// Sources without a start node implement <see cref="IServerEventAccessFilter"/>
/// directly instead.
/// </summary>
public abstract class EntityStartNodeAccessFilter : IServerEventAccessFilter
{
    private readonly IEntityService _entityService;
    private readonly AppCaches _appCaches;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityStartNodeAccessFilter"/> class.
    /// </summary>
    /// <param name="entityService">The entity service used when calculating start nodes.</param>
    /// <param name="appCaches">The application caches backing the start-node calculation.</param>
    protected EntityStartNodeAccessFilter(IEntityService entityService, AppCaches appCaches)
    {
        _entityService = entityService;
        _appCaches = appCaches;
    }

    /// <inheritdoc />
    public abstract IEnumerable<string> FilteredEventSources { get; }

    /// <summary>
    /// Gets the recycle bin identifier for the entity type being filtered.
    /// </summary>
    protected abstract int RecycleBinId { get; }

    /// <summary>
    /// Calculates the start node identifiers for the user for the entity type being filtered.
    /// </summary>
    /// <param name="user">The user to calculate start nodes for.</param>
    /// <param name="entityService">The entity service.</param>
    /// <param name="appCaches">The application caches.</param>
    /// <returns>The start node identifiers, or <c>null</c> if the user has none.</returns>
    protected abstract int[]? CalculateStartNodeIds(IUser user, IEntityService entityService, AppCaches appCaches);

    /// <inheritdoc />
    public Task<bool> HasAccessAsync(IUser user, ServerEventRoutingContext context)
    {
        // Start-node access can only be evaluated with a path; without one, fail closed.
        // (Whitespace is treated as missing too, as HasPathAccess throws on a whitespace path.)
        if (string.IsNullOrWhiteSpace(context.EntityPath))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(ContentPermissions.HasPathAccess(
            context.EntityPath,
            CalculateStartNodeIds(user, _entityService, _appCaches),
            RecycleBinId));
    }
}
