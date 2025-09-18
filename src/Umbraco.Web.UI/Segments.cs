using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace My.Site;

public class MySegmentService : ISegmentService
{
    // define a collection of segments to use
    private readonly Segment[] _segments =
    [
        new Segment { Alias = "vip-members", Name = "VIP members" },
        new Segment { Alias = "recurring-visitors", Name = "Recurring visitors" }
    ];

    public Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsAsync(int skip = 0, int take = 100)
        => Task.FromResult
        (
            Attempt.SucceedWithStatus<PagedModel<Segment>?, SegmentOperationStatus>
            (
                SegmentOperationStatus.Success,
                new PagedModel<Segment> { Total = _segments.Length, Items = _segments.Skip(skip).Take(take) }
            )
        );
}
