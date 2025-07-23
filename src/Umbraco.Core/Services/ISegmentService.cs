using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface ISegmentService
{
    [Obsolete("Use the version of GetPagedSegmentsAsync accepting all parameters instead. Scheduled for removal in V18")]
    Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsAsync(int skip = 0, int take = 100);

    Task<Attempt<PagedModel<Segment>?, SegmentOperationStatus>> GetPagedSegmentsAsync(int skip = 0, int take = 100, Guid? documentId = null, Guid? documentTypeId = null)
        =>  GetPagedSegmentsAsync(skip, take);
}
