using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentViewModelFactory : IDocumentViewModelFactory
{
    private readonly IUmbracoMapper _umbracoMapper;
    private readonly IDocumentUrlFactory _documentUrlFactory;
    private readonly IFileService _fileService;

    public DocumentViewModelFactory(
        IUmbracoMapper umbracoMapper,
        IDocumentUrlFactory documentUrlFactory,
        IFileService fileService)
    {
        _umbracoMapper = umbracoMapper;
        _documentUrlFactory = documentUrlFactory;
        _fileService = fileService;
    }

    public async Task<DocumentViewModel> CreateViewModelAsync(IContent content)
    {
        DocumentViewModel viewModel = _umbracoMapper.Map<DocumentViewModel>(content)!;

        viewModel.Urls = await _documentUrlFactory.GetUrlsAsync(content);

        viewModel.TemplateKey = content.TemplateId.HasValue
            ? _fileService.GetTemplate(content.TemplateId.Value)?.Key
            : null;

        return viewModel;
    }
}
