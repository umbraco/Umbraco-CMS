using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IStylesheetService
{
    Task<IStylesheet?> GetAsync(string path);

    Task<IEnumerable<IStylesheet>> GetAllAsync(params string[] paths);

    Task<Attempt<IStylesheet?, StylesheetOperationStatus>> CreateAsync(StylesheetCreateModel createModel, Guid performingUserKey);

    Task<Attempt<IStylesheet?, StylesheetOperationStatus>> UpdateAsync(StylesheetUpdateModel updateModel, Guid performingUserKey);

    Task<StylesheetOperationStatus> DeleteAsync(string path, Guid performingUserKey);
}
