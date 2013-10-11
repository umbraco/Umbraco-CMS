using System;
using System.Collections.Generic;
using System.Linq;
using Examine;
using Umbraco.Core.Dynamics;
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
        internal static PublishedContentSet<IPublishedContent> ConvertSearchResultToPublishedContent(this IEnumerable<SearchResult> results,
			ContextualPublishedCache cache)
		{
			//TODO: The search result has already returned a result which SHOULD include all of the data to create an IPublishedContent, 
			// however this is currently not the case: 
			// http://examine.codeplex.com/workitem/10350

		    var list = new List<IPublishedContent>();
            var set = new PublishedContentSet<IPublishedContent>(list);
			
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

                list.Add(content);
			    var extend = set.MapContent(content);

			    var property = new PropertyResult("examineScore",
                    result.Score,
			        PropertyResultType.CustomProperty);
                extend.AddProperty(property);
			}

            return set;
		}
	}
}