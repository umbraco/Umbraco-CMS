using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Examine;
using Examine.Search;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Xml;
using Umbraco.Core;
using Umbraco.Examine;
using Umbraco.Web.PublishedCache;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Web
{
    /// <summary>
    ///     A class used to query for published content, media items
    /// </summary>
    public class PublishedContentQuery : IPublishedContentQuery
    {
        private readonly IExamineManager _examineManager;
        private readonly IPublishedSnapshot _publishedSnapshot;
        private readonly IVariationContextAccessor _variationContextAccessor;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PublishedContentQuery" /> class.
        /// </summary>
        public PublishedContentQuery(IPublishedSnapshot publishedSnapshot,
            IVariationContextAccessor variationContextAccessor, IExamineManager examineManager)
        {
            _publishedSnapshot = publishedSnapshot ?? throw new ArgumentNullException(nameof(publishedSnapshot));
            _variationContextAccessor = variationContextAccessor ??
                                        throw new ArgumentNullException(nameof(variationContextAccessor));
            _examineManager = examineManager ?? throw new ArgumentNullException(nameof(examineManager));
        }

        #region Convert Helpers

        private static bool ConvertIdObjectToInt(object id, out int intId)
        {
            switch (id)
            {
                case string s:
                    return int.TryParse(s, out intId);

                case int i:
                    intId = i;
                    return true;

                default:
                    intId = default;
                    return false;
            }
        }

        private static bool ConvertIdObjectToGuid(object id, out Guid guidId)
        {
            switch (id)
            {
                case string s:
                    return Guid.TryParse(s, out guidId);

                case Guid g:
                    guidId = g;
                    return true;

                default:
                    guidId = default;
                    return false;
            }
        }
        private static bool ConvertIdObjectToUdi(object id, out Udi guidId)
        {
            switch (id)
            {
                case string s:
                    return UdiParser.TryParse(s, out guidId);

                case Udi u:
                    guidId = u;
                    return true;

                default:
                    guidId = default;
                    return false;
            }
        }

        #endregion

        #region Content

        public IPublishedContent Content(int id) => ItemById(id, _publishedSnapshot.Content);

        public IPublishedContent Content(Guid id) => ItemById(id, _publishedSnapshot.Content);

        public IPublishedContent Content(Udi id)
        {
            if (!(id is GuidUdi udi)) return null;
            return ItemById(udi.Guid, _publishedSnapshot.Content);
        }

        public IPublishedContent Content(object id)
        {
            if (ConvertIdObjectToInt(id, out var intId))
                return Content(intId);
            if (ConvertIdObjectToGuid(id, out var guidId))
                return Content(guidId);
            if (ConvertIdObjectToUdi(id, out var udiId))
                return Content(udiId);
            return null;
        }

        public IPublishedContent ContentSingleAtXPath(string xpath, params XPathVariable[] vars) =>
            ItemByXPath(xpath, vars, _publishedSnapshot.Content);

        public IEnumerable<IPublishedContent> Content(IEnumerable<int> ids) =>
            ItemsByIds(_publishedSnapshot.Content, ids);

        public IEnumerable<IPublishedContent> Content(IEnumerable<Guid> ids) =>
            ItemsByIds(_publishedSnapshot.Content, ids);

        public IEnumerable<IPublishedContent> Content(IEnumerable<object> ids)
        {
            return ids.Select(Content).WhereNotNull();
        }
        public IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars) =>
            ItemsByXPath(xpath, vars, _publishedSnapshot.Content);

        public IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars) =>
            ItemsByXPath(xpath, vars, _publishedSnapshot.Content);

        public IEnumerable<IPublishedContent> ContentAtRoot() => ItemsAtRoot(_publishedSnapshot.Content);

        #endregion

        #region Media

        public IPublishedContent Media(int id) => ItemById(id, _publishedSnapshot.Media);

        public IPublishedContent Media(Guid id) => ItemById(id, _publishedSnapshot.Media);

        public IPublishedContent Media(Udi id)
        {
            if (!(id is GuidUdi udi)) return null;
            return ItemById(udi.Guid, _publishedSnapshot.Media);
        }

        public IPublishedContent Media(object id)
        {
            if (ConvertIdObjectToInt(id, out var intId))
                return Media(intId);
            if (ConvertIdObjectToGuid(id, out var guidId))
                return Media(guidId);
            if (ConvertIdObjectToUdi(id, out var udiId))
                return Media(udiId);
            return null;
        }

        public IEnumerable<IPublishedContent> Media(IEnumerable<int> ids) => ItemsByIds(_publishedSnapshot.Media, ids);
        public IEnumerable<IPublishedContent> Media(IEnumerable<object> ids)
        {
            return ids.Select(Media).WhereNotNull();
        }

        public IEnumerable<IPublishedContent> Media(IEnumerable<Guid> ids) => ItemsByIds(_publishedSnapshot.Media, ids);

        public IEnumerable<IPublishedContent> MediaAtRoot() => ItemsAtRoot(_publishedSnapshot.Media);

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

        private static IEnumerable<IPublishedContent> ItemsByXPath(string xpath, XPathVariable[] vars,
            IPublishedCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private static IEnumerable<IPublishedContent> ItemsByXPath(XPathExpression xpath, XPathVariable[] vars,
            IPublishedCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private static IEnumerable<IPublishedContent> ItemsAtRoot(IPublishedCache cache) => cache.GetAtRoot();

        #endregion

        #region Search

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(string term, string culture = "*",
            string indexName = Constants.UmbracoIndexes.ExternalIndexName) =>
            Search(term, 0, 0, out _, culture, indexName);

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(string term, int skip, int take, out long totalRecords,
            string culture = "*", string indexName = Constants.UmbracoIndexes.ExternalIndexName)
        {
            if (skip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(skip), skip,
                    "The value must be greater than or equal to zero.");
            }

            if (take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take,
                    "The value must be greater than or equal to zero.");
            }

            if (string.IsNullOrEmpty(indexName))
            {
                indexName = Constants.UmbracoIndexes.ExternalIndexName;
            }

            if (!_examineManager.TryGetIndex(indexName, out var index) || !(index is IUmbracoIndex umbIndex))
            {
                throw new InvalidOperationException(
                    $"No index found by name {indexName} or is not of type {typeof(IUmbracoIndex)}");
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
                queryExecutor = query
                    .Field(UmbracoExamineFieldNames.VariesByCultureFieldName, "n") // Must not vary by culture
                    .And().ManagedQuery(term);
            }
            else
            {
                // Only search the specified culture
                var fields =
                    umbIndex.GetCultureAndInvariantFields(culture)
                        .ToArray(); // Get all index fields suffixed with the culture name supplied
                queryExecutor = query.ManagedQuery(term, fields);
            }

            var results = skip == 0 && take == 0
                ? queryExecutor.Execute()
                : queryExecutor.Execute(skip + take);

            totalRecords = results.TotalItemCount;

            return new CultureContextualSearchResults(
                results.Skip(skip).ToPublishedSearchResults(_publishedSnapshot.Content), _variationContextAccessor,
                culture);
        }

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(IQueryExecutor query) => Search(query, 0, 0, out _);

        /// <inheritdoc />
        public IEnumerable<PublishedSearchResult> Search(IQueryExecutor query, int skip, int take,
            out long totalRecords)
        {
            if (skip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(skip), skip,
                    "The value must be greater than or equal to zero.");
            }

            if (take < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(take), take,
                    "The value must be greater than or equal to zero.");
            }

            var results = skip == 0 && take == 0
                ? query.Execute()
                : query.Execute(skip + take);

            totalRecords = results.TotalItemCount;

            return results.Skip(skip).ToPublishedSearchResults(_publishedSnapshot);
        }

        /// <summary>
        ///     This is used to contextualize the values in the search results when enumerating over them so that the correct
        ///     culture values are used
        /// </summary>
        private class CultureContextualSearchResults : IEnumerable<PublishedSearchResult>
        {
            private readonly string _culture;
            private readonly IVariationContextAccessor _variationContextAccessor;
            private readonly IEnumerable<PublishedSearchResult> _wrapped;

            public CultureContextualSearchResults(IEnumerable<PublishedSearchResult> wrapped,
                IVariationContextAccessor variationContextAccessor, string culture)
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
                return new CultureContextualSearchResultsEnumerator(_wrapped.GetEnumerator(), _variationContextAccessor,
                    originalContext);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            /// <summary>
            ///     Resets the variation context when this is disposed
            /// </summary>
            private class CultureContextualSearchResultsEnumerator : IEnumerator<PublishedSearchResult>
            {
                private readonly VariationContext _originalContext;
                private readonly IVariationContextAccessor _variationContextAccessor;
                private readonly IEnumerator<PublishedSearchResult> _wrapped;

                public CultureContextualSearchResultsEnumerator(IEnumerator<PublishedSearchResult> wrapped,
                    IVariationContextAccessor variationContextAccessor, VariationContext originalContext)
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

                public bool MoveNext() => _wrapped.MoveNext();

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
