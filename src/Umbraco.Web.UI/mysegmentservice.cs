using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace My.Site;

public class MySegmentService : ISegmentService
{
    public Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsAsync(int skip = 0, int take = 100)
        => Task.FromResult
        (
            Attempt.SucceedWithStatus<PagedModel<Segment>?, SegmentOperationStatus>
            (
                SegmentOperationStatus.Success,
                new PagedModel<Segment>
                {
                    Total = 2,
                    Items = [
                        new Segment { Alias = "s1", Name = "Segment 1" },
                        new Segment { Alias = "s2", Name = "Segment 2" },
                    ],
                }
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
