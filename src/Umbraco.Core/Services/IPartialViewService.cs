using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IPartialViewService : IService
{
    Task<PagedModel<PartialViewSnippet>> GetPartialViewSnippetsAsync(int skip, int take);

    Task<Attempt<IPartialView?, PartialViewOperationStatus>> CreateAsync(PartialViewCreateModel createModel, Guid performingUserKey);
}
