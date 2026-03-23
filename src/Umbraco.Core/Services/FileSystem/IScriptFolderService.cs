using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

/// <summary>
///     Defines the contract for a service that manages script folders in the file system.
/// </summary>
public interface IScriptFolderService
{
    /// <summary>
    ///     Gets a script folder by its path.
    /// </summary>
    /// <param name="path">The path to the script folder.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the
    ///     <see cref="ScriptFolderModel"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<ScriptFolderModel?> GetAsync(string path);

    /// <summary>
    ///     Creates a new script folder.
    /// </summary>
    /// <param name="createModel">The model containing the folder creation details.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult, TStatus}"/> with the created <see cref="ScriptFolderModel"/>
    ///     on success, or a <see cref="ScriptFolderOperationStatus"/> indicating the failure reason.
    /// </returns>
    Task<Attempt<ScriptFolderModel?, ScriptFolderOperationStatus>> CreateAsync(ScriptFolderCreateModel createModel);

    /// <summary>
    ///     Deletes a script folder at the specified path.
    /// </summary>
    /// <param name="path">The path to the script folder to delete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a
    ///     <see cref="ScriptFolderOperationStatus"/> indicating the result of the operation.
    /// </returns>
    Task<ScriptFolderOperationStatus> DeleteAsync(string path);
}
