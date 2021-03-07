using System.Text;
using System.Xml;
using System.Xml.XPath;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using Umbraco.Web.Composing;
using Umbraco.Web.Security;

namespace Umbraco.Tests.LegacyXmlPublishedCache
{
    internal class PublishedMemberCache : IPublishedMemberCache
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

        public IPublishedContent GetByProviderKey(object key) => _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByProviderKey", key), () =>
                {
                    IMember result = _memberService.GetByProviderKey(key);
                    if (result == null)
                    {
                        return null;
                    }

                    IPublishedContentType type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type, _userService, _variationContextAccessor).CreateModel(Current.PublishedModelFactory);
                });

        public IPublishedContent GetById(int memberId) => _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetById", memberId), () =>
                {
                    IMember result = _memberService.GetById(memberId);
                    if (result == null)
                    {
                        return null;
                    }

                    IPublishedContentType type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type, _userService, _variationContextAccessor).CreateModel(Current.PublishedModelFactory);
                });

        public IPublishedContent GetByUsername(string username) => _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByUsername", username), () =>
                {
                    IMember result = _memberService.GetByUsername(username);
                    if (result == null)
                    {
                        return null;
                    }

                    IPublishedContentType type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type, _userService, _variationContextAccessor).CreateModel(Current.PublishedModelFactory);
                });

        public IPublishedContent GetByEmail(string email) => _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByEmail", email), () =>
                {
                    IMember result = _memberService.GetByEmail(email);
                    if (result == null)
                    {
                        return null;
                    }

                    IPublishedContentType type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type, _userService, _variationContextAccessor).CreateModel(Current.PublishedModelFactory);
                });

        public IPublishedContent GetByMember(IMember member)
        {
            IPublishedContentType type = _contentTypeCache.Get(PublishedItemType.Member, member.ContentTypeId);
            return new PublishedMember(member, type, _userService, _variationContextAccessor).CreateModel(Current.PublishedModelFactory);
        }

        public XPathNavigator CreateNavigator()
        {
            XmlDocument doc = _xmlStore.GetMemberXml();
            return doc.CreateNavigator();
        }

        public XPathNavigator CreateNavigator(bool preview) => CreateNavigator();

        public XPathNavigator CreateNodeNavigator(int id, bool preview)
        {
            XmlNode n = _xmlStore.GetMemberXmlNode(id);
            return n?.CreateNavigator();
        }

        private static string GetCacheKey(string key, params object[] additional)
        {
            var sb = new StringBuilder($"{typeof(MembershipHelper).Name}-{key}");
            foreach (var s in additional)
            {
                sb.Append("-");
                sb.Append(s);
            }
            return sb.ToString();
        }

        #region Content types

        public IPublishedContentType GetContentType(int id) => _contentTypeCache.Get(PublishedItemType.Member, id);

        public IPublishedContentType GetContentType(string alias) => _contentTypeCache.Get(PublishedItemType.Member, alias);

        #endregion
    }
}
