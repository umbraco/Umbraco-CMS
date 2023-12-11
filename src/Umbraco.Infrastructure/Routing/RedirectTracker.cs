using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Routing
{
    internal class RedirectTracker : IRedirectTracker
    {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly IRedirectUrlService _redirectUrlService;
        private readonly ILogger<RedirectTracker> _logger;

        public RedirectTracker(
            IUmbracoContextFactory umbracoContextFactory,
            IVariationContextAccessor variationContextAccessor,
            ILocalizationService localizationService,
            IRedirectUrlService redirectUrlService,
            ILogger<RedirectTracker> logger)
        {
            _umbracoContextFactory = umbracoContextFactory;
            _variationContextAccessor = variationContextAccessor;
            _localizationService = localizationService;
            _redirectUrlService = redirectUrlService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public void StoreOldRoute(IContent entity, Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes)
        {
            using UmbracoContextReference reference = _umbracoContextFactory.EnsureUmbracoContext();
            IPublishedContentCache? contentCache = reference.UmbracoContext.Content;
            IPublishedContent? entityContent = contentCache?.GetById(entity.Id);
            if (entityContent is null)
            {
                return;
            }

            // Get the default affected cultures by going up the tree until we find the first culture variant entity (default to no cultures)
            var defaultCultures = new Lazy<string[]>(() => entityContent.AncestorsOrSelf().FirstOrDefault(a => a.Cultures.Any())?.Cultures.Keys.ToArray() ?? Array.Empty<string>());

            // Get all language ISO codes (in case we're dealing with invariant content with variant ancestors)
            var languageIsoCodes = new Lazy<string[]>(() => _localizationService.GetAllLanguages().Select(x => x.IsoCode).ToArray());

            foreach (IPublishedContent publishedContent in entityContent.DescendantsOrSelf(_variationContextAccessor))
            {
                // If this entity defines specific cultures, use those instead of the default ones
                IEnumerable<string> cultures = publishedContent.Cultures.Any() ? publishedContent.Cultures.Keys : defaultCultures.Value;

                foreach (var culture in cultures)
                {
                    try
                    {
                        var route = contentCache?.GetRouteById(publishedContent.Id, culture);
                        if (IsValidRoute(route))
                        {
                            oldRoutes[(publishedContent.Id, culture)] = (publishedContent.Key, route);
                        }
                        else if (string.IsNullOrEmpty(culture))
                        {
                            // Retry using all languages, if this is invariant but has a variant ancestor.
                            foreach (string languageIsoCode in languageIsoCodes.Value)
                            {
                                route = contentCache?.GetRouteById(publishedContent.Id, languageIsoCode);
                                if (IsValidRoute(route))
                                {
                                    oldRoutes[(publishedContent.Id, languageIsoCode)] = (publishedContent.Key, route);
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

        /// <inheritdoc/>
        public void CreateRedirects(IDictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> oldRoutes)
        {
            if (!oldRoutes.Any())
            {
                return;
            }

            using UmbracoContextReference reference = _umbracoContextFactory.EnsureUmbracoContext();
            IPublishedContentCache? contentCache = reference.UmbracoContext.Content;
            if (contentCache == null)
            {
                _logger.LogWarning("Could not track redirects because there is no published content cache available on the current published snapshot.");
                return;
            }

            foreach (((int contentId, string culture), (Guid contentKey, string oldRoute)) in oldRoutes)
            {
                try
                {
                    var newRoute = contentCache.GetRouteById(contentId, culture);
                    if (!IsValidRoute(newRoute) || oldRoute == newRoute)
                    {
                        continue;
                    }

                    _redirectUrlService.Register(oldRoute, contentKey, culture);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not track redirects because the new route couldn't be retrieved for content ID {ContentId} and culture '{Culture}'.", contentId, culture);
                }
            }
        }

        private static bool IsValidRoute([NotNullWhen(true)] string? route) => route is not null && !route.StartsWith("err/");
    }
}
