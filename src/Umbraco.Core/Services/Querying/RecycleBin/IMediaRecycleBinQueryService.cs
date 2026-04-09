using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services.Querying.RecycleBin;

/// <summary>
/// Defines a service for querying media items in the recycle bin.
/// </summary>
public interface IMediaRecycleBinQueryService
{
    /// <summary>
    /// Gets the original parent of a trashed media item.
    /// </summary>
    /// <param name="trashedMediaId">The unique identifier of the trashed media item.</param>
    /// <returns>
    /// An <see cref="Attempt{TResult,TStatus}"/> containing the original parent as an <see cref="IMediaEntitySlim"/>
    /// if found, or <c>null</c> if the original parent was the root. The status indicates the result of the operation.
    /// </returns>
    Task<Attempt<IMediaEntitySlim?, RecycleBinQueryResultType>> GetOriginalParentAsync(Guid trashedMediaId);
}
