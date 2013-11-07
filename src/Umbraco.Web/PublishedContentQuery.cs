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
    public class PublishedContentQuery
    {
        private readonly ContextualPublishedContentCache _contentCache;
        private readonly ContextualPublishedMediaCache _mediaCache;

        public PublishedContentQuery(ContextualPublishedContentCache contentCache, ContextualPublishedMediaCache mediaCache)
        {
            _contentCache = contentCache;
            _mediaCache = mediaCache;
        }

        #region Content

        public IPublishedContent TypedContent(int id)
        {
            return TypedDocumentById(id, _contentCache);
        }

        public IPublishedContent TypedContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return TypedDocumentByXPath(xpath, vars, _contentCache);
        }
        
        public IEnumerable<IPublishedContent> TypedContent(IEnumerable<int> ids)
        {
            return TypedDocumentsbyIds(_contentCache, ids);
        }

        public IEnumerable<IPublishedContent> TypedContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return TypedDocumentsByXPath(xpath, vars, _contentCache);
        }

        public IEnumerable<IPublishedContent> TypedContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return TypedDocumentsByXPath(xpath, vars, _contentCache);
        }

        public IEnumerable<IPublishedContent> TypedContentAtRoot()
        {
            return TypedDocumentsAtRoot(_contentCache);
        }

        public dynamic Content(int id)
        {
            return DocumentById(id, _contentCache, DynamicNull.Null);
        }
        
        public dynamic ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return DocumentByXPath(xpath, vars, _contentCache, DynamicNull.Null);
        }

        public dynamic ContentSingleAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return DocumentByXPath(xpath, vars, _contentCache, DynamicNull.Null);
        }
        
        public dynamic Content(IEnumerable<int> ids)
        {
            return DocumentByIds(_contentCache, ids.ToArray());
        }

        public dynamic ContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return DocumentsByXPath(xpath, vars, _contentCache);
        }

        public dynamic ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return DocumentsByXPath(xpath, vars, _contentCache);
        }

        public dynamic ContentAtRoot()
        {
            return DocumentsAtRoot(_contentCache);
        }

        #endregion

        #region Media
        
        public IPublishedContent TypedMedia(int id)
        {
            return TypedDocumentById(id, _mediaCache);
        }
        
        public IEnumerable<IPublishedContent> TypedMedia(IEnumerable<int> ids)
        {
            return TypedDocumentsbyIds(_mediaCache, ids);
        }

        public IEnumerable<IPublishedContent> TypedMediaAtRoot()
        {
            return TypedDocumentsAtRoot(_mediaCache);
        }

        public dynamic Media(int id)
        {
            return DocumentById(id, _mediaCache, DynamicNull.Null);
        }
        
        public dynamic Media(IEnumerable<int> ids)
        {
            return DocumentByIds(_mediaCache, ids);
        }

        public dynamic MediaAtRoot()
        {
            return DocumentsAtRoot(_mediaCache);
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

        private IEnumerable<IPublishedContent> TypedDocumentsbyIds(ContextualPublishedCache cache, IEnumerable<int> ids)
        {
            return ids.Select(eachId => TypedDocumentById(eachId, cache));
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
            return new DynamicPublishedContentList(
                TypedSearch(term, useWildCards, searchProvider));
        }

        /// <summary>
        /// Searhes content
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        public dynamic Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
        {
            return new DynamicPublishedContentList(
                TypedSearch(criteria, searchProvider));
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
            var s = Examine.ExamineManager.Instance.DefaultSearchProvider;
            if (searchProvider != null)
                s = searchProvider;

            var results = s.Search(criteria);
            return results.ConvertSearchResultToPublishedContent(_contentCache);
        }

        #endregion
    }
}