using System;
using System.Globalization;
using System.Linq;
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
                content.Id, content.Path,
                current);
        }

        /// <summary>
        /// Gets the culture that would be selected to render a specified content,
        /// within the context of a specified current request.
        /// </summary>
        /// <param name="umbracoContext">An <see cref="UmbracoContext"/> instance.</param>
        /// <param name="localizationService">An <see cref="ILocalizationService"/> implementation.</param>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="contentPath">The content path.</param>
        /// <param name="current">The request Uri.</param>
        /// <returns>The culture that would be selected to render the content.</returns>
        internal static CultureInfo GetCulture(UmbracoContext umbracoContext, ILocalizationService localizationService,
            int contentId, string contentPath, Uri current)
        {
            var route = umbracoContext.ContentCache.GetRouteById(contentId); // cached
            var pos = route.IndexOf('/');

            var domain = pos == 0
                ? null
                : DomainHelper.DomainForNode(int.Parse(route.Substring(0, pos)), current).Domain;

            if (domain == null)
            {
                var defaultLanguage = localizationService.GetAllLanguages().FirstOrDefault();
                return defaultLanguage == null ? CultureInfo.CurrentUICulture : new CultureInfo(defaultLanguage.IsoCode);
            }

            var wcDomain = DomainHelper.FindWildcardDomainInPath(DomainHelper.GetAllDomains(true), contentPath, domain.RootNodeId);
            return wcDomain == null
                ? new CultureInfo(domain.Language.CultureAlias)
                : new CultureInfo(wcDomain.Language.CultureAlias);
        }
    }
}
