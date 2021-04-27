using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Extensions;
using Umbraco.Web.Composing;

namespace Umbraco.Tests.LegacyXmlPublishedCache
{
    class PublishedMemberCache : IPublishedMemberCache
    {
        private readonly PublishedContentTypeCache _contentTypeCache;
        private readonly IVariationContextAccessor _variationContextAccessor;

        public PublishedMemberCache(PublishedContentTypeCache contentTypeCache, IVariationContextAccessor variationContextAccessor)
        {
            _contentTypeCache = contentTypeCache;
            _variationContextAccessor = variationContextAccessor;
        }

        public IPublishedContent Get(IMember member)
        {
            var type = _contentTypeCache.Get(PublishedItemType.Member, member.ContentTypeId);
            return new PublishedMember(member, type, _variationContextAccessor)
                .CreateModel(Current.PublishedModelFactory);
        }

        #region Content types

        public IPublishedContentType GetContentType(int id) => _contentTypeCache.Get(PublishedItemType.Member, id);

        public IPublishedContentType GetContentType(string alias) => _contentTypeCache.Get(PublishedItemType.Member, alias);

        #endregion
    }
}
