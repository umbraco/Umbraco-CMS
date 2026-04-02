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

/// <summary>
/// Provides methods for generating URLs for documents within the Umbraco CMS management API.
/// </summary>
public class DocumentUrlFactory : IDocumentUrlFactory
{
    private readonly IPublishedUrlInfoProvider _publishedUrlInfoProvider;
    private readonly UrlProviderCollection _urlProviders;
    private readonly IPreviewService _previewService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly ILogger<DocumentUrlFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentUrlFactory"/> class.
    /// </summary>
    /// <param name="publishedUrlInfoProvider">Provides information about published URLs.</param>
    /// <param name="urlProviders">A collection of URL providers used to resolve document URLs.</param>
    /// <param name="previewService">Service for handling content preview functionality.</param>
    /// <param name="backOfficeSecurityAccessor">Provides access to the back office security context.</param>
    /// <param name="logger">The logger used for logging within this factory.</param>
    public DocumentUrlFactory(
        IPublishedUrlInfoProvider publishedUrlInfoProvider,
        UrlProviderCollection urlProviders,
        IPreviewService previewService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        ILogger<DocumentUrlFactory> logger)
    {
        _publishedUrlInfoProvider = publishedUrlInfoProvider;
        _urlProviders = urlProviders;
        _previewService = previewService;
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Asynchronously generates a collection of <see cref="Umbraco.Cms.Api.Management.Factories.DocumentUrlInfo"/> instances representing the URLs for the specified content item.
    /// </summary>
    /// <param name="content">The content item for which to generate URLs.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains an <see cref="IEnumerable{DocumentUrlInfo}"/> with the generated URLs.</returns>
    public async Task<IEnumerable<DocumentUrlInfo>> CreateUrlsAsync(IContent content)
    {
        ISet<UrlInfo> urlInfos = await _publishedUrlInfoProvider.GetAllAsync(content);
        return urlInfos
            .Select(CreateDocumentUrlInfo)
            .ToArray();
    }

    /// <summary>
    /// Asynchronously creates URL sets for the specified collection of content items.
    /// </summary>
    /// <param name="contentItems">The collection of <see cref="IContent"/> items for which to generate URL sets.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a collection of <see cref="DocumentUrlInfoResponseModel"/> objects, each representing the URL set for a content item.</returns>
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
