using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

public interface IScriptService : IBasicFileService<IScript>
{
    /// <summary>
    /// Creates a new script.
    /// </summary>
    /// <param name="createModel"><see cref="ScriptCreateModel"/> containing the information about the script being created.</param>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="ScriptOperationStatus"/>.</returns>
    Task<Attempt<IScript?, ScriptOperationStatus>> CreateAsync(ScriptCreateModel createModel, Guid performingUserKey);

    /// <summary>
    /// Updates an existing script.
    /// </summary>
    /// <param name="updateModel">A <see cref="ScriptUpdateModel"/> with the changes.</param>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <returns>An attempt indicating if the operation was a success as well as a more detailed <see cref="ScriptOperationStatus"/>.</returns>
    Task<Attempt<IScript?, ScriptOperationStatus>> UpdateAsync(ScriptUpdateModel updateModel, Guid performingUserKey);

    /// <summary>
    /// Deletes a Script.
    /// </summary>
    /// <param name="path">The path of the script to delete.</param>
    /// <param name="performingUserKey">The key of the user performing the operation.</param>
    /// <returns>An operation status.</returns>
    Task<ScriptOperationStatus> DeleteAsync(string path, Guid performingUserKey);
}
