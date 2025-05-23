using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IStylesheetService : IBasicFileService<IStylesheet>
{
    /// <summary>
    /// Creates a new stylesheet.
    /// </summary>
    /// <param name="createModel"><see cref="StylesheetCreateModel"/> containing the information about the stylesheet being created.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="StylesheetOperationStatus"/>.</returns>
    Task<Attempt<IStylesheet?, StylesheetOperationStatus>> CreateAsync(StylesheetCreateModel createModel, Guid userKey);

    /// <summary>
    /// Updates an existing stylesheet.
    /// </summary>
    /// <param name="path">The path of the stylesheet to update.</param>
    /// <param name="updateModel">A <see cref="StylesheetUpdateModel"/> with the changes.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="StylesheetOperationStatus"/>.</returns>
    Task<Attempt<IStylesheet?, StylesheetOperationStatus>> UpdateAsync(string path, StylesheetUpdateModel updateModel, Guid userKey);

    /// <summary>
    /// Deletes a stylesheet.
    /// </summary>
    /// <param name="path">The path of the stylesheet to delete.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An operation status.</returns>
    Task<StylesheetOperationStatus> DeleteAsync(string path, Guid userKey);

    /// <summary>
    /// Renames a stylesheet.
    /// </summary>
    /// <param name="path">The path of the stylesheet to rename.</param>
    /// <param name="renameModel">A <see cref="StylesheetRenameModel"/> with the changes.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="StylesheetOperationStatus"/>.</returns>
    Task<Attempt<IStylesheet?, StylesheetOperationStatus>> RenameAsync(string path, StylesheetRenameModel renameModel, Guid userKey);
}
