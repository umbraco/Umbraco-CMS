using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
    /// <summary>
    /// Extension methods for Examine
    /// </summary>
    public static class ExamineExtensions
    {
        public static IEnumerable<PublishedSearchResult> ToPublishedSearchResults(this IEnumerable<ISearchResult> results, IPublishedCache cache)
        {
            var list = new List<PublishedSearchResult>();

            foreach (var result in results.OrderByDescending(x => x.Score))
            {
                if (!int.TryParse(result.Id, out var intId)) continue; //invalid
                var content = cache.GetById(intId);
                if (content == null) continue; // skip if this doesn't exist in the cache

                list.Add(new PublishedSearchResult(content, result.Score));

            }

            return list;
        }
    }
}
