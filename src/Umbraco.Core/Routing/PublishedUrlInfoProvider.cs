using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Routing;

/// <summary>
///     Provides the default implementation of <see cref="IPublishedUrlInfoProvider" />: the URLs of published
///     content for the backoffice, together with a localized explanation for every culture that has no working URL
///     (unpublished ancestors, missing hostnames, collisions).
/// </summary>
public class PublishedUrlInfoProvider : IPublishedUrlInfoProvider
{
    private const string UrlProviderAlias = Constants.UrlProviders.Content;
    private const string PathSeparator = " > ";

    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly ILanguageService _languageService;
    private readonly IPublishedRouter _publishedRouter;
    private readonly IUmbracoContextAccessor _umbracoContextAccessor;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly ILogger<PublishedUrlInfoProvider> _logger;
    private readonly UriUtility _uriUtility;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IDocumentNavigationQueryService _navigationQueryService;
    private readonly IPublishStatusQueryService _publishStatusQueryService;
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IContentService _contentService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedUrlInfoProvider" /> class.
    /// </summary>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="languageService">The language service.</param>
    /// <param name="publishedRouter">The published router.</param>
    /// <param name="umbracoContextAccessor">The Umbraco context accessor.</param>
    /// <param name="localizedTextService">The localized text service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="uriUtility">The URI utility.</param>
    /// <param name="variationContextAccessor">The variation context accessor.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public PublishedUrlInfoProvider(
        IPublishedUrlProvider publishedUrlProvider,
        ILanguageService languageService,
        IPublishedRouter publishedRouter,
        IUmbracoContextAccessor umbracoContextAccessor,
        ILocalizedTextService localizedTextService,
        ILogger<PublishedUrlInfoProvider> logger,
        UriUtility uriUtility,
        IVariationContextAccessor variationContextAccessor)
        : this(
            publishedUrlProvider,
            languageService,
            publishedRouter,
            umbracoContextAccessor,
            localizedTextService,
            logger,
            uriUtility,
            variationContextAccessor,
            StaticServiceProvider.Instance.GetRequiredService<IDocumentNavigationQueryService>(),
            StaticServiceProvider.Instance.GetRequiredService<IPublishStatusQueryService>(),
            StaticServiceProvider.Instance.GetRequiredService<IPublishedContentCache>(),
            StaticServiceProvider.Instance.GetRequiredService<IContentService>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedUrlInfoProvider" /> class.
    /// </summary>
    /// <param name="publishedUrlProvider">The published URL provider.</param>
    /// <param name="languageService">The language service.</param>
    /// <param name="publishedRouter">The published router.</param>
    /// <param name="umbracoContextAccessor">The Umbraco context accessor.</param>
    /// <param name="localizedTextService">The localized text service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="uriUtility">The URI utility.</param>
    /// <param name="variationContextAccessor">The variation context accessor, used to resolve names in the culture being reported.</param>
    /// <param name="navigationQueryService">The document navigation query service, used to walk ancestors.</param>
    /// <param name="publishStatusQueryService">The publish status query service, used to find unpublished ancestors.</param>
    /// <param name="publishedContentCache">The published content cache, used to name published ancestors.</param>
    /// <param name="contentService">The content service, used to name unpublished ancestors.</param>
    public PublishedUrlInfoProvider(
        IPublishedUrlProvider publishedUrlProvider,
        ILanguageService languageService,
        IPublishedRouter publishedRouter,
        IUmbracoContextAccessor umbracoContextAccessor,
        ILocalizedTextService localizedTextService,
        ILogger<PublishedUrlInfoProvider> logger,
        UriUtility uriUtility,
        IVariationContextAccessor variationContextAccessor,
        IDocumentNavigationQueryService navigationQueryService,
        IPublishStatusQueryService publishStatusQueryService,
        IPublishedContentCache publishedContentCache,
        IContentService contentService)
    {
        _publishedUrlProvider = publishedUrlProvider;
        _languageService = languageService;
        _publishedRouter = publishedRouter;
        _umbracoContextAccessor = umbracoContextAccessor;
        _localizedTextService = localizedTextService;
        _logger = logger;
        _uriUtility = uriUtility;
        _variationContextAccessor = variationContextAccessor;
        _navigationQueryService = navigationQueryService;
        _publishStatusQueryService = publishStatusQueryService;
        _publishedContentCache = publishedContentCache;
        _contentService = contentService;
    }

    /// <inheritdoc />
    public Task<ISet<UrlInfo>> GetAllAsync(IContent content)
        => GetAllAsync(content, culture: null);

    /// <inheritdoc />
    public async Task<ISet<UrlInfo>> GetAllAsync(IContent content, string? culture)
    {
        var isInvariant = !content.ContentType.VariesByCulture();

        // Variant content restricted to a single culture, matched against the installed cultures (using their casing).
        if (isInvariant is false && culture is not null)
        {
            var matchedCulture = (await _languageService.GetAllIsoCodesAsync())
                .FirstOrDefault(x => x.InvariantEquals(culture));

            // A specific culture was requested that is not an installed culture - there are no urls to report.
            return matchedCulture is null
                ? new HashSet<UrlInfo>()
                : await BuildUrlInfosAsync(content, [matchedCulture], scopedCulture: matchedCulture, isInvariant);
        }

        // Invariant content (culture ignored), or all cultures.
        IReadOnlyCollection<string> cultures = (await GetCulturesForUrlLookupAsync(content)).ToArray();
        return await BuildUrlInfosAsync(content, cultures, scopedCulture: null, isInvariant);
    }

    /// <summary>
    /// Builds the set of <see cref="UrlInfo" /> for the given cultures, plus the "other" URLs (unless the content is
    /// trashed). When <paramref name="scopedCulture" /> is set, the "other" URLs are filtered to that culture.
    /// </summary>
    private async Task<ISet<UrlInfo>> BuildUrlInfosAsync(
        IContent content,
        IReadOnlyCollection<string> cultures,
        string? scopedCulture,
        bool isInvariant)
    {
        var urlInfos = new HashSet<UrlInfo>();
        foreach (var contentCulture in cultures)
        {
            UrlInfo? urlInfo = await GetCultureUrlInfoAsync(content, contentCulture, isInvariant);
            if (urlInfo is not null)
            {
                urlInfos.Add(urlInfo);
            }
        }

        // If the content is trashed, we can't get the other URLs, as we have no parent structure to navigate through.
        if (content.Trashed)
        {
            return urlInfos;
        }

        foreach (UrlInfo otherUrl in GetOtherUrls(content, scopedCulture))
        {
            urlInfos.Add(otherUrl);
        }

        return urlInfos;
    }

    /// <summary>
    /// Gets the <see cref="UrlInfo" /> for a single culture, or <c>null</c> if there is nothing to report
    /// (an unroutable URL on invariant content). Reports a message for an unroutable URL or a collision.
    /// </summary>
    private async Task<UrlInfo?> GetCultureUrlInfoAsync(IContent content, string culture, bool isInvariant)
    {
        var url = _publishedUrlProvider.GetUrl(content.Key, culture: culture);

        // For invariant content, a missing URL just means there's no domain for this culture - not worth reporting.
        if (isInvariant && url is Constants.Routing.Unroutable or Constants.Routing.UrlProviderException)
        {
            return null;
        }

        if (url is Constants.Routing.UrlProviderException)
        {
            return Message("getUrlException", culture);
        }

        if (url is Constants.Routing.Unroutable)
        {
            return await DescribeMissingUrlAsync(content, culture);
        }

        Attempt<UrlInfo?> hasCollision = await VerifyCollisionAsync(content, url, culture);
        return hasCollision is { Success: true, Result: not null }
            ? hasCollision.Result
            : UrlInfo.AsUrl(url, UrlProviderAlias, culture);
    }

    /// <summary>
    /// Explains why variant content has no URL for a culture: the culture itself is not published, an ancestor
    /// is not published (at all, or for the culture), or the culture is not the default language and has no
    /// hostname assigned. Anything else is reported as an anomaly.
    /// </summary>
    private async Task<UrlInfo> DescribeMissingUrlAsync(IContent content, string culture)
    {
        if (content.IsCulturePublished(culture) is false)
        {
            return Message("itemNotPublished", culture);
        }

        if (_navigationQueryService.TryGetAncestorsKeys(content.Key, out IEnumerable<Guid> ancestorKeys))
        {
            foreach (Guid ancestorKey in ancestorKeys)
            {
                if (_publishStatusQueryService.IsDocumentPublishedInAnyCulture(ancestorKey) is false)
                {
                    return Message("parentNotPublished", culture, GetContentName(ancestorKey, culture));
                }

                if (_publishStatusQueryService.IsDocumentPublished(ancestorKey, culture) is false)
                {
                    return Message("parentCultureNotPublished", culture, GetContentName(ancestorKey, culture));
                }
            }
        }

        var defaultCulture = await _languageService.GetDefaultIsoCodeAsync();
        if (culture.InvariantEquals(defaultCulture) is false && GetDomainCulturesForBranch(content).Contains(culture) is false)
        {
            ILanguage? language = await _languageService.GetAsync(culture);
            return Message("routeErrorNoDomainForCulture", culture, language?.CultureName ?? culture);
        }

        return Message("parentNotPublishedAnomaly", culture);
    }

    /// <summary>
    /// Gets the "other" URLs - i.e. not what you'd get with GetUrl(), including all the URLs registered using domains.
    /// These are not checked for routability or collisions - they are just reported. When scoped to a single culture,
    /// only the other URLs for that culture are returned.
    /// </summary>
    private IEnumerable<UrlInfo> GetOtherUrls(IContent content, string? scopedCulture)
        => _publishedUrlProvider.GetOtherUrls(content.Id)
            .Where(x => scopedCulture is null || string.Equals(x.Culture, scopedCulture, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.Message)
            .ThenBy(x => x.Culture);

    /// <summary>
    /// Gets the cultures to query URLs for.
    /// For invariant content, returns only cultures that have a domain assigned to the content
    /// or one of its ancestors. If no domains exist, returns only the default culture.
    /// For variant content, returns all cultures.
    /// </summary>
    private async Task<IEnumerable<string>> GetCulturesForUrlLookupAsync(IContent content)
    {
        if (content.ContentType.VariesByCulture())
        {
            return await _languageService.GetAllIsoCodesAsync();
        }

        HashSet<string> domainCultures = GetDomainCulturesForBranch(content);
        return domainCultures.Count > 0
            ? domainCultures
            : [await _languageService.GetDefaultIsoCodeAsync()];
    }

    /// <summary>
    /// Gets the cultures that have a domain assigned to the content or one of its ancestors.
    /// </summary>
    private HashSet<string> GetDomainCulturesForBranch(IContent content)
    {
        IUmbracoContext umbracoContext = _umbracoContextAccessor.GetRequiredUmbracoContext();
        var ancestorOrSelfIds = content.AncestorIds().Append(content.Id).ToHashSet();
        return umbracoContext.Domains.GetAll(true)
            .Where(d => ancestorOrSelfIds.Contains(d.ContentId))
            .Select(d => d.Culture)
            .WhereNotNull()
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Attempt<UrlInfo?>> VerifyCollisionAsync(IContent content, string url, string culture)
    {
        var uri = new Uri(url.TrimEnd('/'), UriKind.RelativeOrAbsolute);
        if (uri.IsAbsoluteUri is false)
        {
            uri = uri.MakeAbsolute(_umbracoContextAccessor.GetRequiredUmbracoContext().CleanedUmbracoUrl);
        }

        uri = _uriUtility.UriToUmbraco(uri);
        IPublishedRequestBuilder builder = await _publishedRouter.CreateRequestAsync(uri);
        IPublishedRequest publishedRequest = await _publishedRouter.RouteRequestAsync(builder, new RouteRequestOptions(RouteDirection.Outbound));

        if (publishedRequest.HasPublishedContent() is false)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                const string logMsg = nameof(VerifyCollisionAsync) +
                                      " did not resolve a content item for original url: {Url}, translated to {TranslatedUrl} and culture: {Culture}";
                _logger.LogDebug(logMsg, url, uri, culture);
            }

            return Attempt.Succeed<UrlInfo?>(Message("routeErrorCannotRoute", culture));
        }

        if (publishedRequest.IgnorePublishedContentCollisions)
        {
            return Attempt<UrlInfo?>.Fail();
        }

        if (publishedRequest.PublishedContent is not null && publishedRequest.PublishedContent.Id != content.Id)
        {
            return Attempt.Succeed<UrlInfo?>(Message("routeError", culture, GetContentPath(publishedRequest.PublishedContent, culture)));
        }

        // No collision
        return Attempt<UrlInfo?>.Fail();
    }

    /// <summary>
    /// Gets the names of the content and its ancestors, root first, in the given culture where available.
    /// </summary>
    private string GetContentPath(IPublishedContent content, string culture)
    {
        var names = new List<string> { GetContentName(content, culture) };

        if (_navigationQueryService.TryGetAncestorsKeys(content.Key, out IEnumerable<Guid> ancestorKeys))
        {
            foreach (Guid ancestorKey in ancestorKeys)
            {
                IPublishedContent? ancestor = _publishedContentCache.GetById(ancestorKey);
                if (ancestor is not null)
                {
                    names.Add(GetContentName(ancestor, culture));
                }
            }
        }

        names.Reverse();
        return string.Join(PathSeparator, names);
    }

    private string GetContentName(IPublishedContent content, string culture)
    {
        var name = content.Name(_variationContextAccessor, culture);
        return string.IsNullOrEmpty(name) ? content.Name : name;
    }

    private string GetContentName(Guid key, string culture)
    {
        IContent? content = _contentService.GetById(key);
        if (content is null)
        {
            return key.ToString();
        }

        return (content.ContentType.VariesByCulture() ? content.GetCultureName(culture) : null) ?? content.Name ?? key.ToString();
    }

    private UrlInfo Message(string alias, string culture, params string?[] tokens)
        => UrlInfo.AsMessage(_localizedTextService.Localize("content", alias, tokens), UrlProviderAlias, culture);
}
