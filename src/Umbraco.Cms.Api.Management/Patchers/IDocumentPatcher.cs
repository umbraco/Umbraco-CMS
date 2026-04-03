using Umbraco.Cms.Api.Management.OperationStatus;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Patching;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.Patchers;

public interface IDocumentPatcher
{
    /// <summary>
    /// Applies PATCH operations to a document and returns an update model.
    /// Validates operations and returns appropriate error status if validation fails.
    /// </summary>
    /// <param name="documentKey">The document key.</param>
    /// <param name="patchModel">The patch model containing operations and affected cultures/segments.</param>
    /// <param name="userKey">The user performing the operation.</param>
    /// <returns>An attempt containing the update model or an error status.</returns>
    Task<Attempt<UpdateDocumentRequestModel, ContentPatchingOperationStatus>> ApplyPatchAsync(
        Guid documentKey,
        ContentPatchModel patchModel);
}
