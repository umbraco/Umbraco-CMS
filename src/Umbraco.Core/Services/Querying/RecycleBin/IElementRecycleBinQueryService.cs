using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services.Querying.RecycleBin;

/// <summary>
/// Service for querying the Element recycle bin.
/// </summary>
public interface IElementRecycleBinQueryService
{
    /// <summary>
    /// Gets the original parent container of a trashed element.
    /// </summary>
    /// <param name="trashedElementId">The key of the trashed element.</param>
    /// <returns>
    /// An attempt containing the original parent entity if successful,
    /// or a failure status indicating why the parent could not be retrieved.
    /// </returns>
    Task<Attempt<IEntitySlim?, RecycleBinQueryResultType>> GetOriginalParentAsync(Guid trashedElementId);

    /// <summary>
    /// Gets the original parent container of a trashed element container.
    /// </summary>
    /// <param name="trashedElementContainerId">The key of the trashed element container.</param>
    /// <returns>
    /// An attempt containing the original parent entity if successful,
    /// or a failure status indicating why the parent could not be retrieved.
    /// </returns>
    Task<Attempt<IEntitySlim?, RecycleBinQueryResultType>> GetOriginalParentForContainerAsync(Guid trashedElementContainerId);
}