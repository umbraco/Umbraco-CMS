using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface ISegmentService
{
    Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsAsync(int skip = 0, int take = 100);
}
