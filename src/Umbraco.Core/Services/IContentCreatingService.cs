using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentCreatingService
{
    Task<Attempt<PagedModel<IContentType>?, ContentCreatingOperationStatus>> GetAllowedChildrenContentTypesAsync(Guid key, int skip, int take);
}
