using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Examine;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for Examine
	/// </summary>
	internal static class ExamineExtensions
	{
		internal static IEnumerable<IPublishedContent> ConvertSearchResultToPublishedContent(
			this IEnumerable<SearchResult> results,
			ContextualPublishedCache cache)
		{
			//TODO: The search result has already returned a result which SHOULD include all of the data to create an IPublishedContent, 
			// however thsi is currently not the case: 
			// http://examine.codeplex.com/workitem/10350

			var list = new List<IPublishedContent>();
			
			foreach (var result in results.OrderByDescending(x => x.Score))
			{
				var doc = cache.GetById(result.Id);
				if (doc == null) continue; //skip if this doesn't exist in the cache				
				doc.Properties.Add(
					new PropertyResult("examineScore", result.Score.ToString(), Guid.Empty, PropertyResultType.CustomProperty));				
				list.Add(doc);
			}
			return list;
		}
	}
}