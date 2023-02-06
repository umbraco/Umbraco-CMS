using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentViewModelFactory : IDocumentViewModelFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IContentUrlFactory _contentUrlFactory;
    private readonly IFileService _fileService;

    public DocumentViewModelFactory(
        IUmbracoMapper umbracoMapper,
        IContentUrlFactory contentUrlFactory,
        IFileService fileService)
    {
        _umbracoMapper = umbracoMapper;
        _contentUrlFactory = contentUrlFactory;
        _fileService = fileService;
    }

    public async Task<DocumentViewModel> CreateViewModelAsync(IContent content)
    {
        DocumentViewModel viewModel = _umbracoMapper.Map<DocumentViewModel>(content)!;

        viewModel.Urls = await _contentUrlFactory.GetUrlsAsync(content);

        viewModel.TemplateKey = content.TemplateId.HasValue
            ? _fileService.GetTemplate(content.TemplateId.Value)?.Key
            : null;

        return viewModel;
    }
}
