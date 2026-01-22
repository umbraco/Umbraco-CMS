using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

public class MySegmentService : ISegmentService
{
    // define a collection of segments to use
    private readonly Segment[] _segments =
    [
        new Segment { Alias = "vip-members", Name = "VIP members" }
    ];

    public Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsAsync(int skip = 0, int take = 100) => Task.FromResult
    (
        Attempt.SucceedWithStatus<PagedModel<Segment>?, SegmentOperationStatus>
        (
            SegmentOperationStatus.Success,
            new PagedModel<Segment> { Total = _segments.Length, Items = _segments.Skip(skip).Take(take) }
        )
    );
}

public class MySegmentComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // register the custom segment service in place of the Umbraco core implementation
        builder.Services.AddUnique<ISegmentService, MySegmentService>();

        // update segment configuration so segments are enabled (in the client)
        builder.Services.Configure<SegmentSettings>(settings => settings.Enabled = true);
    }
}
