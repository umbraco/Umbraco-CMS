using Umbraco.Cms.Core.Models;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IPartialViewService
{
    Task<PagedModel<PartialViewSnippet>> GetPartialViewSnippetsAsync(int skip, int take);
}
