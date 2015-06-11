using System;
using System.Globalization;
using System.Linq;
using umbraco.cms.businesslogic.web;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Models
{
    public static class ContentExtensions
    {
        /// <summary>
        /// Gets the culture that would be selected to render a specified content,
        /// within the context of a specified current request.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="current">The request Uri.</param>
        /// <returns>The culture that would be selected to render the content.</returns>
        public static CultureInfo GetCulture(this IContent content, Uri current = null)
        {
            return GetCulture(UmbracoContext.Current,
                ApplicationContext.Current.Services.LocalizationService,
                ApplicationContext.Current.Services.ContentService,
                content.Id, content.Path,
                current);
        }

        /// <summary>
        /// Gets the culture that would be selected to render a specified content,
        /// within the context of a specified current request.
        /// </summary>
        /// <param name="umbracoContext">An <see cref="UmbracoContext"/> instance.</param>
        /// <param name="localizationService">An <see cref="ILocalizationService"/> implementation.</param>
        /// <param name="contentService">An <see cref="IContentService"/> implementation.</param>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="contentPath">The content path.</param>
        /// <param name="current">The request Uri.</param>
        /// <returns>The culture that would be selected to render the content.</returns>
        internal static CultureInfo GetCulture(UmbracoContext umbracoContext, ILocalizationService localizationService, IContentService contentService,
            int contentId, string contentPath, Uri current)
        {
            var route = umbracoContext == null
                ? null // for tests only
                : umbracoContext.ContentCache.GetRouteById(contentId); // cached

            Domain domain;

            if (route == null)
            {
                // if content is not published then route is null and we have to work
                // on non-published content (note: could optimize by checking routes?)

                var content = contentService.GetById(contentId);
                if (content == null) 
                    return GetDefaultCulture(localizationService);

                var hasDomain = DomainHelper.NodeHasDomains(content.Id);
                while (hasDomain == false && content != null)
                {
                    content = content.Parent();
                    hasDomain = content != null && DomainHelper.NodeHasDomains(content.Id);
                }

                domain = hasDomain ? DomainHelper.DomainForNode(content.Id, current).Domain : null;
            }
            else
            {
                // if content is published then we have a (cached) route
                // from which we can figure out the domain

                var pos = route.IndexOf('/');
                domain = pos == 0
                    ? null
                    : DomainHelper.DomainForNode(int.Parse(route.Substring(0, pos)), current).Domain;
            }

            if (domain == null)
                return GetDefaultCulture(localizationService);

            var wcDomain = DomainHelper.FindWildcardDomainInPath(DomainHelper.GetAllDomains(true), contentPath, domain.RootNodeId);
            return wcDomain == null
                ? new CultureInfo(domain.Language.CultureAlias)
                : new CultureInfo(wcDomain.Language.CultureAlias);
        }

        private static CultureInfo GetDefaultCulture(ILocalizationService localizationService)
        {
            var defaultLanguage = localizationService.GetAllLanguages().FirstOrDefault();
            return defaultLanguage == null ? CultureInfo.CurrentUICulture : new CultureInfo(defaultLanguage.IsoCode);
        }
    }
}
