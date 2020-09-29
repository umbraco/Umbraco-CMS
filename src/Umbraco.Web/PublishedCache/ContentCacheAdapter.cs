using System;
using System.Collections.Generic;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Web.PublishedCache.NuCache;

namespace Umbraco.Web.PublishedCache
{
    internal class ContentCacheAdapter : IPublishedContentCache, IPublishedContentCache2
    {
        private readonly IPublishedCache2 _contentCache;
        private readonly ContentCache _contentCache1;
        private readonly IContentRouter _contentRouter;
        private readonly IDomainCache _domainCache;

        public ContentCacheAdapter(ContentCache contentCache, IContentRouter contentRouter,IDomainCache domainCache)
        {
            _contentCache = contentCache as IPublishedCache2;
            _contentCache1 = contentCache;
            _contentRouter = contentRouter;
            _domainCache = domainCache;
        }

        public XPathNavigator CreateNavigator(bool preview)
        {
            return _contentCache.CreateNavigator(preview);
        }

        public XPathNavigator CreateNavigator()
        {
            return _contentCache.CreateNavigator();
        }

        public XPathNavigator CreateNodeNavigator(int id, bool preview)
        {
            return _contentCache.CreateNodeNavigator(id, preview);
        }

        public void Dispose()
        {
            _contentCache1.Dispose();
        }

        public IEnumerable<IPublishedContent> GetAtRoot(bool preview, string culture = null)
        {
            return _contentCache.GetAtRoot(preview, culture);
        }

        public IEnumerable<IPublishedContent> GetAtRoot(string culture = null)
        {
            return _contentCache.GetAtRoot(culture);
        }

        public IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType)
        {
            return _contentCache.GetByContentType(contentType);
        }

        public IPublishedContent GetById(bool preview, int contentId)
        {
            return _contentCache.GetById(preview, contentId);
        }

        public IPublishedContent GetById(bool preview, Guid contentId)
        {
            return _contentCache.GetById(preview, contentId);
        }

        public IPublishedContent GetById(bool preview, Udi contentId)
        {
            return _contentCache.GetById(preview, contentId);
        }

        public IPublishedContent GetById(int contentId)
        {
            return _contentCache.GetById(contentId);
        }

        public IPublishedContent GetById(Guid contentId)
        {
            return _contentCache.GetById(contentId);
        }

        public IPublishedContent GetById(Udi contentId)
        {
            return _contentCache.GetById(contentId);
        }

        public IPublishedContent GetByRoute(bool preview, string route, bool? hideTopLevelNode = null, string culture = null)
        {
            return _contentCache.GetById(_contentRouter.GetIdByRoute(_contentCache, preview, route, hideTopLevelNode, culture).Id);
        }

        public IPublishedContent GetByRoute(string route, bool? hideTopLevelNode = null, string culture = null)
        {
            bool preview = _contentCache1.PreviewDefault;
            return _contentCache.GetById(_contentRouter.GetIdByRoute(_contentCache, preview,route, hideTopLevelNode, culture).Id);
        }

        public IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, params XPathVariable[] vars)
        {
            return _contentCache.GetByXPath(preview, xpath, vars);
        }

        public IEnumerable<IPublishedContent> GetByXPath(string xpath, params XPathVariable[] vars)
        {
            return _contentCache.GetByXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> GetByXPath(bool preview, XPathExpression xpath, params XPathVariable[] vars)
        {
            return _contentCache.GetByXPath(preview, xpath, vars);
        }

        public IEnumerable<IPublishedContent> GetByXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return _contentCache.GetByXPath(xpath, vars);
        }

        public IPublishedContentType GetContentType(Guid key)
        {
            return _contentCache1.GetContentType(key);
        }

        public IPublishedContentType GetContentType(int id)
        {
            return _contentCache.GetContentType(id);
        }

        public IPublishedContentType GetContentType(string alias)
        {
            return _contentCache.GetContentType(alias);
        }

        public string GetRouteById(bool preview, int contentId, string culture = null)
        {
            return _contentRouter.GetRouteById(_contentCache, _domainCache,preview, contentId, culture);
        }

        public string GetRouteById(int contentId, string culture = null)
        {
            return _contentRouter.GetRouteById(_contentCache1.PreviewDefault, _contentCache, _domainCache, contentId, culture);
        }

        public IPublishedContent GetSingleByXPath(bool preview, string xpath, params XPathVariable[] vars)
        {
            return _contentCache.GetSingleByXPath(preview, xpath, vars);
        }

        public IPublishedContent GetSingleByXPath(string xpath, params XPathVariable[] vars)
        {
            return _contentCache.GetSingleByXPath(xpath, vars);
        }

        public IPublishedContent GetSingleByXPath(bool preview, XPathExpression xpath, params XPathVariable[] vars)
        {
            return _contentCache.GetSingleByXPath(preview, xpath, vars);
        }

        public IPublishedContent GetSingleByXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return _contentCache.GetSingleByXPath(xpath, vars);
        }

        public bool HasById(bool preview, int contentId)
        {
            return _contentCache.HasById(preview, contentId);
        }

        public bool HasById(int contentId)
        {
            return _contentCache.HasById(contentId);
        }

        public bool HasContent(bool preview)
        {
            return _contentCache.HasContent(preview);
        }

        public bool HasContent()
        {
            return _contentCache.HasContent();
        }
    }
}
