using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface ISegmentService
{
    /// <summary>
    ///    Gets a paged list of segments.
    /// </summary>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>The paged list of segments.</returns>
    Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsAsync(
        int skip = 0,
        int take = 100);

    /// <summary>
    ///   Gets a paged list of segments for a specific document.
    /// </summary>
    /// <param name="id">The document unique identifier.</param>
    /// <param name="skip">The number of items to skip.</param>
    /// <param name="take">The number of items to take.</param>
    /// <returns>The paged list of segments.</returns>
    [Obsolete("This method is temporary and will be removed in a future release (planned for v20). A more permanent solution will follow.")]
    Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsForDocumentAsync(
        Guid id,
        int skip = 0,
        int take = 100)
        => GetPagedSegmentsAsync(skip, take);
}
