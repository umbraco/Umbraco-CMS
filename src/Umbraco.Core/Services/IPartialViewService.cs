using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Snippets;
using PartialViewSnippet = Umbraco.Cms.Core.Snippets.PartialViewSnippet;

namespace Umbraco.Cms.Core.Services;

public interface IPartialViewService : IBasicFileService<IPartialView>
{
    /// <summary>
    /// Deletes a partial view.
    /// </summary>
    /// <param name="path">The path of the partial view to delete.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An operation status.</returns>
    Task<PartialViewOperationStatus> DeleteAsync(string path, Guid userKey);

    /// <summary>
    /// Gets all the available partial view snippets.
    /// </summary>
    /// <param name="skip">Amount to skip.</param>
    /// <param name="take">Amount to take.</param>
    /// <returns></returns>
    Task<PagedModel<PartialViewSnippetSlim>> GetSnippetsAsync(int skip, int take);

    /// <summary>
    /// Gets a partial view snippet by ID, returns null if not found.
    /// </summary>
    /// <param name="id">The name of the snippet to get.</param>
    /// <returns>The partial view snippet, null if not found.</returns>
    Task<PartialViewSnippet?> GetSnippetAsync(string id);

    /// <summary>
    /// Creates a new partial view.
    /// </summary>
    /// <param name="createModel"><see cref="PartialViewCreateModel"/> containing the information about the partial view being created.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="PartialViewOperationStatus"/>.</returns>
    Task<Attempt<IPartialView?, PartialViewOperationStatus>> CreateAsync(PartialViewCreateModel createModel, Guid userKey);

    /// <summary>
    /// Updates an existing partial view.
    /// </summary>
    /// <param name="path">The path of the partial view to update.</param>
    /// <param name="updateModel">A <see cref="PartialViewUpdateModel"/> with the changes.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="PartialViewOperationStatus"/>.</returns>
    Task<Attempt<IPartialView?, PartialViewOperationStatus>> UpdateAsync(string path, PartialViewUpdateModel updateModel, Guid userKey);

    /// <summary>
    /// Renames a partial view.
    /// </summary>
    /// <param name="path">The path of the partial view to rename.</param>
    /// <param name="renameModel">A <see cref="PartialViewRenameModel"/> with the changes.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="PartialViewOperationStatus"/>.</returns>
    Task<Attempt<IPartialView?, PartialViewOperationStatus>> RenameAsync(string path, PartialViewRenameModel renameModel, Guid userKey);
}
