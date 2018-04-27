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

        public DomainCache(IDomainService domainService, ISystemDefaultCultureAccessor systemDefaultCultureAccessor)
        {
            _domainService = domainService;
            DefaultCulture = systemDefaultCultureAccessor.DefaultCulture;
        }

        public IEnumerable<Domain> GetAll(bool includeWildcards)
        {
            return _domainService.GetAll(includeWildcards)
                .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, CultureInfo.GetCultureInfo(x.LanguageIsoCode), x.IsWildcard));
        }

        public IEnumerable<Domain> GetAssigned(int contentId, bool includeWildcards)
        {
            return _domainService.GetAssignedDomains(contentId, includeWildcards)
                 .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, CultureInfo.GetCultureInfo(x.LanguageIsoCode), x.IsWildcard));
        }

        public string DefaultCulture { get; }
    }
}
