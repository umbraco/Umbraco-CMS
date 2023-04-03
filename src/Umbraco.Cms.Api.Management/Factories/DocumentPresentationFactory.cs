using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Item;
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

    public DocumentPresentationFactory(
        IUmbracoMapper umbracoMapper,
        IContentUrlFactory contentUrlFactory,
        IFileService fileService)
    {
        _umbracoMapper = umbracoMapper;
        _contentUrlFactory = contentUrlFactory;
        _fileService = fileService;
    }

    public async Task<DocumentResponseModel> CreateResponseModelAsync(IContent content)
    {
        DocumentResponseModel responseModel = _umbracoMapper.Map<DocumentResponseModel>(content)!;

        responseModel.Urls = await _contentUrlFactory.GetUrlsAsync(content);

        responseModel.TemplateKey = content.TemplateId.HasValue
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
            Icon = entity.ContentTypeIcon ?? Constants.Icons.ContentType,
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
}
