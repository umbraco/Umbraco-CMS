using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     A no-operation implementation of <see cref="ISegmentService" /> that returns empty results.
///     Used as a default when no segment provider is configured.
/// </summary>
public class NoopSegmentService : ISegmentService
{
    /// <inheritdoc />
    public Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsAsync(int skip = 0, int take = 100)
    {
        return Task.FromResult(Attempt.SucceedWithStatus<PagedModel<Segment>?, SegmentOperationStatus>(
            SegmentOperationStatus.Success,
            new PagedModel<Segment> { Total = 0, Items = Enumerable.Empty<Segment>() }));
    }
}
