using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using ContentTypeCleanupViewModel = Umbraco.Cms.Api.Management.ViewModels.ContentType.ContentTypeCleanup;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentTypeEditingPresentationFactory : ContentTypeEditingPresentationFactory, IDocumentTypeEditingPresentationFactory
{
    public DocumentTypeEditingPresentationFactory(IContentTypeService contentTypeService)
        : base(contentTypeService)
    {
    }

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
        createModel.ContainerKey = requestModel.Folder?.Id;
        createModel.AllowedTemplateKeys = requestModel.AllowedTemplates.Select(reference => reference.Id).ToArray();
        createModel.DefaultTemplateKey = requestModel.DefaultTemplate?.Id;
        createModel.AllowedContentTypes = MapAllowedContentTypes(requestModel.AllowedDocumentTypes);
        createModel.Compositions = MapCompositions(requestModel.Compositions);

        return createModel;
    }

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
        updateModel.AllowedContentTypes = MapAllowedContentTypes(requestModel.AllowedDocumentTypes);
        updateModel.Compositions = MapCompositions(requestModel.Compositions);

        return updateModel;
    }

    private void MapCleanup(ContentTypeModelBase model, ContentTypeCleanupViewModel cleanup)
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

    private IEnumerable<Composition> MapCompositions(IEnumerable<DocumentTypeComposition> documentTypeCompositions)
        => MapCompositions(documentTypeCompositions
            .DistinctBy(c => c.DocumentType.Id)
            .ToDictionary(c => c.DocumentType.Id, c => c.CompositionType));
}
