using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Plugins;
using Umbraco.Core.Xml;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
    /// <summary>
    /// A class used to query for published content, media items
    /// </summary>
    public class PublishedContentQuery : ITypedPublishedContentQuery
    {
        private readonly ITypedPublishedContentQuery _typedContentQuery;
        private readonly IPublishedContentCache _contentCache;
        private readonly IPublishedMediaCache _mediaCache;

        /// <summary>
        /// Constructor used to return results from the caches
        /// </summary>
        /// <param name="contentCache"></param>
        /// <param name="mediaCache"></param>
        public PublishedContentQuery(IPublishedContentCache contentCache, IPublishedMediaCache mediaCache)
        {
            if (contentCache == null) throw new ArgumentNullException(nameof(contentCache));
            if (mediaCache == null) throw new ArgumentNullException(nameof(mediaCache));
            _contentCache = contentCache;
            _mediaCache = mediaCache;
        }

        /// <summary>
        /// Constructor used to wrap the ITypedPublishedContentQuery object passed in
        /// </summary>
        /// <param name="typedContentQuery"></param>
        public PublishedContentQuery(ITypedPublishedContentQuery typedContentQuery)
        {
            if (typedContentQuery == null) throw new ArgumentNullException(nameof(typedContentQuery));
            _typedContentQuery = typedContentQuery;
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


        #endregion

        #region Used by Content/Media

        private IPublishedContent TypedDocumentById(int id, IPublishedCache cache)
        {
            var doc = cache.GetById(id);
            return doc;
        }

        private IPublishedContent TypedDocumentByXPath(string xpath, XPathVariable[] vars, IPublishedContentCache cache)
        {
            var doc = cache.GetSingleByXPath(xpath, vars);
            return doc;
        }

        //NOTE: Not used?
        //private IPublishedContent TypedDocumentByXPath(XPathExpression xpath, XPathVariable[] vars, IPublishedContentCache cache)
        //{
        //    var doc = cache.GetSingleByXPath(xpath, vars);
        //    return doc;
        //}        

        private IEnumerable<IPublishedContent> TypedDocumentsByIds(IPublishedCache cache, IEnumerable<int> ids)
        {
            return ids.Select(eachId => TypedDocumentById(eachId, cache)).WhereNotNull();
        }

        private IEnumerable<IPublishedContent> TypedDocumentsByXPath(string xpath, XPathVariable[] vars, IPublishedContentCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private IEnumerable<IPublishedContent> TypedDocumentsByXPath(XPathExpression xpath, XPathVariable[] vars, IPublishedContentCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private IEnumerable<IPublishedContent> TypedDocumentsAtRoot(IPublishedCache cache)
        {
            return cache.GetAtRoot();
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