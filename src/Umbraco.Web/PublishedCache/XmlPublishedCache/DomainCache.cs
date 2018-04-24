using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Web.Routing;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Core.Models;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    internal class DomainCache : IDomainCache
    {
        private readonly IDomainService _domainService;
        private readonly ILocalizationService _localizationService;

        public DomainCache(IDomainService domainService, ILocalizationService localizationService)
        {
            _domainService = domainService;
            _localizationService = localizationService;
        }

        public IEnumerable<Domain> GetAll(bool includeWildcards)
        {
            return _domainService.GetAll(includeWildcards)
                .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, CultureInfo.GetCultureInfo(x.LanguageIsoCode), x.IsWildcard, x.IsDefaultDomain(_localizationService)));
        }

        public IEnumerable<Domain> GetAssigned(int contentId, bool includeWildcards)
        {
            return _domainService.GetAssignedDomains(contentId, includeWildcards)
                 .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, CultureInfo.GetCultureInfo(x.LanguageIsoCode), x.IsWildcard, x.IsDefaultDomain(_localizationService)));
        }
    }
}
