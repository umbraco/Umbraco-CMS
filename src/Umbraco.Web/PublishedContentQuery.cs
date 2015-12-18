using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core.Xml;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
    /// <summary>
    /// A class used to query for published content, media items
    /// </summary>
    public class PublishedContentQuery : ITypedPublishedContentQuery, IDynamicPublishedContentQuery
    {
        private readonly ITypedPublishedContentQuery _typedContentQuery;
        private readonly IDynamicPublishedContentQuery _dynamicContentQuery;
        private readonly ContextualPublishedContentCache _contentCache;
        private readonly ContextualPublishedMediaCache _mediaCache;

        /// <summary>
        /// Constructor used to return results from the caches
        /// </summary>
        /// <param name="contentCache"></param>
        /// <param name="mediaCache"></param>
        public PublishedContentQuery(ContextualPublishedContentCache contentCache, ContextualPublishedMediaCache mediaCache)
        {
            if (contentCache == null) throw new ArgumentNullException("contentCache");
            if (mediaCache == null) throw new ArgumentNullException("mediaCache");
            _contentCache = contentCache;
            _mediaCache = mediaCache;
        }

        /// <summary>
        /// Constructor used to wrap the ITypedPublishedContentQuery and IDynamicPublishedContentQuery objects passed in
        /// </summary>
        /// <param name="typedContentQuery"></param>
        /// <param name="dynamicContentQuery"></param>
        public PublishedContentQuery(ITypedPublishedContentQuery typedContentQuery, IDynamicPublishedContentQuery dynamicContentQuery)
        {
            if (typedContentQuery == null) throw new ArgumentNullException("typedContentQuery");
            if (dynamicContentQuery == null) throw new ArgumentNullException("dynamicContentQuery");
            _typedContentQuery = typedContentQuery;
            _dynamicContentQuery = dynamicContentQuery;
        }

        #region Content

        public IPublishedContent TypedContent(int id)
        {
            return _typedContentQuery == null
                ? TypedDocumentById(id, _contentCache)
                : _typedContentQuery.TypedContent(id);
        }

        public IPublishedContent TypedContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return _typedContentQuery == null
                ? TypedDocumentByXPath(xpath, vars, _contentCache)
                : _typedContentQuery.TypedContentSingleAtXPath(xpath, vars);
        }
        
        public IEnumerable<IPublishedContent> TypedContent(IEnumerable<int> ids)
        {
            return _typedContentQuery == null
                ? TypedDocumentsByIds(_contentCache, ids)
                : _typedContentQuery.TypedContent(ids);
        }

        public IEnumerable<IPublishedContent> TypedContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return _typedContentQuery == null
                ? TypedDocumentsByXPath(xpath, vars, _contentCache)
                : _typedContentQuery.TypedContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> TypedContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return _typedContentQuery == null
                ? TypedDocumentsByXPath(xpath, vars, _contentCache)
                : _typedContentQuery.TypedContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> TypedContentAtRoot()
        {
            return _typedContentQuery == null
                ? TypedDocumentsAtRoot(_contentCache)
                : _typedContentQuery.TypedContentAtRoot();
        }

        public dynamic Content(int id)
        {
            return _dynamicContentQuery == null
                ? DocumentById(id, _contentCache, DynamicNull.Null)
                : _dynamicContentQuery.Content(id);
        }
        
        public dynamic ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return _dynamicContentQuery == null
                ? DocumentByXPath(xpath, vars, _contentCache, DynamicNull.Null)
                : _dynamicContentQuery.ContentSingleAtXPath(xpath, vars);
        }

        public dynamic ContentSingleAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return _dynamicContentQuery == null
                ? DocumentByXPath(xpath, vars, _contentCache, DynamicNull.Null)
                : _dynamicContentQuery.ContentSingleAtXPath(xpath, vars);
        }
        
        public dynamic Content(IEnumerable<int> ids)
        {
            return _dynamicContentQuery == null
                ? DocumentByIds(_contentCache, ids.ToArray())
                : _dynamicContentQuery.Content(ids);
        }

        public dynamic ContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return _dynamicContentQuery == null
                ? DocumentsByXPath(xpath, vars, _contentCache)
                : _dynamicContentQuery.ContentAtXPath(xpath, vars);
        }

        public dynamic ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return _dynamicContentQuery == null
                ? DocumentsByXPath(xpath, vars, _contentCache)
                : _dynamicContentQuery.ContentAtXPath(xpath, vars);
        }

        public dynamic ContentAtRoot()
        {
            return _dynamicContentQuery == null
                ? DocumentsAtRoot(_contentCache)
                : _dynamicContentQuery.ContentAtRoot();
        }

        #endregion

        #region Media
        
        public IPublishedContent TypedMedia(int id)
        {
            return _typedContentQuery == null
                ? TypedDocumentById(id, _mediaCache)
                : _typedContentQuery.TypedMedia(id);
        }
        
        public IEnumerable<IPublishedContent> TypedMedia(IEnumerable<int> ids)
        {
            return _typedContentQuery == null
                ? TypedDocumentsByIds(_mediaCache, ids)
                : _typedContentQuery.TypedMedia(ids);
        }

        public IEnumerable<IPublishedContent> TypedMediaAtRoot()
        {
            return _typedContentQuery == null
                ? TypedDocumentsAtRoot(_mediaCache)
                : _typedContentQuery.TypedMediaAtRoot();
        }

        public dynamic Media(int id)
        {
            return _dynamicContentQuery == null
                ? DocumentById(id, _mediaCache, DynamicNull.Null)
                : _dynamicContentQuery.Media(id);
        }
        
        public dynamic Media(IEnumerable<int> ids)
        {
            return _dynamicContentQuery == null
                ? DocumentByIds(_mediaCache, ids)
                : _dynamicContentQuery.Media(ids);
        }

        public dynamic MediaAtRoot()
        {
            return _dynamicContentQuery == null
                ? DocumentsAtRoot(_mediaCache)
                : _dynamicContentQuery.MediaAtRoot();
        }

        #endregion

        #region Used by Content/Media

        private IPublishedContent TypedDocumentById(int id, ContextualPublishedCache cache)
        {
            var doc = cache.GetById(id);
            return doc;
        }

        private IPublishedContent TypedDocumentByXPath(string xpath, XPathVariable[] vars, ContextualPublishedContentCache cache)
        {
            var doc = cache.GetSingleByXPath(xpath, vars);
            return doc;
        }

        //NOTE: Not used?
        //private IPublishedContent TypedDocumentByXPath(XPathExpression xpath, XPathVariable[] vars, ContextualPublishedContentCache cache)
        //{
        //    var doc = cache.GetSingleByXPath(xpath, vars);
        //    return doc;
        //}        

        private IEnumerable<IPublishedContent> TypedDocumentsByIds(ContextualPublishedCache cache, IEnumerable<int> ids)
        {
            return ids.Select(eachId => TypedDocumentById(eachId, cache)).WhereNotNull();
        }

        private IEnumerable<IPublishedContent> TypedDocumentsByXPath(string xpath, XPathVariable[] vars, ContextualPublishedContentCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private IEnumerable<IPublishedContent> TypedDocumentsByXPath(XPathExpression xpath, XPathVariable[] vars, ContextualPublishedContentCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private IEnumerable<IPublishedContent> TypedDocumentsAtRoot(ContextualPublishedCache cache)
        {
            return cache.GetAtRoot();
        }

        private dynamic DocumentById(int id, ContextualPublishedCache cache, object ifNotFound)
        {
            var doc = cache.GetById(id);
            return doc == null
                       ? ifNotFound
                       : new DynamicPublishedContent(doc).AsDynamic();
        }

        private dynamic DocumentByXPath(string xpath, XPathVariable[] vars, ContextualPublishedCache cache, object ifNotFound)
        {
            var doc = cache.GetSingleByXPath(xpath, vars);
            return doc == null
                       ? ifNotFound
                       : new DynamicPublishedContent(doc).AsDynamic();
        }

        private dynamic DocumentByXPath(XPathExpression xpath, XPathVariable[] vars, ContextualPublishedCache cache, object ifNotFound)
        {
            var doc = cache.GetSingleByXPath(xpath, vars);
            return doc == null
                       ? ifNotFound
                       : new DynamicPublishedContent(doc).AsDynamic();
        }

        private dynamic DocumentByIds(ContextualPublishedCache cache, IEnumerable<int> ids)
        {
            var dNull = DynamicNull.Null;
            var nodes = ids.Select(eachId => DocumentById(eachId, cache, dNull))
                           .Where(x => TypeHelper.IsTypeAssignableFrom<DynamicNull>(x) == false)
                           .Cast<DynamicPublishedContent>();
            return new DynamicPublishedContentList(nodes);
        }

        private dynamic DocumentsByXPath(string xpath, XPathVariable[] vars, ContextualPublishedCache cache)
        {
            return new DynamicPublishedContentList(
                cache.GetByXPath(xpath, vars)
                     .Select(publishedContent => new DynamicPublishedContent(publishedContent))
                );
        }

        private dynamic DocumentsByXPath(XPathExpression xpath, XPathVariable[] vars, ContextualPublishedCache cache)
        {
            return new DynamicPublishedContentList(
                cache.GetByXPath(xpath, vars)
                     .Select(publishedContent => new DynamicPublishedContent(publishedContent))
                );
        }

        private dynamic DocumentsAtRoot(ContextualPublishedCache cache)
        {
            return new DynamicPublishedContentList(
                cache.GetAtRoot()
                     .Select(publishedContent => new DynamicPublishedContent(publishedContent))
                );
        }

        #endregion

        #region Search

        /// <summary>
        /// Searches content
        /// </summary>
        /// <param name="term"></param>
        /// <param name="useWildCards"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        public dynamic Search(string term, bool useWildCards = true, string searchProvider = null)
        {
            return _dynamicContentQuery == null
                ? new DynamicPublishedContentList(
                    TypedSearch(term, useWildCards, searchProvider))
                : _dynamicContentQuery.Search(term, useWildCards, searchProvider);
        }

        /// <summary>
        /// Searhes content
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        public dynamic Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
        {
            return _dynamicContentQuery == null
                ? new DynamicPublishedContentList(
                    TypedSearch(criteria, searchProvider))
                : _dynamicContentQuery.Search(criteria, searchProvider);
        }

        /// <summary>
        /// Searches content
        /// </summary>
        /// <param name="term"></param>
        /// <param name="useWildCards"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> TypedSearch(string term, bool useWildCards = true, string searchProvider = null)
        {
            if (_typedContentQuery != null) return _typedContentQuery.TypedSearch(term, useWildCards, searchProvider);

            var searcher = Examine.ExamineManager.Instance.DefaultSearchProvider;
            if (string.IsNullOrEmpty(searchProvider) == false)
                searcher = Examine.ExamineManager.Instance.SearchProviderCollection[searchProvider];

            var results = searcher.Search(term, useWildCards);
            return results.ConvertSearchResultToPublishedContent(_contentCache);
        }

        /// <summary>
        /// Searhes content
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> TypedSearch(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
        {
            if (_typedContentQuery != null) return _typedContentQuery.TypedSearch(criteria, searchProvider);

            var s = Examine.ExamineManager.Instance.DefaultSearchProvider;
            if (searchProvider != null)
                s = searchProvider;

            var results = s.Search(criteria);
            return results.ConvertSearchResultToPublishedContent(_contentCache);
        }

        #endregion
    }
}