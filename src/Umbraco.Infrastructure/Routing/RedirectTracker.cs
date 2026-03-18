using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Routing;

/// <summary>
/// Tracks and manages URL redirects for content items, ensuring that old routes are stored and appropriate redirects
/// are created when content URLs change.
/// </summary>
internal sealed class RedirectTracker : IRedirectTracker
{
    private static readonly Uri _placeholderUri = new("http://localhost");

    private readonly ILanguageService _languageService;
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IPublishedContentCache _contentCache;
    private readonly IDocumentNavigationQueryService _navigationQueryService;
    private readonly ILogger<RedirectTracker> _logger;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IPublishedContentStatusFilteringService _publishedContentStatusFilteringService;
    private readonly IDomainCache _domainCache;
    private readonly UrlSegmentProviderCollection _urlSegmentProviders;
    private readonly IDocumentUrlService _documentUrlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectTracker"/> class.
    /// </summary>
    public RedirectTracker(
        ILanguageService languageService,
        IRedirectUrlService redirectUrlService,
        IPublishedContentCache contentCache,
        IDocumentNavigationQueryService navigationQueryService,
        ILogger<RedirectTracker> logger,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedContentStatusFilteringService publishedContentStatusFilteringService,
        IDomainCache domainCache,
        UrlSegmentProviderCollection urlSegmentProviders,
        IDocumentUrlService documentUrlService)
    {
        _languageService = languageService;
        _redirectUrlService = redirectUrlService;
        _contentCache = contentCache;
        _navigationQueryService = navigationQueryService;
        _logger = logger;
        _publishedUrlProvider = publishedUrlProvider;
        _publishedContentStatusFilteringService = publishedContentStatusFilteringService;
        _domainCache = domainCache;
        _urlSegmentProviders = urlSegmentProviders;
        _documentUrlService = documentUrlService;
    }

    /// <inheritdoc/>
#pragma warning disable CS0618 // Type or member is obsolete
    public void StoreOldRoute(IContent entity, Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes)
#pragma warning restore CS0618 // Type or member is obsolete
        => StoreOldRoute(entity, oldRoutes, isMove: true);

