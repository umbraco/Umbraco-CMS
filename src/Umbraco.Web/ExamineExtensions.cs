using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for Examine
	/// </summary>
	internal static class ExamineExtensions
	{
        internal static IEnumerable<IPublishedContent> ConvertSearchResultToPublishedContent(this IEnumerable<SearchResult> results, IPublishedCache cache)
		{
            // no need to think about creating the IPublishedContent from the Examine result
            // content cache is fast and optimized - use it!

		    var list = new List<IPublishedContent>();

			foreach (var result in results.OrderByDescending(x => x.Score))
			{
				var content = cache.GetById(result.Id);
				if (content == null) continue; // skip if this doesn't exist in the cache

                // need to extend the content as we're going to add a property to it,
                // and we should not ever do it to the content we get from the cache,
                // precisely because it is cached and shared by all requests.

                // but we cannot wrap it because we need to respect the type that was
                // returned by the cache, in case the cache can create real types.
                // so we have to ask it to please extend itself.

			    var extend = PublishedContentExtended.Extend(content);
                list.Add(extend.CreateModel()); // take care, must create the model!

                var property = new PropertyResult("examineScore",
                    result.Score,
			        PropertyResultType.CustomProperty);

                extend.AddProperty(property);
			}

            return list;
		}
	}
}