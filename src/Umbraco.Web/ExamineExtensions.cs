﻿using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Examine.LuceneEngine.Providers;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Examine;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
    /// <summary>
    /// Extension methods for Examine.
    /// </summary>
    public static class ExamineExtensions
    {
        /// <summary>
        /// Creates an <see cref="IEnumerable{PublishedSearchResult}" /> containing all content from the <paramref name="cache" />.
        /// </summary>
        /// <param name="results">The search results.</param>
        /// <param name="cache">The cache to fetch the content from.</param>
        /// <returns>
        /// An <see cref="IEnumerable{PublishedSearchResult}" /> containing all content.
        /// </returns>
        /// <exception cref="ArgumentNullException">cache</exception>
        /// <remarks>
        /// Search results are skipped if it can't be fetched from the <paramref name="cache" /> by its integer id.
        /// </remarks>
        public static IEnumerable<PublishedSearchResult> ToPublishedSearchResults(this IEnumerable<ISearchResult> results, IPublishedCache cache)
        {
            if (cache == null) throw new ArgumentNullException(nameof(cache));

            var publishedSearchResults = new List<PublishedSearchResult>();

            foreach (var result in results)
            {
                if (int.TryParse(result.Id, out var contentId) &&
                    cache.GetById(contentId) is IPublishedContent content)
                {
                    publishedSearchResults.Add(new PublishedSearchResult(content, result.Score));
                }
            }

            return publishedSearchResults;
        }

        /// <summary>
        /// Creates an <see cref="IEnumerable{PublishedSearchResult}" /> containing all content, media or members from the <paramref name="snapshot" />.
        /// </summary>
        /// <param name="results">The search results.</param>
        /// <param name="snapshot">The snapshot.</param>
        /// <returns>
        /// An <see cref="IEnumerable{PublishedSearchResult}" /> containing all content, media or members.
        /// </returns>
        /// <exception cref="ArgumentNullException">snapshot</exception>
        /// <remarks>
        /// Search results are skipped if it can't be fetched from the respective cache by its integer id.
        /// </remarks>
        public static IEnumerable<PublishedSearchResult> ToPublishedSearchResults(this IEnumerable<ISearchResult> results, IPublishedSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            var publishedSearchResults = new List<PublishedSearchResult>();

            foreach (var result in results)
            {
                if (int.TryParse(result.Id, out var contentId) &&
                    result.Values.TryGetValue(LuceneIndex.CategoryFieldName, out var indexType))
                {
                    IPublishedContent content;
                    switch (indexType)
                    {
                        case IndexTypes.Content:
                            content = snapshot.Content.GetById(contentId);
                            break;
                        case IndexTypes.Media:
                            content = snapshot.Media.GetById(contentId);
                            break;
                        case IndexTypes.Member:
                            content = snapshot.Members.GetById(contentId);
                            break;
                        default:
                            continue;
                    }

                    if (content != null)
                    {
                        publishedSearchResults.Add(new PublishedSearchResult(content, result.Score));
                    }
                }
            }

            return publishedSearchResults;
        }
    }
}
