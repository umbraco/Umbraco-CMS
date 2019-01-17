using System;
using System.Text;
using System.Xml.XPath;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Web.Security;

namespace Umbraco.Web.PublishedCache.XmlPublishedCache
{
    class PublishedMemberCache : IPublishedMemberCache
    {
        private readonly IMemberService _memberService;
        private readonly IAppCache _requestCache;
        private readonly XmlStore _xmlStore;
        private readonly PublishedContentTypeCache _contentTypeCache;

        public PublishedMemberCache(XmlStore xmlStore, IAppCache requestCacheProvider, IMemberService memberService, PublishedContentTypeCache contentTypeCache)
        {
            _requestCache = requestCacheProvider;
            _memberService = memberService;
            _xmlStore = xmlStore;
            _contentTypeCache = contentTypeCache;
        }

        public IPublishedContent GetByProviderKey(object key)
        {
            return _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByProviderKey", key), () =>
                {
                    var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
                    if (provider.IsUmbracoMembershipProvider() == false)
                    {
                        throw new NotSupportedException("Cannot access this method unless the Umbraco membership provider is active");
                    }

                    var result = _memberService.GetByProviderKey(key);
                    if (result == null) return null;
                    var type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type).CreateModel();
                });
        }

        public IPublishedContent GetById(int memberId)
        {
            return _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetById", memberId), () =>
                {
                    var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
                    if (provider.IsUmbracoMembershipProvider() == false)
                    {
                        throw new NotSupportedException("Cannot access this method unless the Umbraco membership provider is active");
                    }

                    var result = _memberService.GetById(memberId);
                    if (result == null) return null;
                    var type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type).CreateModel();
                });
        }

        public IPublishedContent GetByUsername(string username)
        {
            return _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByUsername", username), () =>
                {
                    var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
                    if (provider.IsUmbracoMembershipProvider() == false)
                    {
                        throw new NotSupportedException("Cannot access this method unless the Umbraco membership provider is active");
                    }

                    var result = _memberService.GetByUsername(username);
                    if (result == null) return null;
                    var type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type).CreateModel();
                });
        }

        public IPublishedContent GetByEmail(string email)
        {
            return _requestCache.GetCacheItem<IPublishedContent>(
                GetCacheKey("GetByEmail", email), () =>
                {
                    var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
                    if (provider.IsUmbracoMembershipProvider() == false)
                    {
                        throw new NotSupportedException("Cannot access this method unless the Umbraco membership provider is active");
                    }

                    var result = _memberService.GetByEmail(email);
                    if (result == null) return null;
                    var type = _contentTypeCache.Get(PublishedItemType.Member, result.ContentTypeId);
                    return new PublishedMember(result, type).CreateModel();
                });
        }

        public IPublishedContent GetByMember(IMember member)
        {
            var type = _contentTypeCache.Get(PublishedItemType.Member, member.ContentTypeId);
            return new PublishedMember(member, type).CreateModel();
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

        public PublishedContentType GetContentType(int id)
        {
            return _contentTypeCache.Get(PublishedItemType.Member, id);
        }

        public PublishedContentType GetContentType(string alias)
        {
            return _contentTypeCache.Get(PublishedItemType.Member, alias);
        }

        #endregion
    }
}
