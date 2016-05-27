using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Core.Xml.XPath;
using Umbraco.Web.PublishedCache.NuCache.Navigable;

namespace Umbraco.Web.PublishedCache.NuCache
{
    class MediaCache : PublishedCacheBase, IPublishedMediaCache, INavigableData, IDisposable
    {
        private readonly ContentStore2.Snapshot _snapshot;

        #region Constructors

        public MediaCache(bool previewDefault, ContentStore2.Snapshot snapshot)
            : base(previewDefault)
        {
            _snapshot = snapshot;
        }

        #endregion

        #region Get, Has

        public override IPublishedContent GetById(bool preview, int contentId)
        {
            var n = _snapshot.Get(contentId);
            return n == null ? null : n.Published;
        }

        public override bool HasById(bool preview, int contentId)
        {
            var n = _snapshot.Get(contentId);
            return n != null;
        }

        public override IEnumerable<IPublishedContent> GetAtRoot(bool preview)
        {
            if (FacadeService.CacheContentCacheRoots == false)
                return GetAtRootNoCache(preview);

            var facade = Facade.Current;
            var cache = (facade == null)
                ? null
                : (preview == false || FacadeService.FullCacheWhenPreviewing
                    ? facade.SnapshotCache
                    : facade.FacadeCache);

            if (cache == null)
                return GetAtRootNoCache(preview);

            // note: ToArray is important here, we want to cache the result, not the function!
            return (IEnumerable<IPublishedContent>)cache.GetCacheItem(
                CacheKeys.MediaCacheRoots(preview),
                () => GetAtRootNoCache(preview).ToArray());
        }

        private IEnumerable<IPublishedContent> GetAtRootNoCache(bool preview)
        {
            var c = _snapshot.GetAtRoot();

            // there's no .Draft for medias, only non-null .Published
            // but we may want published as previewing, still
            return c.Select(n => preview
                ? ContentCache.GetPublishedContentAsPreviewing(n.Published)
                : n.Published);
        }

        public override bool HasContent(bool preview)
        {
            return _snapshot.IsEmpty == false;
        }

        #endregion

        #region XPath

        public override IPublishedContent GetSingleByXPath(bool preview, string xpath, XPathVariable[] vars)
        {
            var navigator = CreateNavigator(preview);
            var iterator = navigator.Select(xpath, vars);
            return GetSingleByXPath(iterator);
        }

        public override IPublishedContent GetSingleByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars)
        {
            var navigator = CreateNavigator(preview);
            var iterator = navigator.Select(xpath, vars);
            return GetSingleByXPath(iterator);
        }

        private static IPublishedContent GetSingleByXPath(XPathNodeIterator iterator)
        {
            if (iterator.MoveNext() == false) return null;

            var xnav = iterator.Current as NavigableNavigator;
            if (xnav == null) return null;

            var xcontent = xnav.UnderlyingObject as NavigableContent;
            return xcontent == null ? null : xcontent.InnerContent;
        }

        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, XPathVariable[] vars)
        {
            var navigator = CreateNavigator(preview);
            var iterator = navigator.Select(xpath, vars);
            return GetByXPath(iterator);
        }

        public override IEnumerable<IPublishedContent> GetByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars)
        {
            var navigator = CreateNavigator(preview);
            var iterator = navigator.Select(xpath, vars);
            return GetByXPath(iterator);
        }

        private static IEnumerable<IPublishedContent> GetByXPath(XPathNodeIterator iterator)
        {
            while (iterator.MoveNext())
            {
                var xnav = iterator.Current as NavigableNavigator;
                if (xnav == null) continue;

                var xcontent = xnav.UnderlyingObject as NavigableContent;
                if (xcontent == null) continue;

                yield return xcontent.InnerContent;
            }
        }

        public override XPathNavigator CreateNavigator(bool preview)
        {
            var source = new Source(this, preview);
            var navigator = new NavigableNavigator(source);
            return navigator;
        }

        public override XPathNavigator CreateNodeNavigator(int id, bool preview)
        {
            var source = new Source(this, preview);
            var navigator = new NavigableNavigator(source);
            return navigator.CloneWithNewRoot(id, 0);
        }

        #endregion

        #region Content types

        public override PublishedContentType GetContentType(int id)
        {
            return _snapshot.GetContentType(id);
        }

        public override PublishedContentType GetContentType(string alias)
        {
            return _snapshot.GetContentType(alias);
        }

        public override IEnumerable<IPublishedContent> GetByContentType(PublishedContentType contentType)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _snapshot.Dispose();
        }

        #endregion
    }
}
