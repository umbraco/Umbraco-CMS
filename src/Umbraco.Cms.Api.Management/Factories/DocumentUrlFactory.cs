using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentUrlFactory : IDocumentUrlFactory
{
    private readonly IDocumentUrlService _documentUrlService;


    public DocumentUrlFactory(IDocumentUrlService documentUrlService)
    {
        _documentUrlService = documentUrlService;
    }

    public async Task<IEnumerable<DocumentUrlInfo>> CreateUrlsAsync(IContent content)
    {
        IEnumerable<UrlInfo> urlInfos = await _documentUrlService.ListUrlsAsync(content.Key);

        return urlInfos
            .Where(urlInfo => urlInfo.IsUrl)
            .Select(urlInfo => new DocumentUrlInfo { Culture = urlInfo.Culture, Url = urlInfo.Text })
            .ToArray();
    }

    public async Task<IEnumerable<DocumentUrlInfoResponseModel>> CreateUrlSetsAsync(IEnumerable<IContent> contentItems)
    {
        var documentUrlInfoResourceSets = new List<DocumentUrlInfoResponseModel>();

        foreach (IContent content in contentItems)
        {
            IEnumerable<DocumentUrlInfo> urls = await CreateUrlsAsync(content);
            documentUrlInfoResourceSets.Add(new DocumentUrlInfoResponseModel(content.Key, urls));
        }

        return documentUrlInfoResourceSets;
    }
}
