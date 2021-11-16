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
    public class PublishedContentQuery : IPublishedContentQuery2
    {
        private readonly IPublishedSnapshot _publishedSnapshot;
        private readonly IVariationContextAccessor _variationContextAccessor;
        private readonly IExamineManager _examineManager;

        [Obsolete("Use the constructor with all parameters instead")]
        public PublishedContentQuery(IPublishedSnapshot publishedSnapshot, IVariationContextAccessor variationContextAccessor)
            : this (publishedSnapshot, variationContextAccessor, ExamineManager.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedContentQuery"/> class.
        /// </summary>
        public PublishedContentQuery(IPublishedSnapshot publishedSnapshot, IVariationContextAccessor variationContextAccessor, IExamineManager examineManager)
        {
            _publishedSnapshot = publishedSnapshot ?? throw new ArgumentNullException(nameof(publishedSnapshot));
            _variationContextAccessor = variationContextAccessor ?? throw new ArgumentNullException(nameof(variationContextAccessor));
            _examineManager = examineManager ?? throw new ArgumentNullException(nameof(examineManager));
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
        public IEnumerable<PublishedSearchResult> Search(string term, string culture = "*", string indexName = Constants.UmbracoIndexes.ExternalIndexName)
        {
            return Search(term, 0, 0, out _, culture, indexName);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(string term, int skip, int take, out long totalRecords, string culture = "*", string indexName = Constants.UmbracoIndexes.ExternalIndexName)
            => Search(term, skip, take, out totalRecords, culture, indexName, null);

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(string term, int skip, int take, out long totalRecords, string culture = "*", string indexName = Constants.UmbracoIndexes.ExternalIndexName, ISet<string> loadedFields = null)
        {
            if (skip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(skip), skip, "The value must be greater than or equal to zero.");
            }

            if (take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take, "The value must be greater than or equal to zero.");
            }

            if (string.IsNullOrEmpty(indexName))
            {
                indexName = Constants.UmbracoIndexes.ExternalIndexName;
            }

            if (!_examineManager.TryGetIndex(indexName, out var index) || !(index is IUmbracoIndex umbIndex))
            {
                throw new InvalidOperationException($"No index found by name {indexName} or is not of type {typeof(IUmbracoIndex)}");
            }

            var query = umbIndex.GetSearcher().CreateQuery(IndexTypes.Content);
            
            IQueryExecutor queryExecutor;
            if (culture == "*")
            {
                // Search everything
                queryExecutor = query.ManagedQuery(term);
            }
            else if (string.IsNullOrWhiteSpace(culture))
            {
                // Only search invariant
                queryExecutor = query.Field(UmbracoContentIndex.VariesByCultureFieldName, "n") // Must not vary by culture
                    .And().ManagedQuery(term);
            }
            else
            {
                // Only search the specified culture
                var fields = umbIndex.GetCultureAndInvariantFields(culture).ToArray(); // Get all index fields suffixed with the culture name supplied
                queryExecutor = query.ManagedQuery(term, fields);
            }
            if (loadedFields != null && queryExecutor is IBooleanOperation booleanOperation)
            {
                queryExecutor = booleanOperation.SelectFields(loadedFields);
            }

            var results = skip == 0 && take == 0
                ? queryExecutor.Execute()
                : queryExecutor.Execute(skip + take);

            totalRecords = results.TotalItemCount;

            return new CultureContextualSearchResults(results.Skip(skip).ToPublishedSearchResults(_publishedSnapshot.Content), _variationContextAccessor, culture);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(IQueryExecutor query)
        {
            return Search(query, 0, 0, out _);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(IQueryExecutor query, int skip, int take, out long totalRecords)
        {
            if (skip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(skip), skip, "The value must be greater than or equal to zero.");
            }

            if (take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take, "The value must be greater than or equal to zero.");
            }

            var results = skip == 0 && take == 0
                ? query.Execute()
                : query.Execute(skip + take);

            totalRecords = results.TotalItemCount;

            return results.Skip(skip).ToPublishedSearchResults(_publishedSnapshot);
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
