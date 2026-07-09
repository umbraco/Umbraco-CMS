using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Api.Management.Services.PermissionFilter;

/// <summary>
/// Service for filtering document entities based on user browse permissions.
/// </summary>
public interface IDocumentPermissionFilterService
{
    /// <summary>
    /// Filters document entities based on the current user's browse permissions.
    /// </summary>
    /// <param name="entities">The entities to filter.</param>
    /// <param name="totalItems">The total number of items before filtering.</param>
    /// <returns>A tuple containing the filtered entities and the adjusted total items count.</returns>
    Task<(IEntitySlim[] Entities, long TotalItems)> FilterAsync(IEntitySlim[] entities, long totalItems);

    /// <summary>
    /// Filters sibling document entities based on the current user's browse permissions.
    /// </summary>
    /// <param name="targetKey">The key of the target entity around which siblings are being retrieved.</param>
    /// <param name="entities">The entities to filter.</param>
    /// <param name="totalBefore">The total number of siblings before the target entity.</param>
    /// <param name="totalAfter">The total number of siblings after the target entity.</param>
    /// <returns>A tuple containing the filtered entities and the adjusted before/after counts.</returns>
    Task<(IEntitySlim[] Entities, long TotalBefore, long TotalAfter)> FilterAsync(Guid targetKey, IEntitySlim[] entities, long totalBefore, long totalAfter);
}
