using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Extensions;

public static class UrlProviderExtensions
{
    /// <summary>
    ///     Gets the URLs of the content item.
    /// </summary>
    /// <remarks>
    ///     <para>Use when displaying URLs. If errors occur when generating the URLs, they will show in the list.</para>
    ///     <para>Contains all the URLs that we can figure out (based upon domains, etc).</para>
    /// </remarks>
    public static async Task<IEnumerable<UrlInfo>> GetContentUrlsAsync(
        this IContent content,
        IPublishedRouter publishedRouter,
        IUmbracoContext umbracoContext,
        ILocalizationService localizationService,
        ILocalizedTextService textService,
        IContentService contentService,
        IVariationContextAccessor variationContextAccessor,
        ILogger<IContent> logger,
        UriUtility uriUtility,
        IPublishedUrlProvider publishedUrlProvider)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(publishedRouter);
        ArgumentNullException.ThrowIfNull(umbracoContext);
        ArgumentNullException.ThrowIfNull(localizationService);
        ArgumentNullException.ThrowIfNull(textService);
        ArgumentNullException.ThrowIfNull(contentService);
        ArgumentNullException.ThrowIfNull(variationContextAccessor);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(uriUtility);
        ArgumentNullException.ThrowIfNull(publishedUrlProvider);

        var result = new List<UrlInfo>();

        if (content.Published == false)
        {
            result.Add(UrlInfo.Message(textService.Localize("content", "itemNotPublished")));
            return result;
        }

        // build a list of URLs, for the back-office
        // which will contain
        // - the 'main' URLs, which is what .Url would return, for each culture
        // - the 'other' URLs we know (based upon domains, etc)
        //
        // need to work through each installed culture:
        // on invariant nodes, each culture returns the same URL segment but,
        // we don't know if the branch to this content is invariant, so we need to ask
        // for URLs for all cultures.
        // and, not only for those assigned to domains in the branch, because we want
        // to show what GetUrl() would return, for every culture.
        var urls = new HashSet<UrlInfo>();
        var cultures = localizationService.GetAllLanguages().Select(x => x.IsoCode).ToList();

        // get all URLs for all cultures
        // in a HashSet, so de-duplicates too
        foreach (UrlInfo cultureUrl in await GetContentUrlsByCultureAsync(content, cultures, publishedRouter, umbracoContext, contentService, textService, variationContextAccessor, logger, uriUtility, publishedUrlProvider))
        {
            urls.Add(cultureUrl);
        }

        // return the real URLs first, then the messages
        foreach (IGrouping<bool, UrlInfo> urlGroup in urls.GroupBy(x => x.IsUrl).OrderByDescending(x => x.Key))
        {
            // in some cases there will be the same URL for multiple cultures:
            // * The entire branch is invariant
            // * If there are less domain/cultures assigned to the branch than the number of cultures/languages installed
            if (urlGroup.Key)
            {
                result.AddRange(urlGroup.DistinctBy(x => x.Text, StringComparer.OrdinalIgnoreCase).OrderBy(x => x.Text)
                    .ThenBy(x => x.Culture));
            }
            else
            {
                result.AddRange(urlGroup);
            }
        }

        // get the 'other' URLs - ie not what you'd get with GetUrl() but URLs that would route to the document, nevertheless.
        // for these 'other' URLs, we don't check whether they are routable, collide, anything - we just report them.
        foreach (UrlInfo otherUrl in publishedUrlProvider.GetOtherUrls(content.Id).OrderBy(x => x.Text)
                     .ThenBy(x => x.Culture))
        {
            // avoid duplicates
            if (urls.Add(otherUrl))
            {
                result.Add(otherUrl);
            }
        }

