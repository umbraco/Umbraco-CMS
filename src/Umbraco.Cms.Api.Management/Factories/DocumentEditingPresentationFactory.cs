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
        return new ContentPatchModel
        {
            TemplateKey = requestModel.Template?.Id,
            Variants = requestModel.Variants?.Select(v => new VariantPatchModel
            {
                Culture = v.Culture,
                Segment = v.Segment,
                Name = v.Name
            }),
            Properties = requestModel.Values?.Select(v => new PropertyPatchModel
            {
                Alias = v.Alias,
                Culture = v.Culture,
                Segment = v.Segment,
                Value = v.Value
            }),
            AffectedCultures = requestModel.Variants?
                .Where(v => v.Culture != null)
                .Select(v => v.Culture!)
                .Distinct()
                .ToArray() ?? Array.Empty<string>()
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
