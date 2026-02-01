using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.OperationStatus;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.PropertyEditors.JsonPath;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Patchers;

/// <summary>
/// Applies JSON Patch operations with JSONPath to documents, converting them to update models.
/// </summary>
public class DocumentPatcher
{
    private readonly IContentEditingService _contentEditingService;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILanguageService _languageService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IDocumentEditingPresentationFactory _documentEditingPresentationFactory;
    private readonly JsonPathEvaluator _jsonPathEvaluator;
    private readonly JsonPathCultureExtractor _cultureExtractor;

    public DocumentPatcher(
        IContentEditingService contentEditingService,
        IContentTypeService contentTypeService,
        ILanguageService languageService,
        IJsonSerializer jsonSerializer,
        IDocumentEditingPresentationFactory documentEditingPresentationFactory)
    {
        _contentEditingService = contentEditingService;
        _contentTypeService = contentTypeService;
        _languageService = languageService;
        _jsonSerializer = jsonSerializer;
        _documentEditingPresentationFactory = documentEditingPresentationFactory;
        _jsonPathEvaluator = new JsonPathEvaluator();
        _cultureExtractor = new JsonPathCultureExtractor();
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
            if (!_jsonPathEvaluator.IsValidExpression(operation.Path))
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
                currentJsonString = _jsonPathEvaluator.ApplyOperation(
                    currentJsonString,
                    operation.Op,
                    operation.Path,
                    operation.Value);
            }
            catch (InvalidOperationException)
            {
                // JSONPath matched no elements or other operation error
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
