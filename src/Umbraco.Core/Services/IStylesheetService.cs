using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IStylesheetService
{
    Task<IStylesheet?> GetAsync(string path);

    Task<Attempt<IStylesheet?, StylesheetOperationStatus>> CreateAsync(StylesheetCreateModel createModel, Guid performingUserKey);
}
