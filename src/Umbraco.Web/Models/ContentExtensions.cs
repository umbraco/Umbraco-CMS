using System;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
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
            var route = UmbracoContext.Current.ContentCache.GetRouteById(content.Id); // cached
            var pos = route.IndexOf('/');

            var domainHelper = new DomainHelper(ApplicationContext.Current.Services.DomainService);

            var domain = pos == 0
                ? null
                : domainHelper.DomainForNode(int.Parse(route.Substring(0, pos)), current).UmbracoDomain;

            if (domain == null)
            {
                var defaultLanguage = ApplicationContext.Current.Services.LocalizationService.GetAllLanguages().FirstOrDefault();
                return defaultLanguage == null ? CultureInfo.CurrentUICulture : new CultureInfo(defaultLanguage.IsoCode);
            }

            var wcDomain = DomainHelper.FindWildcardDomainInPath(ApplicationContext.Current.Services.DomainService.GetAll(true), content.Path, domain.RootContent.Id);
            return wcDomain == null
                ? new CultureInfo(domain.Language.IsoCode)
                : new CultureInfo(wcDomain.Language.IsoCode);
        }
    }
}
