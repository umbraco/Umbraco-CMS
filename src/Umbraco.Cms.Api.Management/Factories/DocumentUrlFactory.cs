using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentUrlFactory : IDocumentUrlFactory
{
    private readonly IPublishedUrlInfoProvider _publishedUrlInfoProvider;


    [Obsolete("Use the constructor that takes all dependencies, scheduled for removal in v16")]
    public DocumentUrlFactory(IDocumentUrlService documentUrlService)
    : this(StaticServiceProvider.Instance.GetRequiredService<IPublishedUrlInfoProvider>())
    {
    }

    [Obsolete("Use the constructor that takes all dependencies, scheduled for removal in v16")]
    public DocumentUrlFactory(IDocumentUrlService documentUrlService, IPublishedUrlInfoProvider publishedUrlInfoProvider)
    : this(publishedUrlInfoProvider)
    {

    }

    public DocumentUrlFactory(IPublishedUrlInfoProvider publishedUrlInfoProvider)
    {
        _publishedUrlInfoProvider = publishedUrlInfoProvider;
    }

    public async Task<IEnumerable<DocumentUrlInfo>> CreateUrlsAsync(IContent content)
    {
        ISet<UrlInfo> urlInfos = await _publishedUrlInfoProvider.GetAllAsync(content);

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
