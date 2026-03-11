using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.OperationStatus;
using Umbraco.Cms.Api.Management.Patching;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Patching;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Patchers;

/// <summary>
/// Applies patch operations with Umbraco's custom path syntax to documents, converting them to update models.
/// </summary>
public class DocumentPatcher
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;

    public DocumentPatcher(
        IContentEditingService contentEditingService,
        IJsonSerializer jsonSerializer,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory)
    {
        _contentEditingService = contentEditingService;
        _jsonSerializer = jsonSerializer;
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
    }

    /// <summary>
    /// Applies PATCH operations to a document and returns an update model.
    /// Validates operations and returns appropriate error status if validation fails.
    /// </summary>
    /// <param name="documentKey">The document key.</param>
    /// <param name="patchModel">The patch model containing operations and affected cultures/segments.</param>
    /// <param name="userKey">The user performing the operation.</param>
    /// <returns>An attempt containing the update model or an error status.</returns>
    public async Task<Attempt<UpdateDocumentRequestModel, ContentPatchingOperationStatus>> ApplyPatchAsync(
        Guid documentKey,
        ContentPatchModel patchModel,
        Guid userKey)
    {
        // Validate operation structure
        foreach (PatchOperationModel operation in patchModel.Operations)
        {
            if (!PatchPathParser.IsValid(operation.Path))
            {
                return Attempt.FailWithStatus(ContentPatchingOperationStatus.InvalidOperation, default(UpdateDocumentRequestModel)!);
            }

            // Validate that replace/add operations have a value
            if ((operation.Op == PatchOperationType.Replace || operation.Op == PatchOperationType.Add) &&
                operation.Value is null)
            {
                return Attempt.FailWithStatus(ContentPatchingOperationStatus.InvalidOperation, default(UpdateDocumentRequestModel)!);
            }
        }

        // Load the content
        IContent? content = await _contentEditingService.GetAsync(documentKey);
        if (content is null)
        {
            return Attempt.FailWithStatus(ContentPatchingOperationStatus.NotFound, default(UpdateDocumentRequestModel)!);
        }

        // Convert to JSON as if a client would have sent the full payload
        UpdateDocumentRequestModel unModifiedUpdateModel = await _documentEditingPresentationFactory.CreateUpdateRequestModelAsync(content);
        var currentJsonString = _jsonSerializer.Serialize(unModifiedUpdateModel);

        // Apply each PATCH operation to the JSON
        foreach (PatchOperationModel operation in patchModel.Operations)
        {
            try
            {
                currentJsonString = PatchEngine.ApplyOperation(
                    currentJsonString,
                    operation.Op,
                    operation.Path,
                    operation.Value);
            }
            catch (InvalidOperationException)
            {
                // Path resolution failed or operation error
                return Attempt.FailWithStatus(
                    ContentPatchingOperationStatus.InvalidOperation,
                    default(UpdateDocumentRequestModel)!);
            }
            catch (FormatException)
            {
                // Path syntax error
                return Attempt.FailWithStatus(
                    ContentPatchingOperationStatus.InvalidOperation,
                    default(UpdateDocumentRequestModel)!);
            }
        }

        // Deserialize the modified JSON back to UpdateDocumentRequestModel
        UpdateDocumentRequestModel? modifiedUpdateModel = _jsonSerializer.Deserialize<UpdateDocumentRequestModel>(currentJsonString);

        return modifiedUpdateModel is not null
            ? Attempt.SucceedWithStatus(ContentPatchingOperationStatus.Success, modifiedUpdateModel)
            : Attempt.FailWithStatus(ContentPatchingOperationStatus.InvalidOperation, default(UpdateDocumentRequestModel)!);
    }
}
