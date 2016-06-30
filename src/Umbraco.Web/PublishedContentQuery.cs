using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
    /// <summary>
    /// A class used to query for published content, media items
    /// </summary>
    public class PublishedContentQuery : IPublishedContentQuery
    {
        private readonly IPublishedContentQuery _query;
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
        /// <param name="query"></param>
        public PublishedContentQuery(IPublishedContentQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            _query = query;
        }

        #region Content

        public IPublishedContent Content(int id)
        {
            return _query == null
                ? ItemById(id, _contentCache)
                : _query.Content(id);
        }

        public IPublishedContent ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return _query == null
                ? ItemByXPath(xpath, vars, _contentCache)
                : _query.ContentSingleAtXPath(xpath, vars);
        }
        
        public IEnumerable<IPublishedContent> Content(IEnumerable<int> ids)
        {
            return _query == null
                ? ItemsByIds(_contentCache, ids)
                : _query.Content(ids);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return _query == null
                ? ItemsByXPath(xpath, vars, _contentCache)
                : _query.ContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return _query == null
                ? ItemsByXPath(xpath, vars, _contentCache)
                : _query.ContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> ContentAtRoot()
        {
            return _query == null
                ? ItemsAtRoot(_contentCache)
                : _query.ContentAtRoot();
        }

        #endregion

        #region Media
        
        public IPublishedContent Media(int id)
        {
            return _query == null
                ? ItemById(id, _mediaCache)
                : _query.Media(id);
        }
        
        public IEnumerable<IPublishedContent> Media(IEnumerable<int> ids)
        {
            return _query == null
                ? ItemsByIds(_mediaCache, ids)
                : _query.Media(ids);
        }

        public IEnumerable<IPublishedContent> MediaAtRoot()
        {
            return _query == null
                ? ItemsAtRoot(_mediaCache)
                : _query.MediaAtRoot();
        }


        #endregion

        #region Used by Content/Media

        private static IPublishedContent ItemById(int id, IPublishedCache cache)
        {
            var doc = cache.GetById(id);
            return doc;
        }

        private static IPublishedContent ItemByXPath(string xpath, XPathVariable[] vars, IPublishedCache cache)
        {
            var doc = cache.GetSingleByXPath(xpath, vars);
            return doc;
        }

        //NOTE: Not used?
        //private IPublishedContent ItemByXPath(XPathExpression xpath, XPathVariable[] vars, IPublishedCache cache)
        //{
        //    var doc = cache.GetSingleByXPath(xpath, vars);
        //    return doc;
        //}        

        private static IEnumerable<IPublishedContent> ItemsByIds(IPublishedCache cache, IEnumerable<int> ids)
        {
            return ids.Select(eachId => ItemById(eachId, cache)).WhereNotNull();
        }

        private static IEnumerable<IPublishedContent> ItemsByXPath(string xpath, XPathVariable[] vars, IPublishedCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private static IEnumerable<IPublishedContent> ItemsByXPath(XPathExpression xpath, XPathVariable[] vars, IPublishedCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private static IEnumerable<IPublishedContent> ItemsAtRoot(IPublishedCache cache)
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
        public IEnumerable<IPublishedContent> Search(string term, bool useWildCards = true, string searchProvider = null)
        {
            if (_query != null) return _query.Search(term, useWildCards, searchProvider);

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
        public IEnumerable<IPublishedContent> Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
        {
            if (_query != null) return _query.Search(criteria, searchProvider);

            var s = Examine.ExamineManager.Instance.DefaultSearchProvider;
            if (searchProvider != null)
                s = searchProvider;

            var results = s.Search(criteria);
            return results.ConvertSearchResultToPublishedContent(_contentCache);
        }

        #endregion
    }
}