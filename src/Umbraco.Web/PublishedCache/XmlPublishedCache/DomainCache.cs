using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    internal class DomainCache : IDomainCache
    {
        private readonly IDomainService _domainService;

        public DomainCache(IDomainService domainService, IDefaultCultureAccessor defaultCultureAccessor)
        {
            _domainService = domainService;
            DefaultCulture = defaultCultureAccessor.DefaultCulture;
        }

        /// <summary>
        /// Returns all <see cref="Domain"/> in the current domain cache including any domains that may be referenced by content items that are no longer published
        /// </summary>
        /// <param name="includeWildcards"></param>
        /// <returns></returns>
        public IEnumerable<Domain> GetAll(bool includeWildcards)
        {
            return _domainService.GetAll(includeWildcards)
                .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, CultureInfo.GetCultureInfo(x.LanguageIsoCode), x.IsWildcard));
        }

        /// <summary>
        /// Returns all assigned <see cref="Domain"/> for the content id specified even if the content item is not published
        /// </summary>
        /// <param name="contentId"></param>
        /// <param name="includeWildcards"></param>
        /// <returns></returns>
        public IEnumerable<Domain> GetAssigned(int contentId, bool includeWildcards)
        {
            return _domainService.GetAssignedDomains(contentId, includeWildcards)
                 .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, CultureInfo.GetCultureInfo(x.LanguageIsoCode), x.IsWildcard));
        }

        public string DefaultCulture { get; }
    }
}