        return result;
    }

    /// <summary>
    ///     Tries to return a <see cref="UrlInfo" /> for each culture for the content while detecting collisions/errors
    /// </summary>
    private static async Task<IEnumerable<UrlInfo>> GetContentUrlsByCultureAsync(
        IContent content,
        IEnumerable<string> cultures,
        IPublishedRouter publishedRouter,
        IUmbracoContext umbracoContext,
        IContentService contentService,
        ILocalizedTextService textService,
        IVariationContextAccessor variationContextAccessor,
        ILogger logger,
        UriUtility uriUtility,
        IPublishedUrlProvider publishedUrlProvider)
    {
        var result = new List<UrlInfo>();

        foreach (var culture in cultures)
        {
            // if content is variant, and culture is not published, skip
            if (content.ContentType.VariesByCulture() && !content.IsCulturePublished(culture))
            {
                continue;
            }

            // if it's variant and culture is published, or if it's invariant, proceed
            string url;
            try
            {
                url = publishedUrlProvider.GetUrl(content.Id, culture: culture);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetUrl exception.");
                url = "#ex";
            }

            switch (url)
            {
                // deal with 'could not get the URL'
                case "#":
                    result.Add(HandleCouldNotGetUrl(content, culture, contentService, textService));
                    break;

                // deal with exceptions
                case "#ex":
                    result.Add(UrlInfo.Message(textService.Localize("content", "getUrlException"), culture));
                    break;

                // got a URL, deal with collisions, add URL
                default:
                    // detect collisions, etc
                    Attempt<UrlInfo?> hasCollision = await DetectCollisionAsync(logger, content, url, culture, umbracoContext, publishedRouter, textService, variationContextAccessor, uriUtility);
                    if (hasCollision.Success && hasCollision.Result is not null)
                    {
                        result.Add(hasCollision.Result);
                    }
                    else
                    {
                        result.Add(UrlInfo.Url(url, culture));
                    }

                    break;
            }
        }

        return result;
    }

    private static UrlInfo HandleCouldNotGetUrl(IContent content, string culture, IContentService contentService, ILocalizedTextService textService)
    {
        // document has a published version yet its URL is "#" => a parent must be
        // unpublished, walk up the tree until we find it, and report.
        IContent? parent = content;
        do
        {
            parent = parent.ParentId > 0 ? contentService.GetParent(parent) : null;
        }
        while (parent != null && parent.Published &&
                 (!parent.ContentType.VariesByCulture() || parent.IsCulturePublished(culture)));

        if (parent == null)
        {
            // oops, internal error
            return UrlInfo.Message(textService.Localize("content", "parentNotPublishedAnomaly"), culture);
        }

        if (!parent.Published)
        {
            // totally not published
            return UrlInfo.Message(textService.Localize("content", "parentNotPublished", new[] { parent.Name }), culture);
        }

        // culture not published
        return UrlInfo.Message(
            textService.Localize("content", "parentCultureNotPublished", new[] { parent.Name }),
            culture);
    }

    private static async Task<Attempt<UrlInfo?>> DetectCollisionAsync(
        ILogger logger,
        IContent content,
        string url,
        string culture,
        IUmbracoContext umbracoContext,
        IPublishedRouter publishedRouter,
        ILocalizedTextService textService,
        IVariationContextAccessor variationContextAccessor,
        UriUtility uriUtility)
    {
        // test for collisions on the 'main' URL
        var uri = new Uri(url.TrimEnd(Constants.CharArrays.ForwardSlash), UriKind.RelativeOrAbsolute);
        if (uri.IsAbsoluteUri == false)
        {
            uri = uri.MakeAbsolute(umbracoContext.CleanedUmbracoUrl);
        }

        uri = uriUtility.UriToUmbraco(uri);
        IPublishedRequestBuilder builder = await publishedRouter.CreateRequestAsync(uri);
        IPublishedRequest pcr =
            await publishedRouter.RouteRequestAsync(builder, new RouteRequestOptions(RouteDirection.Outbound));

        if (!pcr.HasPublishedContent())
        {
            const string logMsg = nameof(DetectCollisionAsync) +
                                  " did not resolve a content item for original url: {Url}, translated to {TranslatedUrl} and culture: {Culture}";
            logger.LogDebug(logMsg, url, uri, culture);

            var urlInfo = UrlInfo.Message(textService.Localize("content", "routeErrorCannotRoute"), culture);
            return Attempt.Succeed(urlInfo);
        }

        if (pcr.IgnorePublishedContentCollisions)
        {
            return Attempt<UrlInfo?>.Fail();
        }

        if (pcr.PublishedContent?.Id != content.Id)
        {
            IPublishedContent? o = pcr.PublishedContent;
            var l = new List<string>();
            while (o != null)
            {
                l.Add(o.Name(variationContextAccessor)!);
                o = o.Parent;
            }

            l.Reverse();
            var s = "/" + string.Join("/", l) + " (id=" + pcr.PublishedContent?.Id + ")";

            var urlInfo = UrlInfo.Message(textService.Localize("content", "routeError", new[] { s }), culture);
            return Attempt.Succeed(urlInfo);
        }

        // no collision
        return Attempt<UrlInfo?>.Fail();
    }
}
