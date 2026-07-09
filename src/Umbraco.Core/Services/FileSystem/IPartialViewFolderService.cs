using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

/// <summary>
///     Defines the contract for a service that manages partial view folders in the file system.
/// </summary>
public interface IPartialViewFolderService
{
    /// <summary>
    ///     Gets a partial view folder by its path.
    /// </summary>
    /// <param name="path">The path to the partial view folder.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the
    ///     <see cref="PartialViewFolderModel"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<PartialViewFolderModel?> GetAsync(string path);

    /// <summary>
    ///     Creates a new partial view folder.
    /// </summary>
    /// <param name="createModel">The model containing the folder creation details.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult, TStatus}"/> with the created <see cref="PartialViewFolderModel"/>
    ///     on success, or a <see cref="PartialViewFolderOperationStatus"/> indicating the failure reason.
    /// </returns>
    Task<Attempt<PartialViewFolderModel?, PartialViewFolderOperationStatus>> CreateAsync(PartialViewFolderCreateModel createModel);

    /// <summary>
    ///     Deletes a partial view folder at the specified path.
    /// </summary>
    /// <param name="path">The path to the partial view folder to delete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a
    ///     <see cref="PartialViewFolderOperationStatus"/> indicating the result of the operation.
    /// </returns>
    Task<PartialViewFolderOperationStatus> DeleteAsync(string path);
}
