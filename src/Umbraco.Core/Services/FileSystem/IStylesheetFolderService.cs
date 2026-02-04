using Umbraco.Cms.Core.Models.FileSystem;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services.FileSystem;

/// <summary>
///     Defines the contract for a service that manages stylesheet folders in the file system.
/// </summary>
public interface IStylesheetFolderService
{
    /// <summary>
    ///     Gets a stylesheet folder by its path.
    /// </summary>
    /// <param name="path">The path to the stylesheet folder.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains the
    ///     <see cref="StylesheetFolderModel"/> if found; otherwise, <c>null</c>.
    /// </returns>
    Task<StylesheetFolderModel?> GetAsync(string path);

    /// <summary>
    ///     Creates a new stylesheet folder.
    /// </summary>
    /// <param name="createModel">The model containing the folder creation details.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains an
    ///     <see cref="Attempt{TResult, TStatus}"/> with the created <see cref="StylesheetFolderModel"/>
    ///     on success, or a <see cref="StylesheetFolderOperationStatus"/> indicating the failure reason.
    /// </returns>
    Task<Attempt<StylesheetFolderModel?, StylesheetFolderOperationStatus>> CreateAsync(StylesheetFolderCreateModel createModel);

    /// <summary>
    ///     Deletes a stylesheet folder at the specified path.
    /// </summary>
    /// <param name="path">The path to the stylesheet folder to delete.</param>
    /// <returns>
    ///     A task that represents the asynchronous operation. The task result contains a
    ///     <see cref="StylesheetFolderOperationStatus"/> indicating the result of the operation.
    /// </returns>
    Task<StylesheetFolderOperationStatus> DeleteAsync(string path);
}
