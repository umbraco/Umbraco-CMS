using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.XPath;
using Examine;
using Examine.LuceneEngine.Providers;
using Examine.LuceneEngine.SearchCriteria;
using Examine.SearchCriteria;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
    using Examine = global::Examine;

    /// <summary>
    /// A class used to query for published content, media items
    /// </summary>
    public class PublishedContentQuery : IPublishedContentQuery
    {
        private readonly IPublishedContentCache _contentCache;
        private readonly IPublishedMediaCache _mediaCache;

        /// <summary>
        /// Constructor used to return results from the caches
        /// </summary>
        /// <param name="contentCache"></param>
        /// <param name="mediaCache"></param>
        public PublishedContentQuery(IPublishedContentCache contentCache, IPublishedMediaCache mediaCache)
        {
            _contentCache = contentCache ?? throw new ArgumentNullException(nameof(contentCache));
            _mediaCache = mediaCache ?? throw new ArgumentNullException(nameof(mediaCache));
        }

        #region Content

        public IPublishedContent Content(int id)
        {
            return ItemById(id, _contentCache);
        }

        public IPublishedContent Content(Guid id)
        {
            return ItemById(id, _contentCache);
        }

        public IPublishedContent Content(Udi id)
        {
            if (!(id is GuidUdi udi)) return null;
            return ItemById(udi.Guid, _contentCache);
        }

        public IPublishedContent ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ItemByXPath(xpath, vars, _contentCache);
        }

        public IEnumerable<IPublishedContent> Content(IEnumerable<int> ids)
        {
            return ItemsByIds(_contentCache, ids);
        }

        public IEnumerable<IPublishedContent> Content(IEnumerable<Guid> ids)
        {
            return ItemsByIds(_contentCache, ids);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ItemsByXPath(xpath, vars, _contentCache);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return ItemsByXPath(xpath, vars, _contentCache);
        }

        public IEnumerable<IPublishedContent> ContentAtRoot()
        {
            return ItemsAtRoot(_contentCache);
        }

        #endregion

        #region Media

        public IPublishedContent Media(int id)
        {
            return ItemById(id, _mediaCache);
        }

        public IPublishedContent Media(Guid id)
        {
            return ItemById(id, _mediaCache);
        }

        public IPublishedContent Media(Udi id)
        {
            if (!(id is GuidUdi udi)) return null;
            return ItemById(udi.Guid, _mediaCache);
        }

        public IEnumerable<IPublishedContent> Media(IEnumerable<int> ids)
        {
            return ItemsByIds(_mediaCache, ids);
        }

        public IEnumerable<IPublishedContent> Media(IEnumerable<Guid> ids)
        {
            return ItemsByIds(_mediaCache, ids);
        }

        public IEnumerable<IPublishedContent> MediaAtRoot()
        {
            return ItemsAtRoot(_mediaCache);
        }


        #endregion

        #region Used by Content/Media

        private static IPublishedContent ItemById(int id, IPublishedCache cache)
        {
            var doc = cache.GetById(id);
            return doc;
        }

        private static IPublishedContent ItemById(Guid id, IPublishedCache cache)
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

        private IEnumerable<IPublishedContent> ItemsByIds(IPublishedCache cache, IEnumerable<Guid> ids)
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

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(string term, bool useWildCards = true, string indexName = null)
        {
            return Search(0, 0, out _, term, useWildCards, indexName);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(int skip, int take, out int totalRecords, string term, bool useWildCards = true, string indexName = null)
        {
            //TODO: Can we inject IExamineManager?

            var indexer = string.IsNullOrEmpty(indexName)
                ? Examine.ExamineManager.Instance.GetIndexer(Constants.Examine.ExternalIndexer)
                : Examine.ExamineManager.Instance.GetIndexer(indexName);

            if (indexer == null) throw new InvalidOperationException("No index found by name " + indexName);

            var searcher = indexer.GetSearcher();

            if (skip == 0 && take == 0)
            {
                var results = searcher.Search(term, useWildCards);
                totalRecords = results.TotalItemCount;
                return results.ToPublishedSearchResults(_contentCache);
            }

            var criteria = SearchAllFields(term, useWildCards, searcher, indexer);
            return Search(skip, take, out totalRecords, criteria, searcher);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(ISearchCriteria criteria, Examine.ISearcher searchProvider = null)
        {
            return Search(0, 0, out _, criteria, searchProvider);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(int skip, int take, out int totalRecords, ISearchCriteria criteria, Examine.ISearcher searchProvider = null)
        {

            //TODO: Can we inject IExamineManager?

            var searcher = searchProvider ?? Examine.ExamineManager.Instance.GetSearcher(Constants.Examine.ExternalIndexer);

            var results = skip == 0 && take == 0
                ? searcher.Search(criteria)
                : searcher.Search(criteria, maxResults: skip + take);

            totalRecords = results.TotalItemCount;
            return results.ToPublishedSearchResults(_contentCache);
        }

        /// <summary>
        /// Creates an ISearchCriteria for searching all fields in a <see cref="BaseLuceneSearcher"/>.
        /// </summary>
        private ISearchCriteria SearchAllFields(string searchText, bool useWildcards, Examine.ISearcher searcher, Examine.IIndexer indexer)
        {
            var sc = searcher.CreateCriteria();

            //if we're dealing with a lucene searcher, we can get all of it's indexed fields,
            //else we can get the defined fields for the index. 
            var searchFields = (searcher is BaseLuceneSearcher luceneSearcher)
                ? luceneSearcher.GetAllIndexedFields()
                : indexer.FieldDefinitionCollection.Keys;

            //this is what Examine does internally to create ISearchCriteria for searching all fields
            var strArray = searchText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            sc = useWildcards == false
                ? sc.GroupedOr(searchFields, strArray).Compile()
                : sc.GroupedOr(searchFields, strArray.Select(x => (IExamineValue)new ExamineValue(Examineness.ComplexWildcard, x.MultipleCharacterWildcard().Value)).ToArray()).Compile();
            return sc;
        }        


        #endregion
    }
}
