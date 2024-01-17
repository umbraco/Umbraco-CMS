﻿using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Composition;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using ContentTypeCleanupViewModel = Umbraco.Cms.Api.Management.ViewModels.ContentType.ContentTypeCleanup;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentTypeEditingPresentationFactory : ContentTypeEditingPresentationFactory<IContentType>, IDocumentTypeEditingPresentationFactory
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
        createModel.ContainerKey = requestModel.ContainerId;
        createModel.AllowedTemplateKeys = requestModel.AllowedTemplateIds;
        createModel.DefaultTemplateKey = requestModel.DefaultTemplateId;

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

        updateModel.AllowedTemplateKeys = requestModel.AllowedTemplateIds;
        updateModel.DefaultTemplateKey = requestModel.DefaultTemplateId;

        return updateModel;
    }

    public IEnumerable<AvailableDocumentTypeCompositionResponseModel> MapCompositionModels(IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults)
        => compositionResults.Select(MapCompositionModel<AvailableDocumentTypeCompositionResponseModel>);

    private void MapCleanup(ContentTypeModelBase model, ContentTypeCleanupViewModel cleanup)
        => model.Cleanup = new ContentTypeCleanup
        {
            PreventCleanup = cleanup.PreventCleanup,
            KeepAllVersionsNewerThanDays = cleanup.KeepAllVersionsNewerThanDays,
            KeepLatestVersionPerDayForDays = cleanup.KeepLatestVersionPerDayForDays
        };
}
