using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentEditingPresentationFactory : ContentEditingPresentationFactory<DocumentValueModel, DocumentVariantRequestModel>, IDocumentEditingPresentationFactory
{
    /// <summary>
    /// Maps a <see cref="CreateDocumentRequestModel"/> to a <see cref="ContentCreateModel"/>.
    /// </summary>
    /// <param name="requestModel">The request model containing data to create the content.</param>
    /// <returns>A <see cref="ContentCreateModel"/> representing the content to be created.</returns>
    public ContentCreateModel MapCreateModel(CreateDocumentRequestModel requestModel)
    {
        ContentCreateModel model = MapContentEditingModel<ContentCreateModel>(requestModel);
        model.Key = requestModel.Id;
        model.ContentTypeKey = requestModel.DocumentType.Id;
        model.TemplateKey = requestModel.Template?.Id;
        model.ParentKey = requestModel.Parent?.Id;

        return model;
    }

    /// <summary>
    /// Maps the given <see cref="UpdateDocumentRequestModel"/> to a <see cref="ContentUpdateModel"/>.
    /// </summary>
    /// <param name="requestModel">The update document request model to map from.</param>
    /// <returns>The mapped <see cref="ContentUpdateModel"/> instance.</returns>
    public ContentUpdateModel MapUpdateModel(UpdateDocumentRequestModel requestModel)
        => MapUpdateContentModel<ContentUpdateModel>(requestModel);

    /// <summary>
    /// Maps a <see cref="ValidateUpdateDocumentRequestModel"/> to a <see cref="ValidateContentUpdateModel"/>, copying relevant validation data.
    /// </summary>
    /// <param name="requestModel">The request model containing the document update validation data.</param>
    /// <returns>A <see cref="ValidateContentUpdateModel"/> populated with validation data from the request model.</returns>
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
