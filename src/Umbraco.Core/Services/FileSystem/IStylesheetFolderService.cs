using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

public interface IStylesheetFolderService
{
    Task<StylesheetFolderModel?> GetAsync(string path);

    Task<Attempt<StylesheetFolderModel?, StylesheetFolderOperationStatus>> CreateAsync(StylesheetFolderCreateModel createModel);

    Task<StylesheetFolderOperationStatus> DeleteAsync(string path);
}
