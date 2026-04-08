using System.Text.Json;
using System.Text.Json.Nodes;
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
/// Applies patch operations with Umbraco's custom path syntax to document update models.
/// </summary>
public class DocumentPatcher : IDocumentPatcher
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

    /// <inheritdoc />
    public async Task<Attempt<UpdateDocumentRequestModel, ContentPatchingOperationStatus>> ApplyPatchAsync(
        Guid documentKey,
        ContentPatchModel patchModel)
    {
        // Validate operation structure
        foreach (PatchOperationModel operation in patchModel.Operations)
        {
            if (PatchPathParser.IsValid(operation.Path, out _) is false)
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

        if (string.IsNullOrWhiteSpace(currentJsonString))
        {
            // should not happen as the content exists.
            throw new JsonException("Unexpected empty JSON string when building update model for patching.");
        }
        
         // Should not fail parsing as the string is a result of JSON serialization.
        JsonNode currentJsonNode = JsonNode.Parse(currentJsonString)
            ?? throw new JsonException("Could not parse JSON string to JsonNode when building update model for patching.");

        // Apply each PATCH operation to the JSON
        foreach (PatchOperationModel operation in patchModel.Operations)
        {
            try
            {
                currentJsonNode = PatchEngine.ApplyOperation(
                    currentJsonNode,
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

        currentJsonString = currentJsonNode.ToJsonString();

        // Deserialize the modified JSON back to UpdateDocumentRequestModel
        UpdateDocumentRequestModel? modifiedUpdateModel = _jsonSerializer.Deserialize<UpdateDocumentRequestModel>(currentJsonString);

        return modifiedUpdateModel is not null
            ? Attempt.SucceedWithStatus(ContentPatchingOperationStatus.Success, modifiedUpdateModel)
            : Attempt.FailWithStatus(ContentPatchingOperationStatus.InvalidOperation, default(UpdateDocumentRequestModel)!);
    }
}
