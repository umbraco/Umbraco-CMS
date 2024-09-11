using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IScriptService : IBasicFileService<IScript>
{
    /// <summary>
    /// Creates a new script.
    /// </summary>
    /// <param name="createModel"><see cref="ScriptCreateModel"/> containing the information about the script being created.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="ScriptOperationStatus"/>.</returns>
    Task<Attempt<IScript?, ScriptOperationStatus>> CreateAsync(ScriptCreateModel createModel, Guid userKey);

    /// <summary>
    /// Updates an existing script.
    /// </summary>
    /// <param name="path">The path of the script to update.</param>
    /// <param name="updateModel">A <see cref="ScriptUpdateModel"/> with the changes.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="ScriptOperationStatus"/>.</returns>
    Task<Attempt<IScript?, ScriptOperationStatus>> UpdateAsync(string path, ScriptUpdateModel updateModel, Guid userKey);

    /// <summary>
    /// Deletes a Script.
    /// </summary>
    /// <param name="path">The path of the script to delete.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An operation status.</returns>
    Task<ScriptOperationStatus> DeleteAsync(string path, Guid userKey);

    /// <summary>
    /// Renames a script.
    /// </summary>
    /// <param name="path">The path of the script to rename.</param>
    /// <param name="renameModel">A <see cref="ScriptRenameModel"/> with the changes.</param>
    /// <param name="userKey">The key of the user performing the operation.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="ScriptOperationStatus"/>.</returns>
    Task<Attempt<IScript?, ScriptOperationStatus>> RenameAsync(string path, ScriptRenameModel renameModel, Guid userKey);
}
