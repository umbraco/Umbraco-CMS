using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

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
        public IEnumerable<Domain> GetAll(bool includeWildcards) => _domainService.GetAll(includeWildcards)
                .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, x.LanguageIsoCode, x.IsWildcard));

        /// <inheritdoc />
        public IEnumerable<Domain> GetAssigned(int documentId, bool includeWildcards = false) => _domainService.GetAssignedDomains(documentId, includeWildcards)
                 .Where(x => x.RootContentId.HasValue && x.LanguageIsoCode.IsNullOrWhiteSpace() == false)
                .Select(x => new Domain(x.Id, x.DomainName, x.RootContentId.Value, x.LanguageIsoCode, x.IsWildcard));

        /// <inheritdoc />
        public bool HasAssigned(int documentId, bool includeWildcards = false)
            => documentId > 0 && GetAssigned(documentId, includeWildcards).Any();

        /// <inheritdoc />
        public string DefaultCulture { get; }
    }
}
