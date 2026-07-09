using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services.Querying.RecycleBin;

/// <summary>
/// Defines a service for querying documents (content items) in the recycle bin.
/// </summary>
public interface IDocumentRecycleBinQueryService
{
    /// <summary>
    /// Gets the original parent of a trashed document.
    /// </summary>
    /// <param name="trashedDocumentId">The unique identifier of the trashed document.</param>
    /// <returns>
    /// An <see cref="Attempt{TResult,TStatus}"/> containing the original parent as an <see cref="IDocumentEntitySlim"/>
    /// if found, or <c>null</c> if the original parent was the root. The status indicates the result of the operation.
    /// </returns>
    Task<Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType>> GetOriginalParentAsync(Guid trashedDocumentId);
}
