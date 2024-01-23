using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

public interface IScriptFolderService
{
    Task<ScriptFolderModel?> GetAsync(string path);

    Task<Attempt<ScriptFolderModel?, ScriptFolderOperationStatus>> CreateAsync(ScriptFolderCreateModel createModel);

    Task<ScriptFolderOperationStatus> DeleteAsync(string path);
}
