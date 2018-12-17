using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.XPath;
using Examine;
using Examine.Search;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;
using Umbraco.Examine;
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

            ISearchResults results;
            if (!culture.IsNullOrWhiteSpace())
            {
                results = searcher.Search(term, (skip == 0 && take == 0
                    ? 500 //default max results
                    : skip + take));
            }
            else
            {
                //get all index fields suffixed with the culture name supplied
                var cultureFields = new List<string>();
                var fields = umbIndex.GetFields();
                var qry = searcher.CreateQuery().Field(UmbracoContentIndex.VariesByCultureFieldName, 1); //must vary by culture
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var field in fields)
                {
                    var match = CultureIsoCodeFieldName.Match(field);
                    if (match.Success && match.Groups.Count == 2 && culture.InvariantEquals(match.Groups[1].Value))
                        cultureFields.Add(field);
                }

                results = qry.And().ManagedQuery(term, cultureFields.ToArray()).Execute((skip == 0 && take == 0
                    ? 500 //default max results
                    : skip + take));

            }

            totalRecords = results.TotalItemCount;
            return results.ToPublishedSearchResults(_contentCache);
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
            return results.ToPublishedSearchResults(_contentCache);
        }

        /// <summary>
        /// Matches a culture iso name suffix
        /// </summary>
        /// <remarks>
        /// myFieldName_en-us will match the "en-us"
        /// </remarks>
        private static readonly Regex CultureIsoCodeFieldName = new Regex("^[_\\w]+_([a-z]{2}-[a-z0-9]{2,4})$", RegexOptions.Compiled);


        #endregion
    }
}