    /// <inheritdoc/>
    public void StoreOldRoute(
        IContent entity,
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes,
        bool isMove)
    {
        IPublishedContent? entityContent = _contentCache.GetById(entity.Id);
        if (entityContent is null)
        {
            return;
        }

        // If this entity was already processed by an ancestor's traversal in this batch,
        // all its descendants will also have been processed — skip entirely to avoid redundant
        // cache lookups, segment checks, and navigation queries.
        if (oldRoutes.Keys.Any(k => k.ContentId == entityContent.Id))
        {
            return;
        }

        // For publishes (not moves), check if URL segment actually changed and whether any provider
        // derives descendant segments from this content's data.
        // If the segment is unchanged and no provider affects descendants, we don't need to traverse.
        // For moves, we have to assume all descendant URLs may have changed since the parent path is part of the URL.
        if (ShouldIgnoreForOldRouteStorage(entity, isMove, entityContent))
        {
            return;
        }

        // Get the default affected cultures by going up the tree until we find the first culture variant entity (default to no cultures).
        var defaultCultures = new Lazy<string[]>(() => entityContent.AncestorsOrSelf(_navigationQueryService, _publishedContentStatusFilteringService)
            .FirstOrDefault(a => a.Cultures.Any())?.Cultures.Keys.ToArray() ?? []);

        // Get the domains assigned to the content item again by looking up the tree (default to 0).
        var domainRootId = new Lazy<int>(() => GetNodeIdWithAssignedDomain(entityContent));

        // Get all language ISO codes (in case we're dealing with invariant content with variant ancestors)
        var languageIsoCodes = new Lazy<string[]>(() => [.. _languageService.GetAllIsoCodesAsync().GetAwaiter().GetResult()]);

        foreach (IPublishedContent publishedContent in entityContent.DescendantsOrSelf(_navigationQueryService, _publishedContentStatusFilteringService))
        {

            // If this entity defines specific cultures, use those instead of the default ones
            IEnumerable<string> cultures = publishedContent.Cultures.Any() ? publishedContent.Cultures.Keys : defaultCultures.Value;

            foreach (var culture in cultures)
            {
                try
                {
                    var route = _publishedUrlProvider.GetUrl(publishedContent.Key, UrlMode.Relative, culture).TrimEnd(Constants.CharArrays.ForwardSlash);
                    if (IsValidRoute(route))
                    {
                        StoreRoute(oldRoutes, publishedContent, culture, route, domainRootId.Value);
                    }
                    else if (string.IsNullOrEmpty(culture))
                    {
                        // Retry using all languages, if this is invariant but has a variant ancestor.
                        foreach (string languageIsoCode in languageIsoCodes.Value)
                        {
                            route = GetUrl(publishedContent.Key, languageIsoCode);
                            if (IsValidRoute(route))
                            {
                                StoreRoute(oldRoutes, publishedContent, languageIsoCode, route, domainRootId.Value);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not register redirects because the old route couldn't be retrieved for content ID {ContentId} and culture '{Culture}'.", publishedContent.Id, culture);
                }
            }
        }
    }

    private bool ShouldIgnoreForOldRouteStorage(IContent entity, bool isMove, IPublishedContent entityContent) =>
        isMove is false &&
            HasUrlSegmentChanged(entity, entityContent) is false &&
            HasProviderAffectingDescendantSegments(entity) is false;

    private bool HasUrlSegmentChanged(IContent entity, IPublishedContent publishedContent)
    {
        // During upgrades, the document URL service is not initialized (see DocumentUrlServiceInitializerNotificationHandler).
        // If a migration triggers content publishing before initialization, fall back to full traversal.
        if (_documentUrlService.IsInitialized is false)
        {
            return true;
        }

        foreach (var culture in GetCultures(publishedContent))
        {
            var currentPublishedSegment = _documentUrlService.GetUrlSegment(entity.Key, culture, isDraft: false);

            // In the unexpected case that the current published segment couldn't be retrieved (e.g. cache inconsistency),
            // we can't confirm the segment is unchanged — fall back to full traversal.
            // Otherwise, if the provider(s) that contribute to the segment detect a change, we need to traverse since the
            // URL of the current node and all descendents has changed.
            if (currentPublishedSegment is null || HasProviderDetectedSegmentChange(entity, currentPublishedSegment, culture))
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> GetCultures(IPublishedContent publishedContent) =>
        publishedContent.Cultures.Any()
            ? publishedContent.Cultures.Keys
            : [string.Empty];

    private bool HasProviderDetectedSegmentChange(IContent entity, string currentPublishedSegment, string culture)
    {
        // Check each provider to see if any detect a change in the URL segment for this content and culture.
        foreach (IUrlSegmentProvider provider in _urlSegmentProviders)
        {
            // Skip providers that don't produce a segment for this content/culture.
            if (string.IsNullOrEmpty(provider.GetUrlSegment(entity, published: false, culture)))
            {
                continue;
            }

            if (provider.HasUrlSegmentChanged(entity, currentPublishedSegment, culture))
            {
                return true;
            }

            // This provider handled the segment — don't check further providers unless it allows additional segments.
            if (provider.AllowAdditionalSegments is false)
            {
                return false;
            }
        }

        // No provider produced a segment, so none would have at publish time either — no change.
        return false;
    }

    private bool HasProviderAffectingDescendantSegments(IContent entity)
    {
        foreach (IUrlSegmentProvider provider in _urlSegmentProviders)
        {
            if (provider.MayAffectDescendantSegments(entity))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetNodeIdWithAssignedDomain(IPublishedContent entityContent, out int domainRootId)
    {
        domainRootId = GetNodeIdWithAssignedDomain(entityContent);
        return domainRootId > 0;
    }

    private int GetNodeIdWithAssignedDomain(IPublishedContent entityContent) =>
        entityContent.Path.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).Reverse()
            .FirstOrDefault(x => _domainCache.HasAssigned(x, includeWildcards: true));

    private string GetUrl(Guid contentKey, string languageIsoCode) =>
        _publishedUrlProvider.GetUrl(contentKey, UrlMode.Relative, languageIsoCode).TrimEnd(Constants.CharArrays.ForwardSlash);

    /// <summary>
    /// Strips the domain's path prefix from a relative URL so that the route stored for redirect
    /// lookup matches the format expected by <see cref="DomainUtilities.PathRelativeToDomain"/>.
    /// For example, given domain <c>example.com/en/</c> and route <c>/en/page</c>, returns <c>/page</c>.
    /// </summary>
    /// <param name="route">The relative URL (may include the domain path prefix).</param>
    /// <param name="domainRootId">The content node ID that has a domain assigned.</param>
    /// <param name="culture">The culture used to select the matching domain.</param>
    /// <returns>The route with the domain path prefix removed, or the original route if no domain is found.</returns>
    private string GetPathRelativeToDomain(string route, int domainRootId, string culture)
    {
        Domain[] domains = _domainCache.GetAssigned(domainRootId, false).ToArray();
        Domain? domain = domains
            .FirstOrDefault(d => d.IsWildcard is false &&
                                 d.Culture is not null &&
                                 d.Culture.Equals(culture, StringComparison.InvariantCultureIgnoreCase))
            ?? domains.FirstOrDefault(d => d.IsWildcard is false);

        if (domain is null)
        {
            return route;
        }

        Uri domainUri = DomainUtilities.ParseUriFromDomainName(domain.Name, _placeholderUri);
        return DomainUtilities.PathRelativeToDomain(domainUri, route);
    }

    private void StoreRoute(
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes,
        IPublishedContent publishedContent,
        string culture,
        string route,
        int domainRootId)
    {
        // Prepend the Id of the node with the associated domain to the route if there is one assigned.
        if (domainRootId > 0)
        {
            route = GetPathRelativeToDomain(route, domainRootId, culture);
            route = domainRootId + route;
        }

        oldRoutes[(publishedContent.Id, culture)] = (publishedContent.Key, route);
    }

    /// <inheritdoc/>
    public void CreateRedirects(IDictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes)
    {
        if (!oldRoutes.Any())
        {
            return;
        }

        foreach (((int contentId, string culture), (Guid contentKey, string oldRoute)) in oldRoutes)
        {
            IPublishedContent? entityContent = _contentCache.GetById(contentKey);
            if (entityContent is null)
            {
                continue;
            }

            try
            {
                var newRoute = GetUrl(contentKey, culture);

                // Prepend the Id of the node with the associated domain to the route if there is one assigned.
                if (TryGetNodeIdWithAssignedDomain(entityContent, out int domainRootId))
                {
                    newRoute = GetPathRelativeToDomain(newRoute, domainRootId, culture);
                    newRoute = domainRootId + newRoute;
                }

                if (!IsValidRoute(newRoute) || oldRoute == newRoute)
                {
                    continue;
                }

                // Ensure we don't create a self-referencing redirect. This can occur if a document is renamed and then the name is reverted back
                // to the original. We resolve this by removing any existing redirect that points to the new route.
                RemoveSelfReferencingRedirect(contentKey, newRoute);

                _redirectUrlService.Register(oldRoute, contentKey, culture);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not track redirects because the new route couldn't be retrieved for content ID {ContentId} and culture '{Culture}'.", contentId, culture);
            }
        }
    }

    private static bool IsValidRoute([NotNullWhen(true)] string? route) => route is not null && !route.StartsWith("err/");

    private void RemoveSelfReferencingRedirect(Guid contentKey, string route)
    {
        IEnumerable<IRedirectUrl> allRedirectUrls = _redirectUrlService.GetContentRedirectUrls(contentKey);
        foreach (IRedirectUrl redirectUrl in allRedirectUrls)
        {
            if (redirectUrl.Url == route)
            {
                _redirectUrlService.Delete(redirectUrl.Key);
            }
        }
    }
}
