using Umbraco.Cms.Api.Management.Mapping.Content;
using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Content;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentType;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentPresentationFactory
    : ContentPresentationFactoryBase<IContentType, IContentTypeService>, IDocumentPresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IContentUrlFactory _contentUrlFactory;
    private readonly IFileService _fileService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IPublicAccessService _publicAccessService;

    public DocumentPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IContentUrlFactory contentUrlFactory,
        IFileService fileService,
        IContentTypeService contentTypeService,
        IPublicAccessService publicAccessService)
        : base(contentTypeService, umbracoMapper)
    {
        _umbracoMapper = umbracoMapper;
        _contentUrlFactory = contentUrlFactory;
        _fileService = fileService;
        _contentTypeService = contentTypeService;
        _publicAccessService = publicAccessService;
    }

    public async Task<DocumentResponseModel> CreateResponseModelAsync(IContent content)
    {
        DocumentResponseModel responseModel = _umbracoMapper.Map<DocumentResponseModel>(content)!;

        responseModel.Urls = await _contentUrlFactory.GetUrlsAsync(content);

        Guid? templateKey = content.TemplateId.HasValue
            ? _fileService.GetTemplate(content.TemplateId.Value)?.Key
            : null;

        responseModel.Template = templateKey.HasValue
            ? new ReferenceByIdModel { Id = templateKey.Value }
            : null;

        return responseModel;
    }

    public DocumentItemResponseModel CreateItemResponseModel(IDocumentEntitySlim entity)
    {
        var responseModel = new DocumentItemResponseModel
        {
            Id = entity.Key,
            IsTrashed = entity.Trashed
        };

        responseModel.IsProtected = _publicAccessService.IsProtected(entity.Path);

        IContentType? contentType = _contentTypeService.Get(entity.ContentTypeAlias);
        if (contentType is not null)
        {
            responseModel.DocumentType = _umbracoMapper.Map<DocumentTypeReferenceResponseModel>(contentType)!;
        }

        responseModel.Variants = CreateVariantsItemResponseModels(entity);

        return responseModel;
    }

    public DocumentBlueprintResponseModel CreateBlueprintItemResponseModel(IDocumentEntitySlim entity)
    {
        var responseModel = new DocumentBlueprintResponseModel()
        {
            Id = entity.Key,
        };

        IContentType? contentType = _contentTypeService.Get(entity.ContentTypeAlias);
        responseModel.Name = contentType?.Name ?? entity.Name ?? string.Empty;
        return responseModel;
    }

    public IEnumerable<VariantItemResponseModel> CreateVariantsItemResponseModels(IDocumentEntitySlim entity)
    {
        if (entity.Variations.VariesByCulture() is false)
        {
            yield return new()
            {
                Name = entity.Name ?? string.Empty,
                State = ContentStateHelper.GetContentState(entity, null),
                Culture = null,
            };
            yield break;
        }

        foreach (KeyValuePair<string, string> cultureNamePair in entity.CultureNames)
        {
            yield return new()
            {
                Name = cultureNamePair.Value,
                Culture = cultureNamePair.Key,
                State = ContentStateHelper.GetContentState(entity, cultureNamePair.Key)
            };
        }
    }

    public DocumentTypeReferenceResponseModel CreateDocumentTypeReferenceResponseModel(IDocumentEntitySlim entity)
        => CreateContentTypeReferenceResponseModel<DocumentTypeReferenceResponseModel>(entity);
}
