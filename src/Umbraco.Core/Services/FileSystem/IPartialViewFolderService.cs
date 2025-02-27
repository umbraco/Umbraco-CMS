using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

public interface IPartialViewFolderService
{
    Task<PartialViewFolderModel?> GetAsync(string path);

    Task<Attempt<PartialViewFolderModel?, PartialViewFolderOperationStatus>> CreateAsync(PartialViewFolderCreateModel createModel);

    Task<PartialViewFolderOperationStatus> DeleteAsync(string path);
}
