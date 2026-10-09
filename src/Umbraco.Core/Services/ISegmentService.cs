using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides operations for managing content segments.
/// </summary>
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
}
