using System.Text;
using System.Xml.XPath;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Core.Cache;
using Umbraco.Web.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Security;

namespace Umbraco.Tests.LegacyXmlPublishedCache
{
    class PublishedMemberCache : IPublishedMemberCache
    {
        private readonly IMemberService _memberService;
        private readonly IAppCache _requestCache;
        private readonly XmlStore _xmlStore;
        private readonly PublishedContentTypeCache _contentTypeCache;
        private readonly IUserService _userService;
        private readonly IVariationContextAccessor _variationContextAccessor;

        public PublishedMemberCache(XmlStore xmlStore, IAppCache requestCache, IMemberService memberService, PublishedContentTypeCache contentTypeCache, IUserService userService, IVariationContextAccessor variationContextAccessor)
        {
            _requestCache = requestCache;
            _memberService = memberService;
            _xmlStore = xmlStore;
            _contentTypeCache = contentTypeCache;
            _userService = userService;
            _variationContextAccessor = variationContextAccessor;
        }

        public IPublishedContent GetByProviderKey(object key)
        {
            return _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByProviderKey", key), () =>
                {
                    var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

                    var result = _memberService.GetByProviderKey(key);
                    if (result == null) return null;
                    var type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type, _userService, _variationContextAccessor).CreateModel(Current.PublishedModelFactory);
                });
        }

        public IPublishedContent GetById(int memberId)
        {
            return _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetById", memberId), () =>
                {
                    var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

                    var result = _memberService.GetById(memberId);
                    if (result == null) return null;
                    var type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type, _userService, _variationContextAccessor).CreateModel(Current.PublishedModelFactory);
                });
        }

        public IPublishedContent GetByUsername(string username)
        {
            return _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByUsername", username), () =>
                {
                    var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

                    var result = _memberService.GetByUsername(username);
                    if (result == null) return null;
                    var type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type, _userService, _variationContextAccessor).CreateModel(Current.PublishedModelFactory);
                });
        }

        public IPublishedContent GetByEmail(string email)
        {
            return _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByEmail", email), () =>
                {
                    var provider = MembershipProviderExtensions.GetMembersMembershipProvider();

                    var result = _memberService.GetByEmail(email);
                    if (result == null) return null;
                    var type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type, _userService, _variationContextAccessor).CreateModel(Current.PublishedModelFactory);
                });
        }

        public IPublishedContent GetByMember(IMember member)
        {
            var type = _contentTypeCache.Get(PublishedItemType.Member, member.ContentTypeId);
            return new PublishedMember(member, type, _userService, _variationContextAccessor).CreateModel(Current.PublishedModelFactory);
        }

        public XPathNavigator CreateNavigator()
        {
            var doc = _xmlStore.GetMemberXml();
            return doc.CreateNavigator();
        }

        public XPathNavigator CreateNavigator(bool preview)
        {
            return CreateNavigator();
        }

        public XPathNavigator CreateNodeNavigator(int id, bool preview)
        {
            var n = _xmlStore.GetMemberXmlNode(id);
            return n?.CreateNavigator();
        }

        private static string GetCacheKey(string key, params object[] additional)
        {
            var sb = new StringBuilder($"{typeof (MembershipHelper).Name}-{key}");
            foreach (var s in additional)
            {
                sb.Append("-");
                sb.Append(s);
            }
            return sb.ToString();
        }

        #region Content types

        public IPublishedContentType GetContentType(int id)
        {
            return _contentTypeCache.Get(PublishedItemType.Member, id);
        }

        public IPublishedContentType GetContentType(string alias)
        {
            return _contentTypeCache.Get(PublishedItemType.Member, alias);
        }

        #endregion
    }
}
