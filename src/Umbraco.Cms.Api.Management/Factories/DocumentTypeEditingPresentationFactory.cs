using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentTypeEditingPresentationFactory : ContentTypeEditingPresentationFactory<IContentType>, IDocumentTypeEditingPresentationFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentTypeEditingPresentationFactory"/> class, using the specified content type service.
    /// </summary>
    /// <param name="contentTypeService">The service used to manage and retrieve content types.</param>
    public DocumentTypeEditingPresentationFactory(IContentTypeService contentTypeService)
        : base(contentTypeService)
    {
    }

    /// <summary>
    /// Maps the data from a <see cref="CreateDocumentTypeRequestModel"/> to a new <see cref="ContentTypeCreateModel"/> instance.
    /// This includes transferring basic properties, allowed templates, default template, list view, allowed content types, compositions, and container information.
    /// </summary>
    /// <param name="requestModel">The request model containing the document type creation data to be mapped.</param>
    /// <returns>
    /// A <see cref="ContentTypeCreateModel"/> populated with the corresponding data from the <paramref name="requestModel"/>.
    /// </returns>
    public ContentTypeCreateModel MapCreateModel(CreateDocumentTypeRequestModel requestModel)
    {
        ContentTypeCreateModel createModel = MapContentTypeEditingModel<
            ContentTypeCreateModel,
            ContentTypePropertyTypeModel,
            ContentTypePropertyContainerModel,
            CreateDocumentTypePropertyTypeRequestModel,
            CreateDocumentTypePropertyTypeContainerRequestModel
        >(requestModel);

        MapCleanup(createModel, requestModel.Cleanup);

        createModel.Key = requestModel.Id;
        createModel.AllowedTemplateKeys = requestModel.AllowedTemplates.Select(reference => reference.Id).ToArray();
        createModel.DefaultTemplateKey = requestModel.DefaultTemplate?.Id;
        createModel.ListView = requestModel.Collection?.Id;
        createModel.AllowedContentTypes = MapAllowedContentTypes(requestModel.AllowedDocumentTypes);

        IDictionary<Guid, ViewModels.ContentType.CompositionType> compositionTypesByKey = CompositionTypesByKey(requestModel.Compositions);
        createModel.Compositions = MapCompositions(compositionTypesByKey);
        createModel.ContainerKey = CalculateCreateContainerKey(requestModel.Parent, compositionTypesByKey);

        return createModel;
    }

    /// <summary>
    /// Maps the given <see cref="UpdateDocumentTypeRequestModel"/> to a <see cref="ContentTypeUpdateModel"/>.
    /// </summary>
    /// <param name="requestModel">The update request model containing document type data to map.</param>
    /// <returns>A <see cref="ContentTypeUpdateModel"/> representing the updated document type.</returns>
    public ContentTypeUpdateModel MapUpdateModel(UpdateDocumentTypeRequestModel requestModel)
    {
        ContentTypeUpdateModel updateModel = MapContentTypeEditingModel<
            ContentTypeUpdateModel,
            ContentTypePropertyTypeModel,
            ContentTypePropertyContainerModel,
            UpdateDocumentTypePropertyTypeRequestModel,
            UpdateDocumentTypePropertyTypeContainerRequestModel
        >(requestModel);

        MapCleanup(updateModel, requestModel.Cleanup);

        updateModel.AllowedTemplateKeys = requestModel.AllowedTemplates.Select(reference => reference.Id).ToArray();
        updateModel.DefaultTemplateKey = requestModel.DefaultTemplate?.Id;
        updateModel.ListView = requestModel.Collection?.Id;
        updateModel.AllowedContentTypes = MapAllowedContentTypes(requestModel.AllowedDocumentTypes);
        updateModel.Compositions = MapCompositions(CompositionTypesByKey(requestModel.Compositions));

        return updateModel;
    }

    public IEnumerable<AvailableDocumentTypeCompositionResponseModel> MapCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults)
        => compositionResults.Select(MapCompositionModel<AvailableDocumentTypeCompositionResponseModel>);

    private void MapCleanup(ContentTypeModelBase model, DocumentTypeCleanup cleanup)
        => model.Cleanup = new ContentTypeCleanup
        {
            PreventCleanup = cleanup.PreventCleanup,
            KeepAllVersionsNewerThanDays = cleanup.KeepAllVersionsNewerThanDays,
            KeepLatestVersionPerDayForDays = cleanup.KeepLatestVersionPerDayForDays
        };

    private IEnumerable<ContentTypeSort> MapAllowedContentTypes(IEnumerable<DocumentTypeSort> allowedDocumentTypes)
        => MapAllowedContentTypes(allowedDocumentTypes
            .DistinctBy(t => t.DocumentType.Id)
            .ToDictionary(t => t.DocumentType.Id, t => t.SortOrder));

    private IDictionary<Guid, ViewModels.ContentType.CompositionType> CompositionTypesByKey(IEnumerable<DocumentTypeComposition> documentTypeCompositions)
        => documentTypeCompositions
            .DistinctBy(c => c.DocumentType.Id)
            .ToDictionary(c => c.DocumentType.Id, c => c.CompositionType);
}
