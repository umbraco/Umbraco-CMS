using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Core.Xml.XPath;
using Umbraco.Web.PublishedCache.NuCache.Navigable;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class ContentCache : PublishedCacheBase, INavigableData, IDisposable
    {
        private readonly ContentStore.Snapshot _snapshot;
        private readonly IVariationContextAccessor _variationContextAccessor;

        internal ContentStore.Snapshot Snapshot => _snapshot;

        #region Constructor

        // TODO: figure this out
        // after the current snapshot has been resync-ed
        // it's too late for UmbracoContext which has captured previewDefault and stuff into these ctor vars
        // but, no, UmbracoContext returns snapshot.Content which comes from elements SO a resync should create a new cache

        public ContentCache(bool previewDefault, ContentStore.Snapshot snapshot, IVariationContextAccessor variationContextAccessor)
            : base(previewDefault)
        {
            _snapshot = snapshot;
            _variationContextAccessor = variationContextAccessor;
        }
        #endregion

        #region Get, Has

        public override IPublishedContent GetById(bool preview, int contentId)
        {
            var node = _snapshot.Get(contentId);
            return GetNodePublishedContent(node, preview);
        }

        public override IPublishedContent GetById(bool preview, Guid contentId)
        {
            var node = _snapshot.Get(contentId);
            return GetNodePublishedContent(node, preview);
        }

        public override IPublishedContent GetById(bool preview, Udi contentId)
        {
            var guidUdi = contentId as GuidUdi;
            if (guidUdi == null)
                throw new ArgumentException($"Udi must be of type {typeof(GuidUdi).Name}.", nameof(contentId));

            if (guidUdi.EntityType != Constants.UdiEntityType.Document)
                throw new ArgumentException($"Udi entity type must be \"{Constants.UdiEntityType.Document}\".", nameof(contentId));

            return GetById(preview, guidUdi.Guid);
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
            // handle context culture for variant
            if (culture == null)
                culture = _variationContextAccessor?.VariationContext?.Culture ?? "";

            // _snapshot.GetAtRoot() returns all ContentNode at root
            // both .Draft and .Published cannot be null at the same time
            // root is already sorted by sortOrder, and does not contain nulls
            //
            // GetNodePublishedContent may return null if !preview and there is no
            // published model, so we need to filter these nulls out

            var atRoot = _snapshot.GetAtRoot()
                .Select(n => GetNodePublishedContent(n, preview))
                .WhereNotNull();

            // if a culture is specified, we must ensure that it is avail/published
            if (culture != "*")
                atRoot = atRoot.Where(x => x.IsInvariantOrHasCulture(culture));

            return atRoot;
        }

        private static IPublishedContent GetNodePublishedContent(ContentNode node, bool preview)
        {
            if (node == null)
                return null;

            // both .Draft and .Published cannot be null at the same time

            return preview
                ? node.DraftModel ?? GetPublishedContentAsDraft(node.PublishedModel)
                : node.PublishedModel;
        }

        // gets a published content as a previewing draft, if preview is true
        // this is for published content when previewing
        private static IPublishedContent GetPublishedContentAsDraft(IPublishedContent content /*, bool preview*/)
        {
            if (content == null /*|| preview == false*/) return null; //content;

            // an object in the cache is either an IPublishedContentOrMedia,
            // or a model inheriting from PublishedContentExtended - in which
            // case we need to unwrap to get to the original IPublishedContentOrMedia.

            var inner = PublishedContent.UnwrapIPublishedContent(content);
            return inner.AsDraft();
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
