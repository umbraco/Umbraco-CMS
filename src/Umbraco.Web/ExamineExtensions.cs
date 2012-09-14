using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Examine;
using Umbraco.Core.Dynamics;

namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for Examine
	/// </summary>
	internal static class ExamineExtensions
	{
		internal static DynamicDocumentList ConvertSearchResultToDynamicDocument(
			this IEnumerable<SearchResult> results,
			IPublishedStore store)
		{
			var list = new DynamicDocumentList();
			var xd = new XmlDocument();

			foreach (var result in results.OrderByDescending(x => x.Score))
			{
				var doc = store.GetDocumentById(
					UmbracoContext.Current,
					result.Id);
				if (doc == null) continue; //skip if this doesn't exist in the cache				
				doc.Properties.Add(
					new PropertyResult("examineScore", result.Score.ToString(), Guid.Empty, PropertyResultType.CustomProperty));
				var dynamicDoc = new DynamicDocument(doc);
				list.Add(dynamicDoc);
			}
			return list;
		}
	}
}