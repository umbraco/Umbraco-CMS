using Microsoft.Extensions.Logging;
using Umbraco.Cms.Api.Management.Routing;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.Factories;

public class DocumentUrlFactory : IDocumentUrlFactory
{
    private readonly IPublishedUrlInfoProvider _publishedUrlInfoProvider;
    private readonly UrlProviderCollection _urlProviders;
    private readonly IPreviewService _previewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IAbsoluteUrlBuilder _absoluteUrlBuilder;
    private readonly ILogger<DocumentUrlFactory> _logger;

    public DocumentUrlFactory(
        IPublishedUrlInfoProvider publishedUrlInfoProvider,
        UrlProviderCollection urlProviders,
        IPreviewService previewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IAbsoluteUrlBuilder absoluteUrlBuilder,
        ILogger<DocumentUrlFactory> logger)
    {
        _publishedUrlInfoProvider = publishedUrlInfoProvider;
        _urlProviders = urlProviders;
        _previewService = previewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _absoluteUrlBuilder = absoluteUrlBuilder;
        _logger = logger;
    }

    public async Task<IEnumerable<DocumentUrlInfo>> CreateUrlsAsync(IContent content)
    {
        ISet<UrlInfo> urlInfos = await _publishedUrlInfoProvider.GetAllAsync(content);
        return urlInfos
            .Select(CreateDocumentUrlInfo)
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

    public async Task<DocumentUrlInfo?> GetPreviewUrlAsync(IContent content, string providerAlias, string? culture, string? segment)
    {
        IUrlProvider? provider = _urlProviders.FirstOrDefault(provider => provider.Alias.InvariantEquals(providerAlias));
        if (provider is null)
        {
            _logger.LogError("Could not resolve a URL provider requested for preview - it was not registered in the URL providers collection.");
            return null;
        }

        UrlInfo? previewUrlInfo = await provider.GetPreviewUrlAsync(content, culture, segment);
        if (previewUrlInfo is null)
        {
            _logger.LogError("The URL provider could not generate a preview URL for content with key: {contentKey}", content.Key);
            return null;
        }

        // must initiate preview state for internal preview URLs
        if (previewUrlInfo.Url is not null && previewUrlInfo.IsExternal is false)
        {
            IUser? currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser;
            if (currentUser is null)
            {
                _logger.LogError("Could not access the current backoffice user while attempting to authenticate for preview.");
                return null;
            }

            if (await _previewService.TryEnterPreviewAsync(currentUser) is false)
            {
                _logger.LogError("A server error occured, could not initiate an authenticated preview state for the current user.");
                return null;
            }
        }

        return CreateDocumentUrlInfo(previewUrlInfo);
    }

    private DocumentUrlInfo CreateDocumentUrlInfo(UrlInfo urlInfo)
    {
        var url = urlInfo.Url?.ToString();
        return new DocumentUrlInfo
        {
            Culture = urlInfo.Culture,
            Url = url,
            Message = urlInfo.Message,
            Provider = urlInfo.Provider,
        };
    }
}
