using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Core.Xml.XPath;
using Umbraco.Web.PublishedCache.NuCache.Navigable;

namespace Umbraco.Web.PublishedCache.NuCache
{
    internal class MediaCache : PublishedCacheBase, IPublishedMediaCache2, INavigableData, IDisposable
    {
        private readonly ContentStore.Snapshot _snapshot;
        private readonly IVariationContextAccessor _variationContextAccessor;

        #region Constructors

        public MediaCache(bool previewDefault, ContentStore.Snapshot snapshot, IVariationContextAccessor variationContextAccessor)
            : base(previewDefault)
        {
            _snapshot = snapshot;
            _variationContextAccessor = variationContextAccessor;
        }

        #endregion

        #region Get, Has

        public override IPublishedContent GetById(bool preview, int contentId)
        {
            // ignore preview, there's only draft for media
            var n = _snapshot.Get(contentId);
            return n?.PublishedModel;
        }

        public override IPublishedContent GetById(bool preview, Guid contentId)
        {
            // ignore preview, there's only draft for media
            var n = _snapshot.Get(contentId);
            return n?.PublishedModel;
        }

        public override IPublishedContent GetById(bool preview, Udi contentId)
        {
            var guidUdi = contentId as GuidUdi;
            if (guidUdi == null)
                throw new ArgumentException($"Udi must be of type {typeof(GuidUdi).Name}.", nameof(contentId));

            if (guidUdi.EntityType != Constants.UdiEntityType.Media)
                throw new ArgumentException($"Udi entity type must be \"{Constants.UdiEntityType.Media}\".", nameof(contentId));

            // ignore preview, there's only draft for media
            var n = _snapshot.Get(guidUdi.Guid);
            return n?.PublishedModel;
        }

        public override bool HasById(bool preview, int contentId)
        {
            var n = _snapshot.Get(contentId);
            return n != null;
        }

        IEnumerable<IPublishedContent> INavigableData.GetAtRoot(bool preview) => GetAtRoot(preview);

        public override IEnumerable<IPublishedContent> GetAtRoot(bool preview, string culture = null)
        {
            // handle context culture for variant
            if (culture == null)
                culture = _variationContextAccessor?.VariationContext?.Culture ?? "";

            var atRoot = _snapshot.GetAtRoot().Select(x => x.PublishedModel);
            return culture == "*" ? atRoot : atRoot.Where(x => x.IsInvariantOrHasCulture(culture));
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

        public override IPublishedContentType GetContentType(int id) => _snapshot.GetContentType(id);

        public override IPublishedContentType GetContentType(string alias) => _snapshot.GetContentType(alias);

        public override IPublishedContentType GetContentType(Guid key) => _snapshot.GetContentType(key);

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _snapshot.Dispose();
        }

        #endregion
    }
}
