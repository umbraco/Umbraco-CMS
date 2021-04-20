using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Xml.XPath;
using Umbraco.Web.PublishedCache.NuCache.Navigable;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class MemberCache : IPublishedMemberCache, INavigableData, IDisposable
    {
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public readonly IVariationContextAccessor VariationContextAccessor;
        private readonly IEntityXmlSerializer _entitySerializer;
        private readonly IAppCache _snapshotCache;
        private readonly IMemberService _memberService;
        private readonly PublishedContentTypeCache _contentTypeCache;
        private readonly bool _previewDefault;
        private bool _disposedValue;

        public MemberCache(bool previewDefault, IAppCache snapshotCache, IMemberService memberService, PublishedContentTypeCache contentTypeCache,
            IPublishedSnapshotAccessor publishedSnapshotAccessor, IVariationContextAccessor variationContextAccessor, IEntityXmlSerializer entitySerializer)
        {
            _snapshotCache = snapshotCache;
            _publishedSnapshotAccessor = publishedSnapshotAccessor;
            VariationContextAccessor = variationContextAccessor;
            _entitySerializer = entitySerializer;
            _memberService = memberService;
            _previewDefault = previewDefault;
            _contentTypeCache = contentTypeCache;
        }

        private T GetCacheItem<T>(string cacheKey, Func<T> getCacheItem)
            where T : class
        {
            var cache = _snapshotCache;
            return cache == null
                ? getCacheItem()
                : cache.GetCacheItem<T>(cacheKey, getCacheItem);
        }

        private static void EnsureProvider()
        {
            var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
            if (provider.IsUmbracoMembershipProvider() == false)
                throw new NotSupportedException("Cannot access this method unless the Umbraco membership provider is active");
        }

        public IPublishedContent GetById(bool preview, int memberId)
        {
            return GetById(memberId);
        }

        public IPublishedContent /*IPublishedMember*/ GetById(int memberId)
        {
            return GetCacheItem(CacheKeys.MemberCacheMember("ById", _previewDefault, memberId), () =>
                {
                    EnsureProvider();
                    var member = _memberService.GetById(memberId);
                    return member == null
                        ? null
                        : PublishedMember.Create(member, GetContentType(member.ContentTypeId), _previewDefault, _publishedSnapshotAccessor, VariationContextAccessor);
                });
        }

        private IPublishedContent /*IPublishedMember*/ GetById(IMember member, bool previewing)
        {
            return GetCacheItem(CacheKeys.MemberCacheMember("ById", _previewDefault, member.Id), () =>
                PublishedMember.Create(member, GetContentType(member.ContentTypeId), previewing, _publishedSnapshotAccessor, VariationContextAccessor));
        }

        public IPublishedContent /*IPublishedMember*/ GetByProviderKey(object key)
        {
            return GetCacheItem(CacheKeys.MemberCacheMember("ByProviderKey", _previewDefault, key), () =>
                {
                    EnsureProvider();
                    var member = _memberService.GetByProviderKey(key);
                    return member == null ? null : GetById(member, _previewDefault);
                });
        }

        public IPublishedContent /*IPublishedMember*/ GetByUsername(string username)
        {
            return GetCacheItem(CacheKeys.MemberCacheMember("ByUsername", _previewDefault, username), () =>
                {
                    EnsureProvider();
                    var member = _memberService.GetByUsername(username);
                    return member == null ? null : GetById(member, _previewDefault);
                });
        }

        public IPublishedContent /*IPublishedMember*/ GetByEmail(string email)
        {
            return GetCacheItem(CacheKeys.MemberCacheMember("ByEmail", _previewDefault, email), () =>
                {
                    EnsureProvider();
                    var member = _memberService.GetByEmail(email);
                    return member == null ? null : GetById(member, _previewDefault);
                });
        }

        public IPublishedContent /*IPublishedMember*/ GetByMember(IMember member)
        {
            return PublishedMember.Create(member, GetContentType(member.ContentTypeId), _previewDefault, _publishedSnapshotAccessor, VariationContextAccessor);
        }

        public IEnumerable<IPublishedContent> GetAtRoot(bool preview)
        {
            // because members are flat (not a tree) everything is at root
            // because we're loading everything... let's just not cache?
            var members = _memberService.GetAllMembers();
            return members.Select(m => PublishedMember.Create(m, GetContentType(m.ContentTypeId), preview, _publishedSnapshotAccessor, VariationContextAccessor));
        }

        public XPathNavigator CreateNavigator()
        {
            var source = new Source(this, false);
            var navigator = new NavigableNavigator(source);
            return navigator;
        }

        public XPathNavigator CreateNavigator(bool preview)
        {
            return CreateNavigator();
        }

        public XPathNavigator CreateNodeNavigator(int id, bool preview)
        {
            var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
            if (provider.IsUmbracoMembershipProvider() == false)
            {
                throw new NotSupportedException("Cannot access this method unless the Umbraco membership provider is active");
            }

            var result = _memberService.GetById(id);
            if (result == null) return null;

            var s = _entitySerializer.Serialize(result);
            var n = s.GetXmlNode();
            return n.CreateNavigator();
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

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _contentTypeCache.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
        }

        #endregion
    }
}
