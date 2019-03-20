using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Examine;
using Examine.Search;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;
using Umbraco.Examine;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
    /// <summary>
    /// A class used to query for published content, media items
    /// </summary>
    public class PublishedContentQuery : IPublishedContentQuery
    {
        private readonly IPublishedSnapshot _publishedSnapshot;
        private readonly IVariationContextAccessor _variationContextAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentQuery"/> class.
        /// </summary>
        public PublishedContentQuery(IPublishedSnapshot publishedSnapshot, IVariationContextAccessor variationContextAccessor)
        {
            _publishedSnapshot = publishedSnapshot ?? throw new ArgumentNullException(nameof(publishedSnapshot));
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
        }

        #region Content

        public IPublishedContent Content(int id)
        {
            return ItemById(id, _publishedSnapshot.Content);
        }

        public IPublishedContent Content(Guid id)
        {
            return ItemById(id, _publishedSnapshot.Content);
        }

        public IPublishedContent Content(Udi id)
        {
            if (!(id is GuidUdi udi)) return null;
            return ItemById(udi.Guid, _publishedSnapshot.Content);
        }

        public IPublishedContent ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ItemByXPath(xpath, vars, _publishedSnapshot.Content);
        }

        public IEnumerable<IPublishedContent> Content(IEnumerable<int> ids)
        {
            return ItemsByIds(_publishedSnapshot.Content, ids);
        }

        public IEnumerable<IPublishedContent> Content(IEnumerable<Guid> ids)
        {
            return ItemsByIds(_publishedSnapshot.Content, ids);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ItemsByXPath(xpath, vars, _publishedSnapshot.Content);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return ItemsByXPath(xpath, vars, _publishedSnapshot.Content);
        }

        public IEnumerable<IPublishedContent> ContentAtRoot()
        {
            return ItemsAtRoot(_publishedSnapshot.Content);
        }

        #endregion

        #region Media

        public IPublishedContent Media(int id)
        {
            return ItemById(id, _publishedSnapshot.Media);
        }

        public IPublishedContent Media(Guid id)
        {
            return ItemById(id, _publishedSnapshot.Media);
        }

        public IPublishedContent Media(Udi id)
        {
            if (!(id is GuidUdi udi)) return null;
            return ItemById(udi.Guid, _publishedSnapshot.Media);
        }

        public IEnumerable<IPublishedContent> Media(IEnumerable<int> ids)
        {
            return ItemsByIds(_publishedSnapshot.Media, ids);
        }

        public IEnumerable<IPublishedContent> Media(IEnumerable<Guid> ids)
        {
            return ItemsByIds(_publishedSnapshot.Media, ids);
        }

        public IEnumerable<IPublishedContent> MediaAtRoot()
        {
            return ItemsAtRoot(_publishedSnapshot.Media);
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
        public IEnumerable<PublishedSearchResult> Search(string term, string culture = null, string indexName = null)
        {
            return Search(term, 0, 0, out _, culture, indexName);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(string term, int skip, int take, out long totalRecords, string culture = null, string indexName = null)
        {
            indexName = string.IsNullOrEmpty(indexName)
                ? Constants.UmbracoIndexes.ExternalIndexName
                : indexName;

            if (!ExamineManager.Instance.TryGetIndex(indexName, out var index) || !(index is IUmbracoIndex umbIndex))
                throw new InvalidOperationException($"No index found by name {indexName} or is not of type {typeof(IUmbracoIndex)}");

            var searcher = umbIndex.GetSearcher();

            // default to max 500 results
            var count = skip == 0 && take == 0 ? 500 : skip + take;

            //set this to the specific culture or to the culture in the request
            culture = culture ?? _variationContextAccessor.VariationContext.Culture;

            ISearchResults results;
            if (culture.IsNullOrWhiteSpace())
            {
                results = searcher.Search(term, count);
            }
            else
            {
                //get all index fields suffixed with the culture name supplied
                var cultureFields = umbIndex.GetCultureFields(culture);
                var qry = searcher.CreateQuery().Field(UmbracoContentIndex.VariesByCultureFieldName, "y"); //must vary by culture
                qry = qry.And().ManagedQuery(term, cultureFields.ToArray());
                results = qry.Execute(count);
            }

            totalRecords = results.TotalItemCount;

            return new CultureContextualSearchResults(results.ToPublishedSearchResults(_publishedSnapshot.Content), _variationContextAccessor, culture);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(IQueryExecutor query)
        {
            return Search(query, 0, 0, out _);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(IQueryExecutor query, int skip, int take, out long totalRecords)
        {
            var results = skip == 0 && take == 0
                ? query.Execute()
                : query.Execute(maxResults: skip + take);

            totalRecords = results.TotalItemCount;
            return results.ToPublishedSearchResults(_publishedSnapshot.Content);
        }

        /// <summary>
        /// This is used to contextualize the values in the search results when enumerating over them so that the correct culture values are used
        /// </summary>
        private class CultureContextualSearchResults : IEnumerable<PublishedSearchResult>
        {
            private readonly IEnumerable<PublishedSearchResult> _wrapped;
            private readonly IVariationContextAccessor _variationContextAccessor;
            private readonly string _culture;

            public CultureContextualSearchResults(IEnumerable<PublishedSearchResult> wrapped, IVariationContextAccessor variationContextAccessor, string culture)
            {
                _wrapped = wrapped;
                _variationContextAccessor = variationContextAccessor;
                _culture = culture;
            }

            public IEnumerator<PublishedSearchResult> GetEnumerator()
            {
                //We need to change the current culture to what is requested and then change it back
                var originalContext = _variationContextAccessor.VariationContext;
                if (!_culture.IsNullOrWhiteSpace() && !_culture.InvariantEquals(originalContext.Culture))
                    _variationContextAccessor.VariationContext = new VariationContext(_culture);

                //now the IPublishedContent returned will be contextualized to the culture specified and will be reset when the enumerator is disposed
                return new CultureContextualSearchResultsEnumerator(_wrapped.GetEnumerator(), _variationContextAccessor, originalContext);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            /// <summary>
            /// Resets the variation context when this is disposed
            /// </summary>
            private class CultureContextualSearchResultsEnumerator : IEnumerator<PublishedSearchResult>
            {
                private readonly IEnumerator<PublishedSearchResult> _wrapped;
                private readonly IVariationContextAccessor _variationContextAccessor;
                private readonly VariationContext _originalContext;

                public CultureContextualSearchResultsEnumerator(IEnumerator<PublishedSearchResult> wrapped, IVariationContextAccessor variationContextAccessor, VariationContext originalContext)
                {
                    _wrapped = wrapped;
                    _variationContextAccessor = variationContextAccessor;
                    _originalContext = originalContext;
                }

                public void Dispose()
                {
                    _wrapped.Dispose();
                    //reset
                    _variationContextAccessor.VariationContext = _originalContext;
                }

                public bool MoveNext()
                {
                    return _wrapped.MoveNext();
                }

                public void Reset()
                {
                    _wrapped.Reset();
                }

                public PublishedSearchResult Current => _wrapped.Current;
                object IEnumerator.Current => Current;
            }
        }

        


        #endregion
    }
}
