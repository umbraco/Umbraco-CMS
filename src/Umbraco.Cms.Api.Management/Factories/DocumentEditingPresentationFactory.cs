using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentEditingPresentationFactory : ContentEditingPresentationFactory<DocumentValueModel, DocumentVariantRequestModel>, IDocumentEditingPresentationFactory
{
    public ContentCreateModel MapCreateModel(CreateDocumentRequestModel requestModel)
    {
        ContentCreateModel model = MapContentEditingModel<ContentCreateModel>(requestModel);
        model.Key = requestModel.Id;
        model.ContentTypeKey = requestModel.DocumentType.Id;
        model.TemplateKey = requestModel.Template?.Id;
        model.ParentKey = requestModel.Parent?.Id;

        return model;
    }

    public ContentUpdateModel MapUpdateModel(UpdateDocumentRequestModel requestModel)
        => MapUpdateContentModel<ContentUpdateModel>(requestModel);

    public ContentPatchModel MapPatchModel(PatchDocumentRequestModel requestModel)
    {
        var cultureExtractor = new Umbraco.Cms.Core.PropertyEditors.JsonPath.JsonPathCultureExtractor();
        var operations = requestModel.Operations.Select(op => new PatchOperationModel
        {
            Op = MapOperationType(op.Op),
            Path = op.Path,
            Value = op.Value
        }).ToArray();

        var affectedCultures = cultureExtractor.ExtractCulturesFromOperations(operations.Select(o => o.Path)).ToArray();
        var affectedSegments = operations
            .SelectMany(o => cultureExtractor.ExtractSegments(o.Path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return new ContentPatchModel
        {
            Operations = operations,
            AffectedCultures = affectedCultures,
            AffectedSegments = affectedSegments
        };
    }

    private static PatchOperationType MapOperationType(string op)
    {
        return op.ToLowerInvariant() switch
        {
            "replace" => PatchOperationType.Replace,
            "add" => PatchOperationType.Add,
            "remove" => PatchOperationType.Remove,
            _ => throw new ArgumentException($"Unsupported operation type: {op}", nameof(op))
        };
    }

    public ValidateContentUpdateModel MapValidateUpdateModel(ValidateUpdateDocumentRequestModel requestModel)
    {
        ValidateContentUpdateModel model = MapUpdateContentModel<ValidateContentUpdateModel>(requestModel);
        model.Cultures = requestModel.Cultures;

        return model;
    }

    private TUpdateModel MapUpdateContentModel<TUpdateModel>(UpdateDocumentRequestModel requestModel)
        where TUpdateModel : ContentUpdateModel, new()
    {
        TUpdateModel model = MapContentEditingModel<TUpdateModel>(requestModel);
        model.TemplateKey = requestModel.Template?.Id;

        return model;
    }
}
