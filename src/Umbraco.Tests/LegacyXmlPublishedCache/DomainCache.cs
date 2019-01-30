using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.LegacyXmlPublishedCache
{
    internal class DomainCache : IDomainCache
    {
        private readonly IDomainService _domainService;

        public DomainCache(IDomainService domainService, IDefaultCultureAccessor defaultCultureAccessor)
        {
            _domainService = domainService;
            DefaultCulture = defaultCultureAccessor.DefaultCulture;
        }

        /// <inheritdoc />
        public IEnumerable<Domain> GetAll(bool includeWildcards)
        {
            return _domainService.GetAll(includeWildcards)
                .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, CultureInfo.GetCultureInfo(x.LanguageIsoCode), x.IsWildcard));
        }

        /// <inheritdoc />
        public IEnumerable<Domain> GetAssigned(int contentId, bool includeWildcards)
        {
            return _domainService.GetAssignedDomains(contentId, includeWildcards)
                 .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, CultureInfo.GetCultureInfo(x.LanguageIsoCode), x.IsWildcard));
        }

        public string DefaultCulture { get; }
    }
}
