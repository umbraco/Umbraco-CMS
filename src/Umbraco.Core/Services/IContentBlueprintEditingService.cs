using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IContentBlueprintEditingService
{
    Task<IContent?> GetAsync(Guid key);

    Task<Attempt<IContent?, ContentEditingOperationStatus>> DeleteAsync(Guid key, Guid userKey);
}
