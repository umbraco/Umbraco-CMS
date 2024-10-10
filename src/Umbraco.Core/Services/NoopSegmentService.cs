using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public class NoopSegmentService : ISegmentService
{
    public async Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsAsync(int skip = 0, int take = 100)
    {
        return await Task.FromResult(Attempt.SucceedWithStatus<PagedModel<Segment>?, SegmentOperationStatus>(
            SegmentOperationStatus.Success,
            new PagedModel<Segment> { Total = 0, Items = Enumerable.Empty<Segment>() }));
    }
}
