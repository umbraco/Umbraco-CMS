using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentPresentationModelFactory : IDocumentPresentationModelFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IContentUrlFactory _contentUrlFactory;
    private readonly IFileService _fileService;

    public DocumentPresentationModelFactory(
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
}
