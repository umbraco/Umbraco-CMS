using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
using Umbraco.Cms.Api.Management.ViewModels.DocumentBlueprint.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentPresentationFactory : IDocumentPresentationFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IContentUrlFactory _contentUrlFactory;
    private readonly IFileService _fileService;
    private readonly IContentTypeService _contentTypeService;

    public DocumentPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IContentUrlFactory contentUrlFactory,
        IFileService fileService,
        IContentTypeService contentTypeService)
    {
        _umbracoMapper = umbracoMapper;
        _contentUrlFactory = contentUrlFactory;
        _fileService = fileService;
        _contentTypeService = contentTypeService;
    }

    public async Task<DocumentResponseModel> CreateResponseModelAsync(IContent content)
    {
        DocumentResponseModel responseModel = _umbracoMapper.Map<DocumentResponseModel>(content)!;

        responseModel.Urls = await _contentUrlFactory.GetUrlsAsync(content);

        responseModel.TemplateId = content.TemplateId.HasValue
            ? _fileService.GetTemplate(content.TemplateId.Value)?.Key
            : null;

        return responseModel;
    }

    public DocumentItemResponseModel CreateItemResponseModel(IDocumentEntitySlim entity, string? culture = null)
    {
        var responseModel = new DocumentItemResponseModel
        {
            Name = entity.Name ?? string.Empty,
            Id = entity.Key,
            Icon = entity.ContentTypeIcon,
        };

        if (culture == null || !entity.Variations.VariesByCulture())
        {
            return responseModel;
        }

        if (entity.CultureNames.TryGetValue(culture, out var cultureName))
        {
            responseModel.Name = cultureName;
        }

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
}
