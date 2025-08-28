using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentUrlFactory : IDocumentUrlFactory
{
    private readonly IPublishedUrlInfoProvider _publishedUrlInfoProvider;
    private readonly UrlProviderCollection _urlProviders;

    public DocumentUrlFactory(IPublishedUrlInfoProvider publishedUrlInfoProvider, UrlProviderCollection urlProviders)
    {
        _publishedUrlInfoProvider = publishedUrlInfoProvider;
        _urlProviders = urlProviders;
    }

    public async Task<IEnumerable<DocumentUrlInfo>> CreateUrlsAsync(IContent content)
    {
        ISet<UrlInfo> urlInfos = await _publishedUrlInfoProvider.GetAllAsync(content);
        return CreateDocumentUrlInfos(urlInfos);
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

    public Task<IEnumerable<DocumentUrlInfoResponseModel>> CreatePreviewUrlSetsAsync(IEnumerable<IContent> contentItems)
    {
        DocumentUrlInfoResponseModel[] documentUrlInfoResourceSets = contentItems.Select(content =>
            {
                IEnumerable<UrlInfo> previewUrls = _urlProviders.SelectMany(provider => provider.GetPreviewUrls(content));
                return new DocumentUrlInfoResponseModel(content.Key, CreateDocumentUrlInfos(previewUrls));
            })
            .ToArray();

        return Task.FromResult<IEnumerable<DocumentUrlInfoResponseModel>>(documentUrlInfoResourceSets);
    }

    private IEnumerable<DocumentUrlInfo> CreateDocumentUrlInfos(IEnumerable<UrlInfo> urlInfos)
        => urlInfos
            .Where(urlInfo => urlInfo.Url is not null)
            .Select(urlInfo => new DocumentUrlInfo { Culture = urlInfo.Culture, Url = urlInfo.Url!.ToString(), Message = urlInfo.Message, IsExternal = urlInfo.IsExternal })
            .ToArray();
}
