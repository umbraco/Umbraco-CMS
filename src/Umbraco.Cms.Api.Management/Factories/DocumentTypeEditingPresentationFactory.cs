using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType.Composition;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentTypeEditing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using ContentTypeCleanupViewModel = Umbraco.Cms.Api.Management.ViewModels.ContentType.ContentTypeCleanup;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentTypeEditingPresentationFactory : ContentTypeEditingPresentationFactory, IDocumentTypeEditingPresentationFactory
{
    private readonly IContentTypeService _contentTypeService;

    public DocumentTypeEditingPresentationFactory(IContentTypeService contentTypeService)
        : base(contentTypeService)
    {
        _contentTypeService = contentTypeService;
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

    public IEnumerable<AvailableContentTypeCompositionResponseModel> CreateCompositionModels(
        IEnumerable<ContentTypeAvailableCompositionsResult> compositionResults,
        IEnumerable<string> currentCompositionAliases,
        IEnumerable<string> persistedCompositionAliases)
        => compositionResults.Select(x => CreateCompositionModel(x, currentCompositionAliases, persistedCompositionAliases));

    private void MapCleanup(ContentTypeModelBase model, ContentTypeCleanupViewModel cleanup)
        => model.Cleanup = new ContentTypeCleanup
        {
            PreventCleanup = cleanup.PreventCleanup,
            KeepAllVersionsNewerThanDays = cleanup.KeepAllVersionsNewerThanDays,
            KeepLatestVersionPerDayForDays = cleanup.KeepLatestVersionPerDayForDays
        };

    private AvailableContentTypeCompositionResponseModel CreateCompositionModel(
        ContentTypeAvailableCompositionsResult compositionResult,
        IEnumerable<string> persistedCompositionAliases,
        IEnumerable<string> ancestorCompositionAliases)
    {
        IContentTypeComposition composition = compositionResult.Composition;
        IEnumerable<string>? folders = null;
        bool isAllowed;

        if (composition is IContentType contentType)
        {
            var containers = _contentTypeService.GetContainers(contentType); // NB: different for media/member (media/member service)
            folders = containers.Select(c => c.Name).WhereNotNull();
        }

        // We need to ensure that the item is allowed if it is already selected
        // but do not allow it if it is any of the ancestors
        if (persistedCompositionAliases.Contains(composition.Alias) && ancestorCompositionAliases.Contains(composition.Alias) is false)
        {
            isAllowed = true;
        }
        else
        {
            isAllowed = compositionResult.Allowed;
        }

        return new AvailableContentTypeCompositionResponseModel
        {
            Id = composition.Key,
            Name = composition.Name ?? string.Empty,
            Icon = composition.Icon ?? string.Empty,
            FolderPath = folders ?? Array.Empty<string>(),
            IsAllowed = isAllowed
        };
    }
}
