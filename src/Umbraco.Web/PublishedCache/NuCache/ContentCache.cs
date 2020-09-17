using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Core.Xml.XPath;
using Umbraco.Web.PublishedCache.NuCache.Navigable;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class ContentCache : PublishedCacheBase, INavigableData, IDisposable
    {
        private readonly ContentStore.Snapshot _snapshot;
        private readonly SnapshotGetStrategy _snapshotGetStrategy;

        #region Constructor

        // TODO: figure this out
        // after the current snapshot has been resync-ed
        // it's too late for UmbracoContext which has captured previewDefault and stuff into these ctor vars
        // but, no, UmbracoContext returns snapshot.Content which comes from elements SO a resync should create a new cache

        public ContentCache(bool previewDefault, ContentStore.Snapshot snapshot, SnapshotGetStrategy snapshotByIdStrategy, IContentSnapshotAccessor snapshotAcessor)
            : base(previewDefault)
        {
            _snapshot = snapshot;
            snapshotAcessor.SetContentSnapshot(snapshot);
            _snapshotGetStrategy = snapshotByIdStrategy;
        }


        #endregion

        #region Get, Has

        public override IPublishedContent GetById(bool preview, int contentId)
        {
            return _snapshotGetStrategy.GetById(preview, contentId);
        }

        public override IPublishedContent GetById(bool preview, Guid contentId)
        {
            return _snapshotGetStrategy.GetById(preview, contentId);
        }

        public override IPublishedContent GetById(bool preview, Udi contentId)
        {
            return _snapshotGetStrategy.GetById(preview, contentId);
        }

        public override bool HasById(bool preview, int contentId)
        {
            var n = _snapshot.Get(contentId);
            if (n == null) return false;

            return preview || n.PublishedModel != null;
        }

        IEnumerable<IPublishedContent> INavigableData.GetAtRoot(bool preview) => GetAtRoot(preview);

        public override IEnumerable<IPublishedContent> GetAtRoot(bool preview, string culture = null)
        {
            return _snapshotGetStrategy.GetAtRoot(preview, culture);
        }

        public override bool HasContent(bool preview)
        {
            return preview
                ? _snapshot.IsEmpty == false
                : _snapshot.GetAtRoot().Any(x => x.PublishedModel != null);
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
            var xcontent = xnav?.UnderlyingObject as NavigableContent;
            return xcontent?.InnerContent;
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
            iterator = iterator.Clone();
            while (iterator.MoveNext())
            {
                var xnav = iterator.Current as NavigableNavigator;
                var xcontent = xnav?.UnderlyingObject as NavigableContent;
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

        public override IPublishedContentType GetContentType(int id) => _snapshot.GetContentType(id);

        public override IPublishedContentType GetContentType(string alias) => _snapshot.GetContentType(alias);

        public override IPublishedContentType GetContentType(Guid key) => _snapshot.GetContentType(key);

        #endregion

        #region IDisposable

        public override void Dispose()
        {
            _snapshot.Dispose();
        }

        #endregion
    }
}
