using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Web.UI;

public class SegmentService : ISegmentService
{
  private static readonly Segment[] segments = new Segment[] {
        new Segment
        {
            Alias = "segment-one",
            Name = "First Segment"
        },
        new Segment
        {
            Alias = "segment-two",
            Name = "Second Segment"
        },
        new Segment
        {
            Alias = "segment-three",
            Name = "Thrird Segment"
        },
    };

  public async Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsAsync(int skip = 0, int take = 100)
  {
    return await Task.FromResult(Attempt.SucceedWithStatus<PagedModel<Segment>?, SegmentOperationStatus>(
        SegmentOperationStatus.Success,
        new PagedModel<Segment> { Total = segments.Length, Items = segments.Skip(0).Take(take) }));
  }
}

public class SegmentServiceOverrideComposer : IComposer
{
  public void Compose(IUmbracoBuilder builder) => builder.Services.AddUnique<ISegmentService, SegmentService>();
}